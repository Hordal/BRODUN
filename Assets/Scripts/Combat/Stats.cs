// Stats.cs — 스탯 집계 시트 (장비/제단/유물/시너지 → 최종 스탯). 문서 01-5/06-4 공식 연동.
using System.Collections.Generic;
using UnityEngine;

namespace BroDungeon.Combat
{
    public enum StatType
    {
        AttackFlat, AttackPct,
        DefenseFlat, DefensePct,
        MaxHpFlat, MaxHpPct,
        MaxManaFlat, MaxManaPct,
        MoveSpeedPct, AttackSpeedPct,
        CritChance, CritDamage,
        CooldownReduction, CastTimePct,
        StaminaFlat,
        PiercePct, LifestealPct, DropRatePct
    }

    public struct StatModifier
    {
        public StatType Type;
        public float Value; // 비율 스탯은 분수(0.15 = +15%), 고정 스탯은 절대값
        public StatModifier(StatType t, float v) { Type = t; Value = v; }
    }

    /// <summary>한 캐릭터의 스탯 가산 합. 최종값 = (base + flat) × (1 + pctSum).</summary>
    public class StatSheet
    {
        readonly Dictionary<StatType, float> _sum = new Dictionary<StatType, float>();

        public void Clear() => _sum.Clear();
        public float Get(StatType t) => _sum.TryGetValue(t, out var v) ? v : 0f;
        public void Add(StatType t, float v) => _sum[t] = Get(t) + v;
        public void Add(StatModifier m) => Add(m.Type, m.Value);
        public void AddRange(IEnumerable<StatModifier> mods) { foreach (var m in mods) Add(m); }

        // ── 최종 파생 스탯 헬퍼 ──
        public float FinalAttack(float baseAtk) => (baseAtk + Get(StatType.AttackFlat)) * (1f + Get(StatType.AttackPct));
        public float FinalDefense(float baseDef) => (baseDef + Get(StatType.DefenseFlat)) * (1f + Get(StatType.DefensePct));
        public float FinalMaxHp(float baseHp) => Mathf.Max(1f, (baseHp + Get(StatType.MaxHpFlat)) * (1f + Get(StatType.MaxHpPct)));
        public float FinalMaxMana(float baseMana) => Mathf.Max(0f, (baseMana + Get(StatType.MaxManaFlat)) * (1f + Get(StatType.MaxManaPct)));
        public float MoveSpeedMul => 1f + Get(StatType.MoveSpeedPct);
        public float AttackSpeedMul => 1f + Get(StatType.AttackSpeedPct);
        public float CritChance => Mathf.Clamp01(0.05f + Get(StatType.CritChance));
        public float CritDamage => 1.5f + Get(StatType.CritDamage);
        public float CooldownReduction => Mathf.Clamp(Get(StatType.CooldownReduction), 0f, 0.8f);
        public float CastTimeMul => Mathf.Clamp(1f + Get(StatType.CastTimePct), 0.2f, 2f);
        public float Pierce => Mathf.Clamp01(Get(StatType.PiercePct));
    }
}
