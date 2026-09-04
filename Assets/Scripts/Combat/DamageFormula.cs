// DamageFormula.cs — 핵심 데미지 공식 (문서 01-5 / 06-4)
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Combat
{
    /// <summary>한 번의 공격 계산에 필요한 입력값.</summary>
    public struct DamageContext
    {
        public float BaseAtk;       // 기본 공격력
        public float GearAtk;       // 장비 공격력
        public float AltarBonus;    // 제단 보너스(가산)
        public float SkillMul;      // 스킬 배율 (0.5 = +50%)
        public float AttributeBonus;// 속성 보너스 (시너지 합산 기반)
        public float BondBonus;     // 유대 보너스
        public float CritMul;       // 크리티컬 배율 (비크리=1)
        public float TargetDefense; // 적 방어력
        public float PierceRatio;   // 방어 무시% (관통)
    }

    public static class DamageFormula
    {
        /// 적 방어율 = 방어력 / (방어력 + 100). 관통으로 유효 방어 감소.
        public static float DefenseRatio(float defense, float pierce = 0f)
        {
            float effDef = Mathf.Max(0f, defense * (1f - Mathf.Clamp01(pierce)));
            return effDef / (effDef + Constants.DEFENSE_K);
        }

        /// 최종 데미지 = (기본+장비+제단) × (1+스킬) × (1+속성) × (1+유대) × 크리 × (1-방어율)
        public static float Calculate(in DamageContext c)
        {
            float baseDmg = c.BaseAtk + c.GearAtk + c.AltarBonus;
            float mul = (1f + c.SkillMul) * (1f + c.AttributeBonus) * (1f + c.BondBonus) * c.CritMul;
            float defRatio = DefenseRatio(c.TargetDefense, c.PierceRatio);
            return Mathf.Max(1f, baseDmg * mul * (1f - defRatio));
        }

        /// EHP = HP / (1 - 방어율) (문서 06-4)
        public static float EffectiveHp(float hp, float defense)
            => hp / Mathf.Max(0.01f, 1f - DefenseRatio(defense));

        /// 크리티컬 굴림.
        public static bool RollCrit(float critChance) => Random.value < Mathf.Clamp01(critChance);
    }
}
