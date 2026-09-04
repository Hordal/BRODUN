// AltarSystem.cs — 제단 3택 (A풀/B풀/C풀) + 제물 경제 (문서 05-2, I-04 안B).
// A풀(런 한정 스탯)·C풀(영구 강화)을 실제 StatModifier로 EquipmentSystem에 반영.
// B풀(축복)은 특수 효과라 런 버프 문자열로 유지(개별 핸들러 TODO).
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;
using BroDungeon.Items;

namespace BroDungeon.Dungeon
{
    public enum AltarTarget { Both, A, B }

    public class AltarOption
    {
        public AltarPool Pool;
        public string Name, Effect;
        public int Cost;
        public AltarTarget Target = AltarTarget.Both;
        public List<StatModifier> Modifiers = new List<StatModifier>();
        public string PermKey; // C풀 영구 강화 식별
    }

    public class AltarSystem : MonoBehaviour
    {
        public EquipmentSystem equipment;
        public float altarCostReduction = 0f; // 유물 R24 초월의 제단석

        // ── A풀 스탯 강화 (런 한정). value는 min~max 롤. ──
        struct StatEntry { public string key, name; public StatType stat; public float min, max; public AltarTarget target; public bool percent; }
        static StatEntry SE(string k, string n, StatType s, float mn, float mx, AltarTarget t, bool pct)
            => new StatEntry { key = k, name = n, stat = s, min = mn, max = mx, target = t, percent = pct };

        static readonly StatEntry[] APool =
        {
            SE("atk","공격력 강화", StatType.AttackFlat, 8, 15, AltarTarget.Both, false),
            SE("def","방어력 강화", StatType.DefenseFlat, 5, 12, AltarTarget.Both, false),
            SE("ms", "이속 강화",   StatType.MoveSpeedPct, 8, 15, AltarTarget.Both, true),
            SE("hp", "체력 강화",   StatType.MaxHpFlat, 30, 80, AltarTarget.Both, false),
            SE("mp", "마나 강화",   StatType.MaxManaFlat, 25, 60, AltarTarget.B, false),
            SE("as", "공속 강화",   StatType.AttackSpeedPct, 5, 12, AltarTarget.Both, true),
            SE("stm","스태미나 강화",StatType.StaminaFlat, 15, 40, AltarTarget.A, false),
            SE("cast","캐스팅 강화", StatType.CastTimePct, 8, 18, AltarTarget.B, true),   // 감소(빠름)
            SE("cdr","쿨감 강화",   StatType.CooldownReduction, 5, 12, AltarTarget.Both, true),
        };

        // ── C풀 영구 강화 (고정 step). 일부(포션/제물)는 스탯 없음 → perm만 저장. ──
        static readonly StatEntry[] CPool =
        {
            SE("c_hp","체력 영구+10",  StatType.MaxHpFlat, 10, 10, AltarTarget.Both, false),
            SE("c_atk","공격 영구+3",  StatType.AttackFlat, 3, 3, AltarTarget.Both, false),
            SE("c_def","방어 영구+2",  StatType.DefenseFlat, 2, 2, AltarTarget.Both, false),
            SE("c_mp","마나 영구+10",  StatType.MaxManaFlat, 10, 10, AltarTarget.B, false),
            SE("c_ms","이속 영구+1%",  StatType.MoveSpeedPct, 1, 1, AltarTarget.Both, true),
            SE("c_cdr","쿨감 영구+1%", StatType.CooldownReduction, 1, 1, AltarTarget.Both, true),
            SE("c_cast","캐스팅 영구-1%",StatType.CastTimePct, 1, 1, AltarTarget.B, true),
            SE("c_stm","스태미나 영구+5",StatType.StaminaFlat, 5, 5, AltarTarget.A, false),
        };
        static readonly (string key, string name)[] CPoolNoStat =
        {
            ("c_potion","포션 영구+3%"), ("c_sacrifice","제물 영구+5%"),
        };

        static readonly string[] BPool = {
            "전사의 축복","마법사의 축복","수호의 축복","탐험가의 축복","신속의 축복","불사의 축복",
            "흡혈의 가호","연쇄의 가호","화염의 가호","빙결의 가호","뇌전의 가호",
            "치명","제단 총애","감정사의 눈","시너지 공명",
        };

        // B풀 축복 → 스탯 매핑(가능한 것만). 비스탯 효과(가호/총애/감정 등)는 런 버프 문자열 + 특수 플래그.
        struct Blessing { public StatType stat; public float value; public AltarTarget target; public bool hasStat; }
        static Blessing Bless(StatType s, float v, AltarTarget t) => new Blessing { stat = s, value = v, target = t, hasStat = true };
        static readonly Dictionary<string, Blessing> BlessingStats = new Dictionary<string, Blessing>
        {
            { "전사의 축복",   Bless(StatType.AttackPct, 0.10f, AltarTarget.A) },
            { "마법사의 축복", Bless(StatType.AttackPct, 0.10f, AltarTarget.B) },
            { "수호의 축복",   Bless(StatType.DefensePct, 0.12f, AltarTarget.Both) },
            { "신속의 축복",   Bless(StatType.MoveSpeedPct, 0.12f, AltarTarget.Both) },
            { "불사의 축복",   Bless(StatType.MaxHpPct, 0.15f, AltarTarget.Both) },
            { "치명",          Bless(StatType.CritChance, 0.15f, AltarTarget.Both) },
            { "탐험가의 축복", Bless(StatType.DropRatePct, 0.15f, AltarTarget.A) },
        };

