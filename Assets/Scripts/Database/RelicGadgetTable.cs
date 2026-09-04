// RelicGadgetTable.cs — 유물 24종 (문서 02-6) + 가젯 12종 (문서 02-5)
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    public class RelicDef
    {
        public string Id, Name, Effect;
        public Grade Grade;
        public RelicDef(string id, string name, string effect, Grade grade)
        { Id = id; Name = name; Effect = effect; Grade = grade; }
    }

    public class GadgetDef
    {
        public string Id, Name, MergedEffect, SeparatedB, SeparatedA;
        public GadgetDef(string id, string name, string merged, string sepB, string sepA)
        { Id = id; Name = name; MergedEffect = merged; SeparatedB = sepB; SeparatedA = sepA; }
    }

    public static class RelicTable
    {
        public static readonly Dictionary<string, RelicDef> All = Build();
        public static RelicDef Get(string id) => All.TryGetValue(id, out var v) ? v : null;

        static Dictionary<string, RelicDef> Build()
        {
            var d = new Dictionary<string, RelicDef>();
            void R(string id, string n, string e, Grade g) => d[id] = new RelicDef(id, n, e, g);
            var C = Grade.Common; var Ra = Grade.Rare; var L = Grade.Legendary;
            R("R01","전사의 심장","A 최대 HP +20%",C);
            R("R02","마법사의 오라","B 마나 +20%",C);
            R("R03","질주의 장화","A 이속 +15%",C);
            R("R04","집중의 렌즈","B 캐스팅 -20%",C);
            R("R05","투지의 두건","HP50%↓ 공+15%",C);
            R("R06","마나 잔류물","미사용 3초 마나 10% 회복",C);
            R("R07","철벽의 고리","방어 +12%",C);
            R("R08","신속의 깃털","공속 +10%",C);
            R("R09","유대의 끈","합체 시 전 스탯 +5%",Ra);
            R("R10","분리의 자유","분리 디버프 50%↓",Ra);
            R("R11","던지기 마스터","던지기 +30%, 착지 충격파",Ra);
            R("R12","패리의 직감","패리 판정 +0.3초",Ra);
            R("R13","룬 공명기","룬 부여 +25%",Ra);
            R("R14","감정사의 안경","미감정 즉시 감정",Ra);
            R("R15","연금술사의 솥","제작 재료 50% 절약",Ra);
            R("R16","콤보 카운터","5히트마다 +10% 중첩",Ra);
            R("R17","시너지 증폭기","시너지 보너스 +50%",L);
            R("R18","이중 시전","B 스킬 10% 2회 시전",L);
            R("R19","불사의 인장","사망 시 HP100% 부활",L);
            R("R20","혼돈의 룬","랜덤 룬 효과 추가",L);
            R("R21","완벽한 합체","합체 시 전 스탯 +10%",L);
            R("R22","두 사람의 맹약","크리 시 A/B HP/마나 3%",L);
            R("R23","던전 마스터의 지도","맵 구조 미리 표시",L);
            R("R24","초월의 제단석","제단 비용 -30%",L);
            return d;
        }
    }

    public static class GadgetTable
    {
        public static readonly Dictionary<string, GadgetDef> All = Build();
        public static GadgetDef Get(string id) => All.TryGetValue(id, out var v) ? v : null;

        static Dictionary<string, GadgetDef> Build()
        {
            var d = new Dictionary<string, GadgetDef>();
            void G(string id, string n, string m, string b, string a) => d[id] = new GadgetDef(id, n, m, b, a);
            G("G01","갈고리","벽/천장 이동","물건 끌어오기","벽 이동");
            G("G02","자석 폭탄","투척, 적 끌기","금속 물체 이동","투척");
            G("G03","텔레포트 비석","설치 후 귀환","B 비석 순간이동","A 비석 귀환");
            G("G04","쉴드 드론","동시 보호","B 방어 드론","A 방어 드론");
            G("G05","조명탄","어둠 전체 밝힘","좁은 범위","넓은 범위");
            G("G06","덫 설치기","대형 덫 1개","소형 여러 개","대형 1개");
            G("G07","음파 탐지기","벽 뒤 감지","퍼즐 힌트","적 위치 감지");
            G("G08","치유 샘","범위 치유","B 소형 치유","A 소형 치유");
            G("G09","미끼 인형","어그로 끌기(5초)","B 보호용 어그로","어그로");
            G("G10","플랫폼 생성기","발판 1개","작은 발판 3개","발판 1개");
            G("G11","시간 모래시계","적 3초 정지","적 3초 둔화","적 3초 정지");
            G("G12","부활의 깃털","즉시 부활 HP30%","B 자동 부활","A 자동 부활");
            return d;
        }
    }
}
