// CursedItemHandler.cs — 저주 장비 12종 전용 효과/디메리트 (문서 02-7).
// 강력한 효과 + 심각한 디메리트. 일반 스탯으로 표현 불가한 부분을 전담.
// 이로운 일반 스탯(공+40% 등)은 EquipmentSystem이 BaseEffect/Special 파싱으로 이미 적용.
// 여기서는: (1) 파싱 불가 이로운 효과, (2) 스탯 기반 디메리트(공급자), (3) 틱/조건 디메리트.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;
using BroDungeon.Characters;
using BroDungeon.Utilities;

namespace BroDungeon.Items
{
    public class CursedItemHandler : MonoBehaviour
    {
        public WarriorController a;
        public MageController b;
        public MergeSystem merge;
        public EquipmentSystem equipment;
        public LootSystem loot;

        // 현재 장착 중인 저주 장비 id 집합
        readonly HashSet<string> _active = new HashSet<string>();

        // 스탯 기반 디메리트/이로운효과(공급자로 EquipmentSystem에 합산)
        readonly List<StatModifier> _statA = new List<StatModifier>();
        readonly List<StatModifier> _statB = new List<StatModifier>();

        // 틱 디메리트
        bool _drainHpA;            // CA01 비전투 HP 1%/초↓
        float _silenceInterval;    // CB06 3분마다 스킬 봉인
        float _silenceTimer;

        bool _registered;
        bool _ca04Active; // 고독의 팔찌 활성

        void Start() => EnsureRegistered();   // 참조 와이어링 이후 등록
        void OnDestroy()
        {
            if (_registered)
            {
                equipment?.UnregisterProvider(ProvideMods);
                EventBus.Unsubscribe<MergeStateChangedEvent>(OnMergeChanged);
            }
        }

        /// 참조가 설정된 뒤 공급자 등록(Awake 순서 문제 회피).
        public void EnsureRegistered()
        {
            if (_registered || equipment == null) return;
            equipment.RegisterProvider(ProvideMods);
            EventBus.Subscribe<MergeStateChangedEvent>(OnMergeChanged);
            _registered = true;
        }

        // CA04: 합체/분리에 따라 A 공격 보너스 토글.
        void OnMergeChanged(MergeStateChangedEvent e)
        {
            if (a == null) return;
            a.CursedConditionalAtkPct = _ca04Active
                ? (e.State == MergeState.Separated ? 0.5f : -0.2f) : 0f;
        }

        IEnumerable<StatModifier> ProvideMods(CharacterType who)
            => who == CharacterType.A_Warrior ? _statA : _statB;

        public void OnEquipped(string id)
        {
            EnsureRegistered();
            if (!_active.Add(id)) return;
            ApplySpec(id, true);
            equipment?.Recalculate();
        }

        public void OnUnequipped(string id)
        {
            if (!_active.Remove(id)) return;
            ApplySpec(id, false);
            equipment?.Recalculate();
        }

        // sign: true=적용, false=해제(역연산)
        void ApplySpec(string id, bool on)
        {
            switch (id)
            {
                case "CA01": // 폭식의 대검: (이로움 공+40% 파싱됨) / 비전투 HP1%/초↓
                    _drainHpA = on; break;

                case "CA02": // 피의 갑옷: (방+35% 파싱됨) / 회복 50%↓
                    if (a) a.HealMultiplier = on ? 0.5f : 1f; break;

                case "CA03": // 분노의 투구: (공속+30%,크리+15% 파싱됨) / 방어 0
                    AddStat(_statA, StatType.DefensePct, -1f, on); break;

                case "CA04": // 고독의 팔찌: 분리시 공+50% / 합체시 공-20% (합체 이벤트로 토글)
                    _ca04Active = on;
                    if (a != null)
                        a.CursedConditionalAtkPct = on
                            ? (merge != null && !merge.IsMerged ? 0.5f : -0.2f) : 0f;
                    break;

                case "CA05": // 탐욕의 반지: 골드/제물 3배 / 드롭 -50%
                    if (loot) { loot.gainMultiplier += on ? 2f : -2f; loot.dropRateBonus += on ? -0.5f : 0.5f; }
                    break;

                case "CA06": // 광전사의 부적: HP↓ 공↑(최대+80%) / 회복 불가
                    if (a) a.HealMultiplier = on ? 0f : 1f;
                    // HP 비례 공격 증가는 WarriorController가 매 공격 참조(아래 LowHpAttackBonus)
                    _lowHpRageA = on; break;

                case "CB01": // 폭주의 지팡이: (마공+50%,범위+30% 파싱됨) / 마나 2배 소비
                    if (b) b.ManaCostMultiplier = on ? 2f : 1f; break;

                case "CB02": // 흡마의 서적: (쿨-25% 등) / 최대마나 -40%
                    AddStat(_statB, StatType.MaxManaPct, -0.4f, on); break;

                case "CB03": // 유리의 오브: 전 마법 2배 / 체력 1
                    AddStat(_statB, StatType.AttackPct, 1f, on);      // 이로움: 마법 2배
                    AddStat(_statB, StatType.MaxHpPct, -0.995f, on);  // 디메리트: 체력 ≈1
                    break;

                case "CB04": // 속박의 목걸이: 합체 자동시전 3배 / 분리 스킬 불가
                    if (b) b.SkillLockWhenSeparated = on; break;

                case "CB05": // 예언자의 안경: 맵/함정/적 전표시 / 시야 -70%
                    // TODO: 시야 반경/맵 표시(카메라·미니맵 연동 단계). 플래그만 기록.
                    break;

                case "CB06": // 시간의 저주서: 전 쿨 -50% / 3분마다 스킬 봉인
                    AddStat(_statB, StatType.CooldownReduction, 0.5f, on);
                    _silenceInterval = on ? 180f : 0f;
                    _silenceTimer = 0f;
                    break;
            }
        }

        bool _lowHpRageA;

        void Update()
        {
            float dt = Time.deltaTime;

            // CA01: 비전투 시에만 HP 1%/초 드레인
            if (_drainHpA && a != null && a.IsAlive && CombatState.OutOfCombat)
                a.TakeDamage(a.MaxHp * 0.01f * dt, AttributeType.None, null, isDoT: true);

            // CA06: 저체력 분노(공격력은 WarriorController가 LowHpRageBonus로 참조)
            if (a != null) a.LowHpRageActive = _lowHpRageA;

            // CB06: 주기적 스킬 봉인
            if (_silenceInterval > 0f && b != null)
            {
                _silenceTimer += dt;
                if (_silenceTimer >= _silenceInterval)
                {
                    _silenceTimer = 0f;
                    b.ApplyStatus(new StatusInstance { Type = StatusType.Silence, Duration = 5f });
                }
            }
        }

        // on=true면 모디파이어 추가, on=false면 동일 모디파이어 제거.
        // (이전엔 음수 값을 누적해 상쇄했으나, 장착/해제 반복 시 리스트가 무한 증가했음)
        static void AddStat(List<StatModifier> list, StatType t, float v, bool on)
        {
            if (on) list.Add(new StatModifier(t, v));
            else
            {
                int idx = list.FindIndex(m => m.Type == t && Mathf.Approximately(m.Value, v));
                if (idx >= 0) list.RemoveAt(idx);
            }
        }
    }
}
