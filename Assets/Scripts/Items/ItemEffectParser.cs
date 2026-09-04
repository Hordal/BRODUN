// ItemEffectParser.cs — 아이템 효과 문자열("공+15%" 등) → StatModifier 변환.
// md 표의 한글 효과 표기를 재인코딩 없이 실제 스탯으로 연결(베스트-에포트).
// 인식 불가 토큰은 무시(표시 문자열로만 남음).
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BroDungeon.Combat;

namespace BroDungeon.Items
{
    public static class ItemEffectParser
    {
        // 키워드 → 스탯 (긴 키워드 우선: 공속/마공/방무시/최대HP 를 공/방/HP 보다 먼저).
        static readonly (string kw, StatType stat, bool percent)[] Rules =
        {
            ("공속",   StatType.AttackSpeedPct, true),
            ("마공",   StatType.AttackPct,      true),
            ("방무시", StatType.PiercePct,      true),
            ("최대 HP",StatType.MaxHpPct,       true),
            ("최대HP", StatType.MaxHpPct,       true),
            ("체력",   StatType.MaxHpPct,       true),
            ("마나",   StatType.MaxManaPct,     true),
            ("이속",   StatType.MoveSpeedPct,   true),
            ("치확",   StatType.CritChance,     true),
            ("크리",   StatType.CritChance,     true),
            ("흡혈",   StatType.LifestealPct,   true),
            ("드롭",   StatType.DropRatePct,    true),
            ("쿨감",   StatType.CooldownReduction, true),
            ("캐스팅", StatType.CastTimePct,    true),
            ("차징",   StatType.CastTimePct,    true),
            ("방",     StatType.DefensePct,     true),
            ("공",     StatType.AttackPct,      true),
            ("HP",     StatType.MaxHpPct,       true),
        };

        public static List<StatModifier> Parse(string effect)
        {
            var mods = new List<StatModifier>();
            if (string.IsNullOrEmpty(effect)) return mods;

            string work = effect;
            foreach (var rule in Rules)
            {
                // 키워드 뒤의 부호+숫자(%선택) 추출. 예: "공+15%", "방무시 25%", "쿨-15%"
                // "처치마나20%"/"처치HP5%" 같은 '처치 시' 효과는 최대치 증가가 아니므로 제외(부정 룩비하인드).
                var rx = new Regex(@"(?<!처치)" + Regex.Escape(rule.kw) + @"\s*([+\-]?\d+)\s*%?");
                Match m;
                while ((m = rx.Match(work)).Success)
                {
                    if (int.TryParse(m.Groups[1].Value, out int num))
                    {
                        float val = rule.percent ? num / 100f : num;
                        // 캐스팅/차징은 "감소"가 이득 → 음수로 저장(빠름)
                        if (rule.stat == StatType.CastTimePct && val > 0f) val = -val;
                        mods.Add(new StatModifier(rule.stat, val));
                    }
                    // 매칭 구간 제거(공속이 공으로 이중 매칭되는 것 방지)
                    work = work.Remove(m.Index, m.Length).Insert(m.Index, new string(' ', m.Length));
                }
            }
            return mods;
        }
    }
}
