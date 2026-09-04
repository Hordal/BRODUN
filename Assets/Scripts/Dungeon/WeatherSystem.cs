// WeatherSystem.cs — 환경 변이 16종 (문서 05-6). 층당 50% 확률.
// 데이터 + 게임플레이 적용: 스탯 모디파이어(EquipmentSystem 공급자), 주기적 HP, 골드/드롭 배율.
// 시야/지형/적 전역버프가 필요한 변이(안개·지진·정전·중력·번개폭풍·저주의밤)는
// 카메라·레벨·적 시스템 연동 단계에서 구현 — 현재는 데이터 유지 + 로그.
using System;
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;
using BroDungeon.Items;
using BroDungeon.Characters;

namespace BroDungeon.Dungeon
{
    public struct WeatherDef
    {
        public string Id, Name, Effect, Affinity;
        public bool Bonus;
        public WeatherDef(string id, string n, string e, string aff, bool bonus)
        { Id = id; Name = n; Effect = e; Affinity = aff; Bonus = bonus; }
    }

    public class WeatherSystem : MonoBehaviour
    {
        public WeatherDef? Active { get; private set; }

        [Header("적용 대상 (GameBootstrap 와이어링)")]
        public EquipmentSystem equipment;
        public WarriorController warrior;
        public MageController mage;

        public static readonly List<WeatherDef> All = new List<WeatherDef>
        {
            new WeatherDef("W01","폭우","이속-10%, 번개+20%","번개↑불↓",false),
            new WeatherDef("W02","안개","시야50%↓","바람 제거",false),
            new WeatherDef("W03","폭설","빙판, 이속-15%, 얼음+20%","얼음↑불↓",false),
            new WeatherDef("W04","열파","스태미나+30%, 불+15%","불↑얼음↓",false),
            new WeatherDef("W05","독안개","초당 독 피해","독↑바람 완화",false),
            new WeatherDef("W06","지진","5초 흔들림+낙석","땅↑",false),
            new WeatherDef("W07","마력 폭풍","마나+50%, 마법+15%, 물리-10%","마법↑",false),
            new WeatherDef("W08","정전","매우 어두움","빛↑어둠↑",false),
            new WeatherDef("W09","중력 이상","점프2배, 낙하 느림","중력↑",false),
            new WeatherDef("W10","시간 왜곡","전체 80% 속도","시간↑",false),
            new WeatherDef("W11","번개 폭풍","낙뢰, 감전+15%","번개↑",false),
            new WeatherDef("W12","꽃가루","HP+3%/초, 치유+30%","보너스",true),
            new WeatherDef("W13","바람의 축제","이속+15%, 넉백2배","바람↑",false),
            new WeatherDef("W14","저주의 밤","적공+20%, 디버프2배","저주↑",false),
            new WeatherDef("W15","황금빛","골드2배, 상점-20%","보너스",true),
            new WeatherDef("W16","룬 공명","룬+30%, 드롭+20%","보너스",true),
        };

        // ── 런타임 적용 상태 ──
        readonly List<StatModifier> _wA = new List<StatModifier>();
        readonly List<StatModifier> _wB = new List<StatModifier>();
        float _hpPerSecPct;            // +회복 / -피해 (최대HP 비율/초)
        AttributeType _hpAttr;         // 피해 속성(독안개 등)
        float _appliedGoldDelta;       // LootSystem에 적용 중인 골드배율 델타
        float _appliedDropDelta;       // 적용 중인 드롭보너스 델타
        bool _registered;

        /// 층 진입 시 시드 기반 50% 확률 적용(멀티 동기화).
        public void RollForFloor(int seed)
        {
            var rng = new System.Random(seed ^ 0x5EED);
            Active = rng.NextDouble() < 0.5 ? All[rng.Next(All.Count)] : (WeatherDef?)null;
            ApplyActive();
        }

        void EnsureRegistered()
        {
            if (_registered || equipment == null) return;
            equipment.RegisterProvider(ProvideMods);
            _registered = true;
        }

