// SkillSystem.cs — 스킬 장착/시전/쿨다운 (문서 02-4). A/B 각 액티브3 + 패시브1.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;
using BroDungeon.Utilities;

namespace BroDungeon.Combat
{
    /// <summary>한 캐릭터의 스킬 슬롯(액티브 최대3 + 패시브1) 관리.</summary>
    public class SkillSystem : MonoBehaviour
    {
        public ICombatant owner;
        public int maxActiveSlots = 2; // 성장에 따라 3으로 해금
        public ProjectilePool projectilePool;
        public LayerMask enemyMask;

        readonly List<string> _activeSkills = new List<string>();
        string _passiveSkill;
        readonly Dictionary<string, Cooldown> _cooldowns = new Dictionary<string, Cooldown>();

        public float cooldownReduction = 0f; // 집중/시간 속성

        public bool Equip(string skillId)
        {
            var def = SkillTable.Get(skillId);
            if (def == null) return false;
            if (def.Owner == SkillOwner.A_Passive || def.Owner == SkillOwner.B_Passive)
            { _passiveSkill = skillId; return true; }
            if (_activeSkills.Count >= maxActiveSlots) return false;
            _activeSkills.Add(skillId);
            _cooldowns[skillId] = new Cooldown();
            return true;
        }

        public void Unequip(string skillId)
        {
            _activeSkills.Remove(skillId);
            _cooldowns.Remove(skillId);
            if (_passiveSkill == skillId) _passiveSkill = null;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            foreach (var cd in _cooldowns.Values) cd.Tick(dt);
        }

        public bool IsReady(string skillId) => _cooldowns.TryGetValue(skillId, out var cd) && cd.Ready;
        public float CooldownRatio(string skillId) => _cooldowns.TryGetValue(skillId, out var cd) ? cd.Ratio : 0f;

        /// 슬롯 인덱스로 시전.
        public bool CastSlot(int index, Vector2 aim)
        {
            if (index < 0 || index >= _activeSkills.Count) return false;
            return Cast(_activeSkills[index], aim);
        }

        public bool Cast(string skillId, Vector2 aim)
        {
            if (owner == null) return false;
            var def = SkillTable.Get(skillId);
            if (def == null) return false;
            if (!IsReady(skillId)) return false;
            if (owner.Status != null && owner.Status.Silenced) return false;
            // 저주 속박의 목걸이(CB04): 분리 중 B 스킬 봉인
            if (owner is Characters.MageController mb && mb.SkillLockWhenSeparated && mb.IsSeparatedState) return false;

            // B 스킬은 마나 소비 (저주 장비의 마나 소비 배율 반영)
            if (owner is Characters.MageController mage
                && !mage.TrySpendMana(def.ManaCost * mage.ManaCostMultiplier)) return false;

            ExecuteEffect(def, aim);

            // 쿨감: 스킬시스템 자체 값 + 캐릭터 스탯(쿨감 속성/장비) 합산
            float ownerCdr = (owner as Characters.CharacterBase)?.Stats.CooldownReduction ?? 0f;
            float totalCdr = Mathf.Clamp(cooldownReduction + ownerCdr, 0f, 0.8f);
            _cooldowns[skillId].Trigger(def.Cooldown * (1f - totalCdr));
            return true;
        }

        void ExecuteEffect(SkillDef def, Vector2 aim)
        {
            var origin = owner.Transform.position;

            // 투사체: B 액티브 공격/투사체 타입은 발사체로 처리
            bool isProjectile = def.Type == SkillType.Projectile
                || (def.Type == SkillType.Attack && def.Owner == SkillOwner.B_Active);
            if (isProjectile)
            {
                var preq = BuildRequest(def);
                Vector2 dir = (aim - (Vector2)origin).normalized;
                projectilePool?.Fire(owner, preq, origin, dir, enemyMask,
                    pierce: def.ChainCount, bounce: def.Bounce);
                return;
            }

            switch (def.Type)
            {
                case SkillType.Area:
                case SkillType.CC:
                {
                    var req = BuildRequest(def);
                    CombatManager.Instance.AreaAttack(aim, def.AreaRadius, req, enemyMask);
                    break;
                }
                case SkillType.Heal:
                    owner.Heal(def.HealAmount); break;
                case SkillType.Defense:
                    owner.ApplyStatus(new StatusInstance { Type = StatusType.Shield, Duration = def.Duration }); break;
                case SkillType.Buff:
                    owner.ApplyStatus(new StatusInstance { Type = StatusType.Haste, Duration = def.Duration, Magnitude = def.Magnitude }); break;
                default:
                {
                    // Dash/Move/Util 등은 캐릭터 컨트롤러가 처리하도록 이벤트 위임
                    var req = BuildRequest(def);
                    CombatManager.Instance.AreaAttack(origin, def.AreaRadius > 0 ? def.AreaRadius : 1.5f, req, enemyMask);
                    break;
                }
            }
        }

        AttackRequest BuildRequest(SkillDef def)
        {
            var cb = owner as Characters.CharacterBase;
            return new AttackRequest
            {
                Source = owner,
                BaseAtk = cb != null ? cb.FinalAttackBase * cb.ExternalAtkMultiplier : 0f,
                AltarBonus = cb != null ? cb.AltarAttackBonus : 0f,
                SkillMul = def.DamageMul,
                AttributeBonus = cb != null ? cb.AttributeSynergyBonus : 0f,
                BondBonus = cb != null ? cb.BondBonus : 0f,
                CritChance = cb != null ? cb.CritChanceTotal : 0.05f,
                CritDamage = cb != null ? cb.CritDamageTotal : 1.5f,
                PierceRatio = cb != null ? cb.PierceTotal : 0f,
                Attribute = def.Attribute
            };
        }

        public IReadOnlyList<string> ActiveSkills => _activeSkills;
        public string PassiveSkill => _passiveSkill;
    }
}