        // ── 3택 생성 ──
        public List<AltarOption> Generate(int seed)
        {
            var rng = new System.Random(seed);
            int aIdx1 = rng.Next(APool.Length);
            var opts = new List<AltarOption>
            {
                MakeStatOption(AltarPool.A_Stat, APool[aIdx1], rng, 20, 120, perm: false),
                MakeBlessing(BPool[rng.Next(BPool.Length)], rng),
            };

            // C풀: 약 1/3 확률(영구). 아니면 A풀 추가(1번과 다른 항목으로 — 중복 3택 방지).
            if (rng.Next(3) == 0)
            {
                if (rng.Next(10) < 8) // 스탯형 C풀
                    opts.Add(MakeStatOption(AltarPool.C_Permanent, CPool[rng.Next(CPool.Length)], rng, 120, 250, perm: true));
                else                  // 비스탯형 C풀(포션/제물)
                {
                    var ns = CPoolNoStat[rng.Next(CPoolNoStat.Length)];
                    opts.Add(new AltarOption { Pool = AltarPool.C_Permanent, Name = ns.name, Effect = "영구 강화", Cost = ScaledCost(120, 250, rng), PermKey = ns.key });
                }
            }
            else
            {
                int aIdx2 = APool.Length > 1 ? (aIdx1 + 1 + rng.Next(APool.Length - 1)) % APool.Length : aIdx1;
                opts.Add(MakeStatOption(AltarPool.A_Stat, APool[aIdx2], rng, 20, 120, perm: false));
            }

            return opts;
        }

        AltarOption MakeStatOption(AltarPool pool, StatEntry e, System.Random rng, int costMin, int costMax, bool perm)
        {
            int raw = (e.min == e.max) ? (int)e.min : rng.Next((int)e.min, (int)e.max + 1);
            float value = e.percent ? raw / 100f : raw;
            if (e.stat == StatType.CastTimePct) value = -value; // 캐스팅/차징 감소 = 음수(빠름)

            string sign = e.stat == StatType.CastTimePct ? "-" : "+";
            string unit = e.percent ? "%" : "";
            return new AltarOption
            {
                Pool = pool, Name = e.name, Effect = $"{e.name} ({sign}{raw}{unit})",
                Cost = ScaledCost(costMin, costMax, rng), Target = e.target,
                PermKey = perm ? e.key : null,
                Modifiers = { new StatModifier(e.stat, value) }
            };
        }

        AltarOption MakeBlessing(string name, System.Random rng)
        {
            var opt = new AltarOption { Pool = AltarPool.B_Blessing, Name = name, Effect = "런 한정 축복", Cost = ScaledCost(40, 120, rng) };
            if (BlessingStats.TryGetValue(name, out var b) && b.hasStat)
            {
                opt.Target = b.target;
                opt.Modifiers.Add(new StatModifier(b.stat, b.value));
                opt.Effect = $"{name} (스탯)";
            }
            return opt;
        }

        int ScaledCost(int min, int max, System.Random rng)
            => Mathf.Max(1, Mathf.RoundToInt(rng.Next(min, max + 1) * (1f - altarCostReduction)));

        // ── 선택 → 제물 소비 + 효과 적용 ──
        public bool Choose(AltarOption opt)
        {
            if (CurrencyManager.Instance == null) return false;
            if (!CurrencyManager.Instance.TrySpend(Currency.Sacrifice, opt.Cost)) return false;
            ApplyEffect(opt);
            return true;
        }

        void ApplyEffect(AltarOption opt)
        {
            // 스탯 모디파이어 적용(A풀=런, C풀=영구 — 둘 다 이번 세션엔 즉시 반영)
            ApplyModifiers(opt.Target, opt.Modifiers);

            if (opt.Pool == AltarPool.C_Permanent)
            {
                var perm = Core.SaveManager.Instance?.Data.permanent;
                if (perm != null && opt.PermKey != null)
                {
                    perm.permUpgrades.TryGetValue(opt.PermKey, out int lv);
                    perm.permUpgrades[opt.PermKey] = lv + 1; // 단계 누적(표시 단계에서 5~10 캡)
                    Core.SaveManager.Instance.SavePermanent();
                }
            }
            else
            {
                Core.SaveManager.Instance?.Data.run.altarBuffs.Add(opt.Name);
            }
        }

        void ApplyModifiers(AltarTarget target, List<StatModifier> mods)
        {
            if (equipment == null || mods == null) return;
            foreach (var m in mods)
            {
                if (target == AltarTarget.Both || target == AltarTarget.A)
                    equipment.AddExternalModifier(CharacterType.A_Warrior, m);
                if (target == AltarTarget.Both || target == AltarTarget.B)
                    equipment.AddExternalModifier(CharacterType.B_Mage, m);
            }
        }

        /// 런 시작 시 영구 강화(C풀) 재적용. permUpgrades(key→level) → 스탯.
        public void ApplyPermanentUpgrades()
        {
            var perm = Core.SaveManager.Instance?.Data.permanent;
            if (perm == null || equipment == null) return;
            foreach (var kv in perm.permUpgrades)
            {
                var e = System.Array.Find(CPool, x => x.key == kv.Key);
                if (string.IsNullOrEmpty(e.key)) continue; // 비스탯형(포션/제물)은 별도 처리
                float per = e.percent ? e.min / 100f : e.min;
                if (e.stat == StatType.CastTimePct) per = -per;
                var mod = new StatModifier(e.stat, per * kv.Value); // level배
                ApplyModifiers(e.target, new List<StatModifier> { mod });
            }
        }
    }
}