        IEnumerable<StatModifier> ProvideMods(CharacterType who)
            => who == CharacterType.A_Warrior ? _wA : _wB;

        /// 현재 Active 날씨의 효과를 적용(이전 효과는 교체).
        void ApplyActive()
        {
            // 이전 loot 델타 원복
            ApplyLootDelta(-_appliedGoldDelta, -_appliedDropDelta);

            _wA.Clear(); _wB.Clear();
            _hpPerSecPct = 0f; _hpAttr = AttributeType.None;
            float gold = 0f, drop = 0f;

            if (Active.HasValue)
            {
                switch (Active.Value.Id)
                {
                    case "W01": Both(StatType.MoveSpeedPct, -0.10f); break;            // 폭우: 이속-10%
                    case "W03": Both(StatType.MoveSpeedPct, -0.15f); break;            // 폭설: 이속-15%
                    case "W04": _wA.Add(new StatModifier(StatType.StaminaFlat, 30f)); break; // 열파: 스태미나+30%(A 기준+30)
                    case "W05": _hpPerSecPct = -0.03f; _hpAttr = AttributeType.Poison; break; // 독안개: 초당 3% 독
                    case "W07":                                                        // 마력폭풍
                        _wB.Add(new StatModifier(StatType.MaxManaPct, 0.50f));
                        _wB.Add(new StatModifier(StatType.AttackPct, 0.15f));
                        _wA.Add(new StatModifier(StatType.AttackPct, -0.10f));
                        break;
                    case "W10": Both(StatType.MoveSpeedPct, -0.20f); Both(StatType.AttackSpeedPct, -0.20f); break; // 시간왜곡: 80%
                    case "W12": _hpPerSecPct = 0.03f; break;                           // 꽃가루: HP+3%/초
                    case "W13": Both(StatType.MoveSpeedPct, 0.15f); break;             // 바람의 축제: 이속+15%
                    case "W15": gold = 1f; break;                                      // 황금빛: 골드 2배(배율 1→2)
                    case "W16": drop = 0.20f; break;                                   // 룬 공명: 드롭+20%
                    default:
                        Debug.Log($"[Weather] {Active.Value.Name}: 게임플레이 효과 미연동(카메라/지형/적 시스템 필요) — 데이터만 활성");
                        break;
                }
            }

            EnsureRegistered();
            equipment?.Recalculate(); // 스탯 모디파이어 즉시 반영
            ApplyLootDelta(gold, drop);
            _appliedGoldDelta = gold; _appliedDropDelta = drop;
        }

        void Both(StatType t, float v)
        {
            _wA.Add(new StatModifier(t, v));
            _wB.Add(new StatModifier(t, v));
        }

        static void ApplyLootDelta(float gold, float drop)
        {
            var loot = LootSystem.Instance;
            if (loot == null) return;
            loot.gainMultiplier += gold;
            loot.dropRateBonus += drop;
        }

        void Update()
        {
            if (_hpPerSecPct == 0f) return;
            float dt = Time.deltaTime;
            ApplyPeriodic(warrior, dt);
            if (mage != null && mage.State == DownState.Alive) ApplyPeriodic(mage, dt);
        }

        void ApplyPeriodic(CharacterBase c, float dt)
        {
            if (c == null || !c.IsAlive) return;
            if (_hpPerSecPct > 0f) c.Heal(c.MaxHp * _hpPerSecPct * dt);
            else c.TakeDamage(c.MaxHp * -_hpPerSecPct * dt, _hpAttr, null, isDoT: true);
        }

        void OnDisable()
        {
            // 씬 종료/비활성 시 loot 델타 원복(중복 누적 방지)
            ApplyLootDelta(-_appliedGoldDelta, -_appliedDropDelta);
            _appliedGoldDelta = 0f; _appliedDropDelta = 0f;
            if (_registered) { equipment?.UnregisterProvider(ProvideMods); _registered = false; }
        }
    }
}
