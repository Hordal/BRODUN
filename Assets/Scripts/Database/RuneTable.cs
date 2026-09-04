// RuneTable.cs — 룬 조합 마법 12종 + 장비 룬 부여 8종 (문서 02-2)
// 24속성 룬 자체는 AttributeTable에 정의됨. 룬 문자는 비주얼 모티프(I-03).
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    public class RuneCombo
    {
        public AttributeType A, B;
        public string ResultName, Effect;
        public RuneCombo(AttributeType a, AttributeType b, string name, string eff)
        { A = a; B = b; ResultName = name; Effect = eff; }
        public bool Matches(AttributeType x, AttributeType y)
            => (A == x && B == y) || (A == y && B == x);
    }

    public class RuneEnchant
    {
        public EquipSlot TargetSlot;     // 무기/방어구
        public AttributeType Rune;
        public string ResultName, Extra;
        public RuneEnchant(EquipSlot slot, AttributeType rune, string name, string extra)
        { TargetSlot = slot; Rune = rune; ResultName = name; Extra = extra; }
    }

    public static class RuneTable
    {
        // ── 룬 조합 마법 12종 (거점 룬 공방, 조합 시 소모 안 됨) ──
        public static readonly List<RuneCombo> Combos = new List<RuneCombo>
        {
            new RuneCombo(AttributeType.Fire, AttributeType.Wind,  "화염 토네이도","범위 화상 + 넉백"),
            new RuneCombo(AttributeType.Ice,  AttributeType.Water, "빙하 폭풍",    "범위 둔화 + 빙결"),
            new RuneCombo(AttributeType.Lightning, AttributeType.Water,"전기 방류","물 위 광역 감전"),
            new RuneCombo(AttributeType.Earth, AttributeType.Rock,  "지진 강타",   "넉다운 + 범위 피해"),
            new RuneCombo(AttributeType.Fire, AttributeType.Earth,  "용암 분출",   "지면 DoT 영역"),
            new RuneCombo(AttributeType.Light, AttributeType.Holy,  "신성 결계",   "보호막 + 해독"),
            new RuneCombo(AttributeType.Dark, AttributeType.Curse,  "역병의 안개", "범위 DoT + 치유↓"),
            new RuneCombo(AttributeType.Ice,  AttributeType.Wind,   "눈보라",      "부채꼴 둔화 + 시야 방해"),
            new RuneCombo(AttributeType.Lightning, AttributeType.Time,"시간 가속", "공속/이속 대폭↑"),
            new RuneCombo(AttributeType.Gravity, AttributeType.Chain,"블랙홀",     "끌어당기기 + 다중 타격"),
            new RuneCombo(AttributeType.Fire, AttributeType.Explosion,"메테오",    "지정 지점 대폭발 (고쿨)"),
            new RuneCombo(AttributeType.Light, AttributeType.Dark,  "황혼의 일격", "양 속성 동시, 전 적 특효"),
        };

        public static RuneCombo FindCombo(AttributeType a, AttributeType b)
            => Combos.Find(c => c.Matches(a, b));

        // ── 장비 룬 부여 8종 (룬 1회 소비, A 무기/방어구) ──
        public static readonly List<RuneEnchant> Enchants = new List<RuneEnchant>
        {
            new RuneEnchant(EquipSlot.A_Weapon, AttributeType.Fire,      "화염검 (화상 DoT)","불 스킬 +5%"),
            new RuneEnchant(EquipSlot.A_Weapon, AttributeType.Ice,       "빙결검 (둔화)",    "빙결 +5%"),
            new RuneEnchant(EquipSlot.A_Weapon, AttributeType.Lightning, "뇌전검 (감전)",    "연쇄 +1"),
            new RuneEnchant(EquipSlot.A_Weapon, AttributeType.Poison,    "맹독검 (독 DoT)",  "중첩 +10%"),
            new RuneEnchant(EquipSlot.A_Armor,  AttributeType.Ice,       "빙결 저항",        "피격 시 둔화 반사"),
            new RuneEnchant(EquipSlot.A_Armor,  AttributeType.Fire,      "화염 방패",        "피격 시 반격 화상"),
            new RuneEnchant(EquipSlot.A_Armor,  AttributeType.Earth,     "대지 갑옷",        "방어 +10%, 넉백 저항"),
            new RuneEnchant(EquipSlot.A_Armor,  AttributeType.Light,     "빛의 갑옷",        "HP 회복, 어둠 저항"),
        };

        // ── 룬 정제 배율 (문서 02-2) ──
        public static float RefineMultiplier(int tier) // 0=일반,1=정제,2=순수
        {
            switch (tier)
            {
                case 1: return Constants.RUNE_REFINE_REFINED;
                case 2: return Constants.RUNE_REFINE_PURE;
                default: return Constants.RUNE_REFINE_NORMAL;
            }
        }
    }
}
