// EndlessTables.cs — 변이 18종 + 도전 던전 12종 + 도전 과제 9종 (문서 05-4,5 / 05-7).
using System.Collections.Generic;

namespace BroDungeon.Database
{
    public struct MutationDef
    {
        public string Name; public bool Beneficial; // 유리 변이 여부
        public MutationDef(string n, bool good) { Name = n; Beneficial = good; }
    }

    public struct ChallengeDef
    {
        public string Id, Name, Condition, Reward, Unlock; public int Rooms;
        public ChallengeDef(string id, string n, string cond, int rooms, string reward, string unlock)
        { Id = id; Name = n; Condition = cond; Rooms = rooms; Reward = reward; Unlock = unlock; }
    }

    public static class EndlessTables
    {
        // 변이 18종 (문서 05-4): 불리 14 + 유리 3 + 극한 1
        public static readonly List<MutationDef> Mutations = new List<MutationDef>
        {
            new MutationDef("광폭화",false), new MutationDef("재생",false), new MutationDef("속성면역",false),
            new MutationDef("분리/합체 저주",false), new MutationDef("안개",false), new MutationDef("시간가속",false),
            new MutationDef("독",false), new MutationDef("엘리트행렬",false), new MutationDef("보스러시",false),
            new MutationDef("장비봉인",false), new MutationDef("룬불안정",false), new MutationDef("거울반전",false),
            new MutationDef("피의제단",false), new MutationDef("쿨봉인",false),
            new MutationDef("보물축복",true), new MutationDef("시너지폭주",true), new MutationDef("강화상점",true),
            new MutationDef("극한",false),
        };

        // 도전 던전 12종 (문서 05-5)
        public static readonly Dictionary<string, ChallengeDef> Challenges = Build();
        static Dictionary<string, ChallengeDef> Build()
        {
            var d = new Dictionary<string, ChallengeDef>();
            void C(string id,string n,string cond,int rooms,string reward,string unlock)
                => d[id]=new ChallengeDef(id,n,cond,rooms,reward,unlock);
            C("CD01","전사의 시련","A만 조작",5,"A 코스메틱","1구역");
            C("CD02","마법사의 시련","B만 조작",5,"B 코스메틱","1구역");
            C("CD03","분리 금지","합체만",5,"유물: 합체의 왕관","2구역");
            C("CD04","합체 금지","분리만",5,"유물: 독립의 증표","2구역");
            C("CD05","맨손 도전","장비 해제",3,"칭호: 무장해제","3구역");
            C("CD06","속도전","3분 내",5,"칭호: 질풍","3구역");
            C("CD07","무피해","피격=실패",3,"칭호: 불가침","3구역");
            C("CD08","퍼즐 마라톤","퍼즐 10개 연속",10,"유물: 해결사의 렌즈","4구역");
            C("CD09","보스 러시","5보스 연속",5,"전설 유물 택1","5구역");
            C("CD10","역할 반전","스탯 교환",5,"반전 스킨","5구역");
            C("CD11","1HP 도전","체력 1",3,"칭호: 유리 심장","엔드리스");
            C("CD12","전속성 도전","적 랜덤 면역",5,"유물: 만물의 룬","엔드리스");
            return d;
        }

        // 도전 과제 9종 (문서 05-7)
        public static readonly string[] Achievements =
        {
            "첫 걸음(1구역)","분리 마스터(패리50회)","콤보 장인(10연속)","연금술사(레시피10개)",
            "룬 수집가(24룬)","무피해 보스","속도 클리어(5분)","미궁 정복자(5구역)","심연의 끝(100층)",
        };
    }
}
