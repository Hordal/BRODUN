// CombatManager.cs — 공격 판정/데미지 적용 중앙 처리 (호스트 권한, 문서 06-5)
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Combat
{
    /// <summary>한 번의 공격 요청. 무기/스킬 → CombatManager로 전달.</summary>
    public struct AttackRequest
    {
        public ICombatant Source;
        public ICombatant Target;
        public float BaseAtk, GearAtk, AltarBonus;
        public float SkillMul, AttributeBonus, BondBonus;
        public float CritChance, CritDamage; // CritDamage 예: 1.5 = 150%
        public float PierceRatio;
        public AttributeType Attribute;
    }

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        /// 단일 대상 공격 처리. 멀티 시 호스트만 호출(문서 01-4 호스트 권한).
        public float ProcessAttack(in AttackRequest req)
        {
            if (req.Target == null || !req.Target.IsAlive) return 0f;
            if (req.Target.Status != null && req.Target.Status.Invulnerable) return 0f;

            CombatState.Mark(); // 전투 상태 갱신(저주 비전투 드레인 등 조건부 효과용)
            bool crit = DamageFormula.RollCrit(req.CritChance);
            var ctx = new DamageContext
            {
                BaseAtk = req.BaseAtk,
                GearAtk = req.GearAtk,
                AltarBonus = req.AltarBonus,
                SkillMul = req.SkillMul,
                AttributeBonus = req.AttributeBonus,
                BondBonus = req.BondBonus,
                CritMul = crit ? Mathf.Max(1f, req.CritDamage) : 1f,
                // 저주(방어 감소)는 ICombatant.Defense 구현에서 이미 반영됨.
                TargetDefense = req.Target.Defense,
                PierceRatio = req.PierceRatio
            };
            float dmg = DamageFormula.Calculate(in ctx);
            req.Target.TakeDamage(dmg, req.Attribute, req.Source);

            ApplyOnHitStatus(req.Target, req.Attribute);
            EventBus.Publish(new DamageDealtEvent { Source = req.Source, Target = req.Target, Amount = dmg, Crit = crit });
            return dmg;
        }

        /// 속성별 기본 상태이상 부여(문서 02-1 장비 부여 효과).
        void ApplyOnHitStatus(ICombatant target, AttributeType attr)
        {
            switch (attr)
            {
                case AttributeType.Fire:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Burn, Duration = 3f, TickInterval = 1f, TickDamage = 5f }); break;
                case AttributeType.Ice:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Slow, Duration = 2f, Magnitude = 0.3f }); break;
                case AttributeType.Lightning:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Shock, Duration = 2f }); break;
                case AttributeType.Poison:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Poison, Duration = 4f, TickInterval = 1f, TickDamage = 4f }); break;
                case AttributeType.Curse:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Curse, Duration = 5f, Magnitude = 0.3f }); break;
                case AttributeType.Rock:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Stun, Duration = 1f }); break;
                case AttributeType.Wind:
                    target.ApplyStatus(new StatusInstance { Type = StatusType.Pull, Duration = 0.2f }); break;
            }
        }

        /// 반경 내 모든 적 (Area 스킬/폭발용). 레이어/태그 기반.
        public void AreaAttack(Vector3 center, float radius, AttackRequest template, LayerMask enemyMask)
        {
            var hits = Physics2D.OverlapCircleAll(center, radius, enemyMask);
            foreach (var h in hits)
            {
                var c = h.GetComponentInParent<ICombatant>();
                if (c == null || !c.IsAlive) continue;
                var req = template; req.Target = c;
                ProcessAttack(in req);
            }
        }
    }
}
