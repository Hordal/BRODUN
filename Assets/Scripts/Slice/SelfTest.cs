// SelfTest.cs — 핵심 로직 런타임 검증(파서/데미지/시너지/스탯). Unity 미설치 환경 대비 수동 점검 보조.
// 사용: 빈 GameObject에 부착 후 Play → Console에 PASS/FAIL 로그.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;
using BroDungeon.Items;
using BroDungeon.Database;
using BroDungeon.Dungeon;
using BroDungeon.AI;

namespace BroDungeon.Slice
{
    public class SelfTest : MonoBehaviour
    {
        int _pass, _fail;

        void Start() => RunAll();

        [ContextMenu("Run Self Test")]
        public void RunAll()
        {
            _pass = _fail = 0;

            // 1) 효과 파서
            var m1 = ItemEffectParser.Parse("공+15%");
            Check("파서 공+15%", m1.Count == 1 && Approx(GetVal(m1, StatType.AttackPct), 0.15f));

            var m2 = ItemEffectParser.Parse("공+8%, 방+10%");
            Check("파서 복합", Approx(GetVal(m2, StatType.AttackPct), 0.08f) && Approx(GetVal(m2, StatType.DefensePct), 0.10f));

            var m3 = ItemEffectParser.Parse("공속+20%, 이속+10%");
            Check("파서 공속≠공", Approx(GetVal(m3, StatType.AttackSpeedPct), 0.20f)
                && Approx(GetVal(m3, StatType.MoveSpeedPct), 0.10f)
                && GetVal(m3, StatType.AttackPct) == 0f);

            // 2) 스탯 시트
            var sheet = new StatSheet();
            sheet.Add(StatType.AttackPct, 0.15f);
            Check("스탯 FinalAttack", Approx(sheet.FinalAttack(50f), 57.5f));
            sheet.Add(StatType.MaxHpPct, 0.2f);
            Check("스탯 FinalMaxHp", Approx(sheet.FinalMaxHp(500f), 600f));

            // 3) 데미지 공식 (방어율 = def/(def+100))
            var ctx = new DamageContext { BaseAtk = 50, CritMul = 1f, TargetDefense = 100f };
            Check("데미지 기본", Approx(DamageFormula.Calculate(ctx), 25f));
            Check("방어율 0.5", Approx(DamageFormula.DefenseRatio(100f), 0.5f));

            // 4) 시너지 (Fire 2개 = 2세트 +5%)
            var syn = SynergyCalculator.Evaluate(
                new[] { AttributeType.Fire, AttributeType.Fire }, new AttributeType[0], new AttributeType[0]);
            Check("시너지 2세트", Approx(SynergyCalculator.TotalAttributeBonus(syn), 0.05f));

            // 5) 강화 계열 시너지 (I-07): 액티브 스킬 없는 속공은 제외, 액티브 보유 관통은 참여
            var syn2 = SynergyCalculator.Evaluate(
                new[] { AttributeType.Haste, AttributeType.Haste }, new AttributeType[0], new AttributeType[0]);
            Check("시너지 강화(속공) 제외", syn2.Count == 0);
            var syn3 = SynergyCalculator.Evaluate(
                new[] { AttributeType.Pierce, AttributeType.Pierce }, new AttributeType[0], new AttributeType[0]);
            Check("시너지 강화(관통) 참여", Approx(SynergyCalculator.TotalAttributeBonus(syn3), 0.05f));

            // 6) 저주 스탯 모델링 (디메리트/이로운효과)
            var cz = new StatSheet(); cz.Add(StatType.DefensePct, -1f);
            Check("저주 방어0", Approx(cz.FinalDefense(20f), 0f));
            var ch = new StatSheet(); ch.Add(StatType.MaxHpPct, -0.995f);
            Check("저주 체력≈1", Approx(ch.FinalMaxHp(200f), 1f));
            var cm = new StatSheet(); cm.Add(StatType.AttackPct, 1f);
            Check("저주 마법2배", Approx(cm.FinalAttack(30f), 60f));
            var cc = new StatSheet(); cc.Add(StatType.CooldownReduction, 0.5f); cc.Add(StatType.CooldownReduction, 0.5f);
            Check("쿨감 상한 0.8", Approx(cc.CooldownReduction, 0.8f));

            // 7) 제단 생성 (시드 결정적, 3택 + A풀 모디파이어)
            var altar = gameObject.AddComponent<AltarSystem>();
            var opts = altar.Generate(12345);
            bool hasStatMod = false;
            foreach (var o in opts) if (o.Pool == AltarPool.A_Stat && o.Modifiers.Count > 0) hasStatMod = true;
            Check("제단 3택", opts.Count == 3);
            Check("제단 A풀 스탯", hasStatMod);

            // 7-b) 보스 패턴 라이브러리
            var b1 = BossPatternLibrary.Build("BOSS1", 60f);
            var b5 = BossPatternLibrary.Build("BOSS5", 180f);
            Check("보스1 3페이즈", b1.Length == 3);
            Check("보스5 4페이즈", b5.Length == 4);
            bool allPhasesHaveActions = true;
            foreach (var ph in b5) if (ph.Count == 0) allPhasesHaveActions = false;
            Check("보스5 페이즈 액션", allPhasesHaveActions);

            // 8) 데이터 무결성
            Check("아이템 84개", ItemTable.All.Count == 84);
            Check("스킬 72개", SkillTable.All.Count == 72);
            Check("속성 24개", AttributeTable.All.Count == 24);
            Check("몬스터 44+보스", MonsterTable.All.Count == 44 && BossTable.All.Count == 6);

            Debug.Log($"[SelfTest] 완료 — PASS {_pass} / FAIL {_fail}");
        }

        void Check(string name, bool ok)
        {
            if (ok) { _pass++; Debug.Log($"[SelfTest] PASS: {name}"); }
            else { _fail++; Debug.LogError($"[SelfTest] FAIL: {name}"); }
        }

        static bool Approx(float a, float b) => Mathf.Abs(a - b) < 0.001f;
        static float GetVal(List<StatModifier> mods, StatType t)
        {
            float s = 0f; foreach (var m in mods) if (m.Type == t) s += m.Value; return s;
        }
    }
}
