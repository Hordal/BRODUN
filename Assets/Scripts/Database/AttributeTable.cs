// AttributeTable.cs — 24속성 전수 인코딩 (문서 02-1)
// 룬 문자는 비주얼 모티프(I-03). 세트 시너지 참여 18종 한정(I-07).
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    public struct AttributeInfo
    {
        public AttributeType Type;
        public AttributeFamily Family;
        public string Name;       // 한글명
        public string Rune;       // 룬 문자(비주얼 모티프)
        public string GearEffect; // 장비 부여 효과
        public string Explore;    // 탐험 연동
        public bool SynergyMember; // 세트 시너지 참여 여부(I-07: 18종만 true)

        public AttributeInfo(AttributeType t, AttributeFamily f, string n, string r,
                             string g, string e, bool syn)
        { Type = t; Family = f; Name = n; Rune = r; GearEffect = g; Explore = e; SynergyMember = syn; }
    }

    public static class AttributeTable
    {
        public static readonly Dictionary<AttributeType, AttributeInfo> All = Build();

        static Dictionary<AttributeType, AttributeInfo> Build()
        {
            var d = new Dictionary<AttributeType, AttributeInfo>();
            void Add(AttributeInfo a) => d[a.Type] = a;
            var E = AttributeFamily.Element; var LD = AttributeFamily.LightDark;
            var EN = AttributeFamily.Enhance; var TA = AttributeFamily.Tactic;

            // 원소 6 (시너지 참여)
            Add(new AttributeInfo(AttributeType.Fire,      E, "불",   "ᚲ", "화상 DoT, 공격력%↑", "덩굴/나무 태우기", true));
            Add(new AttributeInfo(AttributeType.Ice,       E, "얼음", "ᛁ", "둔화, 빙결 저항",     "수면 동결",       true));
            Add(new AttributeInfo(AttributeType.Lightning, E, "번개", "ᛋ", "감전 연쇄, 치확↑",   "전기 장치 작동",  true));
            Add(new AttributeInfo(AttributeType.Water,     E, "물",   "ᛚ", "마나 회복, 치유↑",   "마른 수로 채우기", true));
            Add(new AttributeInfo(AttributeType.Wind,      E, "바람", "ᚨ", "넉백, 이속↑",        "안개 제거",       true));
            Add(new AttributeInfo(AttributeType.Earth,     E, "땅",   "ᚢ", "방어력↑, 슈퍼아머",  "바위 파괴",       true));

            // 빛/어둠 4 (시너지 참여)
            Add(new AttributeInfo(AttributeType.Light, LD, "빛",       "ᛞ", "회복↑, 언데드 특효", "어둠 방 밝히기", true));
            Add(new AttributeInfo(AttributeType.Dark,  LD, "어둠",     "ᚾ", "흡혈, 은신↑",       "그림자 통로",   true));
            Add(new AttributeInfo(AttributeType.Holy,  LD, "성스러움", "ᛉ", "보호막, 해독",      "저주 문 해제",  true));
            Add(new AttributeInfo(AttributeType.Curse, LD, "저주",     "ᚦ", "방어↓ 디버프, DoT", "봉인 문 개방",  true));

            // 강화 8 (I-07): 액티브(공격형) 스킬 보유 2종(관통 SKA20·흡수 SKB23)만 세트 시너지 참여,
            // 나머지 6종은 단일 강화로만 작동(세트 시너지 제외). → 시너지 참여 총 16+2 = 18종.
            Add(new AttributeInfo(AttributeType.Haste,  EN, "속공", "ᚠ", "공격속도↑",      "-", false));
            Add(new AttributeInfo(AttributeType.Regen,  EN, "재생", "ᛒ", "HP 자동 회복",   "-", false));
            Add(new AttributeInfo(AttributeType.Focus,  EN, "집중", "ᛗ", "캐스팅↓",        "-", false));
            Add(new AttributeInfo(AttributeType.Endure, EN, "인내", "ᛇ", "스태미나 회복↑", "-", false));
            Add(new AttributeInfo(AttributeType.Pierce, EN, "관통", "ᛏ", "방어 무시%",     "-", true));
            Add(new AttributeInfo(AttributeType.Absorb, EN, "흡수", "ᛈ", "마나 킬 회복",   "-", true));
            Add(new AttributeInfo(AttributeType.Luck,   EN, "행운", "ᚹ", "드롭률↑",        "-", false));
            Add(new AttributeInfo(AttributeType.Growth, EN, "성장", "ᛃ", "경험치/제물↑",   "-", false));

            // 전술 6 (시너지 참여)
            Add(new AttributeInfo(AttributeType.Rock,      TA, "바위", "ᚺ", "넉다운, CC 저항",     "바위벽 파괴",    true));
            Add(new AttributeInfo(AttributeType.Poison,    TA, "독",   "ᛖ", "독 DoT, 치유↓",      "독 안개 정화",   true));
            Add(new AttributeInfo(AttributeType.Explosion, TA, "폭발", "ᛟ", "범위 피해, 자가 피해", "균열 벽 파괴",  true));
            Add(new AttributeInfo(AttributeType.Time,      TA, "시간", "ᚱ", "쿨감, 슬로우",        "시간 잠금 해제", true));
            Add(new AttributeInfo(AttributeType.Gravity,   TA, "중력", "ᛝ", "끌어당김, 낙하↑",     "부유 발판",     true));
            Add(new AttributeInfo(AttributeType.Chain,     TA, "연쇄", "ᚷ", "다중 타격, 바운스",   "연결 스위치",   true));

            return d;
        }

        public static AttributeInfo Get(AttributeType t) => All[t];
        public static string Name(AttributeType t) => t == AttributeType.None ? "무" : All[t].Name;
        public static bool IsSynergyMember(AttributeType t) => t != AttributeType.None && All[t].SynergyMember;
    }
}
