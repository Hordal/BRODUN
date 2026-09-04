// SynergyCalculator.cs — 세트 시너지 합산 (문서 02-1, I-05/I-07)
// 합산 소스 = 아이템(최대3) + 장착 스킬(최대3) + A 무기/방어구 룬 부여(최대2). 캐릭당 최대 8.
// 세트 참여 속성 = 18종(원소6+빛어둠4+전술6=16 + 강화 중 액티브 스킬 보유 2: 관통·흡수).
// 나머지 강화 6종은 단일 옵션으로만(세트 시너지 제외, I-07).
using System.Collections.Generic;
using BroDungeon.Data;
using BroDungeon.Database;

namespace BroDungeon.Items
{
    public struct SynergyResult
    {
        public AttributeType Attribute;
        public int Count;            // 합산 카운트
        public string Tier;          // 2/4/6세트
        public float DamageBonus;    // (1+속성 보너스)에 들어갈 값
    }

    public static class SynergyCalculator
    {
        /// 속성별 카운트 합산 후 보너스 산출.
        public static List<SynergyResult> Evaluate(
            IEnumerable<AttributeType> itemAttrs,   // 장착 아이템 속성들
            IEnumerable<AttributeType> skillAttrs,  // 장착 스킬 속성들
            IEnumerable<AttributeType> runeAttrs)   // 룬 부여 속성들
        {
            var counts = new Dictionary<AttributeType, float>();
            void Tally(IEnumerable<AttributeType> src, int max, float weight)
            {
                int n = 0;
                foreach (var a in src)
                {
                    if (n >= max) break;
                    if (!AttributeTable.IsSynergyMember(a)) continue; // 강화 제외(I-07)
                    counts[a] = counts.TryGetValue(a, out var v) ? v + weight : weight;
                    n++;
                }
            }

            Tally(itemAttrs, Constants.SYNERGY_MAX_ITEM, 1f);
            Tally(skillAttrs, Constants.SYNERGY_MAX_SKILL, 1f);
            Tally(runeAttrs, Constants.SYNERGY_MAX_RUNE, Constants.SYNERGY_RUNE_WEIGHT); // [TBD: 가중치]

            var results = new List<SynergyResult>();
            foreach (var kv in counts)
            {
                int c = UnityEngine.Mathf.FloorToInt(kv.Value + 0.0001f);
                results.Add(new SynergyResult
                {
                    Attribute = kv.Key,
                    Count = c,
                    Tier = c >= 6 ? "6세트(궁극)" : c >= 4 ? "4세트(대형)" : c >= 2 ? "2세트(소형)" : "없음",
                    DamageBonus = BonusFor(c)
                });
            }
            return results;
        }

        /// 시너지 카운트 → 데미지 보너스(문서 06-4 DPS 증가율 기준).
        public static float BonusFor(int count)
        {
            if (count >= 6) return 0.30f; // 6세트 +25~35% 중앙
            if (count >= 4) return 0.15f; // 4세트 +12~18%
            if (count >= 2) return 0.05f; // 2세트 +4~6%
            return 0f;
        }

        /// 캐릭터의 총 속성 보너스(데미지 공식 (1+속성) 항).
        public static float TotalAttributeBonus(List<SynergyResult> results)
        {
            float sum = 0f;
            foreach (var r in results) sum += r.DamageBonus;
            return sum;
        }
    }
}
