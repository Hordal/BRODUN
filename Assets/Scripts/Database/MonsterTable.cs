// MonsterTable.cs — 몬스터 45종 + 보스 5+히든1 (문서 04). HP는 구역 밸런스 기준 대표값.
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    public enum HpTier { Low, Mid, High, VeryHigh, Extreme }

    public class MonsterDef
    {
        public string Id, Name, Behavior, Counter;
        public int Zone;
        public AttributeType Attr1, Attr2;
        public EnemyType Type;
        public HpTier Hp;
        public MonsterDef(string id, int zone, string name, AttributeType a1, EnemyType t, HpTier hp,
                          string behavior, string counter, AttributeType a2 = AttributeType.None)
        { Id = id; Zone = zone; Name = name; Attr1 = a1; Type = t; Hp = hp; Behavior = behavior; Counter = counter; Attr2 = a2; }
    }

    public class BossDef
    {
        public string Id, Name, Appearance;
        public int Zone;
        public AttributeType[] Attrs;
        public float MaxHp;
        public string[] PhasePatterns;
        public string Reward;
        public BossDef(string id, int zone, string name, float hp, AttributeType[] attrs,
                       string appearance, string[] phases, string reward)
        { Id = id; Zone = zone; Name = name; MaxHp = hp; Attrs = attrs; Appearance = appearance; PhasePatterns = phases; Reward = reward; }
    }

    public static class MonsterTable
    {
        public static readonly Dictionary<string, MonsterDef> All = Build();
        public static MonsterDef Get(string id) => All.TryGetValue(id, out var v) ? v : null;
        public static IEnumerable<MonsterDef> ByZone(int zone)
        { foreach (var v in All.Values) if (v.Zone == zone) yield return v; }

        /// 구역 밸런스 기준(문서 04-밸런스 기준) → HpTier를 실수치로.
        public static float ResolveHp(int zone, HpTier tier)
        {
            float[] lowHigh; // {low, high}
            switch (zone)
            {
                case 1: lowHigh = new[] { 100f, 300f }; break;
                case 2: lowHigh = new[] { 200f, 500f }; break;
                case 3: lowHigh = new[] { 350f, 700f }; break;
                case 4: lowHigh = new[] { 500f, 900f }; break;
                default: lowHigh = new[] { 600f, 1000f }; break;
            }
            switch (tier)
            {
                case HpTier.Low: return lowHigh[0];
                case HpTier.Mid: return (lowHigh[0] + lowHigh[1]) * 0.5f;
                case HpTier.High: return lowHigh[1];
                case HpTier.VeryHigh: return zone == 1 ? 800f : zone == 2 ? 1200f : zone == 3 ? 1800f : zone == 4 ? 2200f : 2500f; // 엘리트
                default: return 3000f;
            }
        }

        static Dictionary<string, MonsterDef> Build()
        {
            var d = new Dictionary<string, MonsterDef>();
            void M(MonsterDef x) => d[x.Id] = x;
            var Lo = HpTier.Low; var Mi = HpTier.Mid; var Hi = HpTier.High; var VH = HpTier.VeryHigh;

            // 1구역 9종
            M(new MonsterDef("M01",1,"석상 보병", AttributeType.Earth, EnemyType.Melee, Lo, "돌진→2연타","A 콤보"));
            M(new MonsterDef("M02",1,"이끼 슬라임",AttributeType.Earth, EnemyType.Melee, Lo, "사망 시 분열","분열 전 처리"));
            M(new MonsterDef("M03",1,"바위 굴러미",AttributeType.Rock,  EnemyType.Charge,Mi, "구체 돌진→벽 스턴","스턴 시 공격"));
            M(new MonsterDef("M04",1,"균열 박쥐", AttributeType.Wind,  EnemyType.Flying,Lo, "급강하→복귀","B 원거리 격추"));
            M(new MonsterDef("M05",1,"먼지 유령", AttributeType.Wind,  EnemyType.Ranged,Lo, "거리 유지+모래 탄환","B CC→A 접근"));
            M(new MonsterDef("M06",1,"석상 궁수", AttributeType.Earth, EnemyType.Ranged,Mi, "석궁→위치 변경","B 분리 우회"));
            M(new MonsterDef("M07",1,"바위 거미", AttributeType.Rock,  EnemyType.Melee, Mi, "천장 기습","탐지+B 선공"));
            M(new MonsterDef("M08",1,"파편 골렘", AttributeType.Rock,  EnemyType.Melee, Hi, "강타→사망 파편","콤보+파편 회피"));
            M(new MonsterDef("M09",1,"회랑의 파수꾼",AttributeType.Earth,EnemyType.Elite,VH,"방패 무적→3연타→충격파","A 유인+B 분리 측면",AttributeType.Rock));
            // 2구역 9종
            M(new MonsterDef("M10",2,"물 정령",   AttributeType.Water, EnemyType.Ranged,Mi, "물탄환→둔화 웅덩이","B 얼음으로 동결"));
            M(new MonsterDef("M11",2,"빙결 전갈", AttributeType.Ice,   EnemyType.Melee, Mi, "빙결 꼬리→구속","분리 탈출, 불 유리"));
            M(new MonsterDef("M12",2,"독 해파리", AttributeType.Poison,EnemyType.Flying,Lo, "독액 낙하","B 바람+A 점프"));
            M(new MonsterDef("M13",2,"수정 게",   AttributeType.Ice,   EnemyType.Melee, Hi, "정면 방어→후방 약점","A 탱킹+B 후방"));
            M(new MonsterDef("M14",2,"독안개 뱀", AttributeType.Poison,EnemyType.Ranged,Mi, "독안개→은신→기습","B 바람 안개 제거"));
            M(new MonsterDef("M15",2,"물의 분신", AttributeType.Water, EnemyType.Melee, Lo, "행동 복제","패턴 변화"));
            M(new MonsterDef("M16",2,"빙하 골렘", AttributeType.Ice,   EnemyType.Melee, VH, "빙결 펀치→슬라이드","불로 갑옷 용해"));
            M(new MonsterDef("M17",2,"수맥 요술사",AttributeType.Water, EnemyType.Support,Mi,"아군 회복+독 버프","최우선+B CC",AttributeType.Poison));
            M(new MonsterDef("M18",2,"수맥의 수호자",AttributeType.Water,EnemyType.Elite,VH,"삼지창→베기→빙결 아우라","빙결 시 분리 필수",AttributeType.Ice));
            // 3구역 9종
            M(new MonsterDef("M19",3,"화염 임프", AttributeType.Fire,  EnemyType.Melee, Lo, "할퀴기→자폭","자폭 밖, B 범위기"));
            M(new MonsterDef("M20",3,"용암 구더기",AttributeType.Fire,  EnemyType.Ambush,Mi, "바닥 기습→화염 분사","B 얼음 선제"));
            M(new MonsterDef("M21",3,"뇌전 도롱뇽",AttributeType.Lightning,EnemyType.Ranged,Mi,"체인 라이트닝(합체 동시 피격)","분리하여 분산"));
            M(new MonsterDef("M22",3,"폭탄 골렘", AttributeType.Explosion,EnemyType.Charge,Hi,"돌진→자폭(광범위)","B가 등 화약통 파괴"));
            M(new MonsterDef("M23",3,"화산 박쥐", AttributeType.Fire,  EnemyType.Flying,Lo, "무리 화염 급강하","B 범위기",AttributeType.Wind));
            M(new MonsterDef("M24",3,"전기 수정", AttributeType.Lightning,EnemyType.Setup,Mi,"주기적 전기장","타이밍 파괴"));
            M(new MonsterDef("M25",3,"화로의 대장장이",AttributeType.Fire,EnemyType.Melee,Hi,"망치→화염 바닥→던지기","A 유인+B 차징",AttributeType.Rock));
            M(new MonsterDef("M26",3,"용암 거북", AttributeType.Fire,  EnemyType.Defense,VH, "등 면역→머리만 공격","A 유인+B 측면",AttributeType.Earth));
            M(new MonsterDef("M27",3,"화로의 집행자",AttributeType.Fire,EnemyType.Elite,VH,"화염→번개→전신 폭발→약화","폭발 시 분리 도주",AttributeType.Lightning));
            // 4구역 9종
            M(new MonsterDef("M28",4,"빛의 기사", AttributeType.Light, EnemyType.Melee, Hi, "신성 참격→빛 폭발(시야↓)","B 방향 지시"));
            M(new MonsterDef("M29",4,"어둠의 암살자",AttributeType.Dark,EnemyType.Melee,Mi,"은신→기습→흡혈","탐지+분리 패리"));
            M(new MonsterDef("M30",4,"성스러운 방패병",AttributeType.Holy,EnemyType.Defense,Hi,"보호막→아군 보호→돌진","방패병 최우선"));
            M(new MonsterDef("M31",4,"저주의 주술사",AttributeType.Curse,EnemyType.Ranged,Mi,"저주탄+결계","B 성스러움 해제"));
            M(new MonsterDef("M32",4,"황혼의 나비",AttributeType.Light, EnemyType.Flying,Lo, "빛/어둠 전환 공격","반대 속성",AttributeType.Dark));
            M(new MonsterDef("M33",4,"가시 덤불 정령",AttributeType.Dark,EnemyType.Setup,Mi,"가시 확장→통로 차단","빛 마법 소멸"));
            M(new MonsterDef("M34",4,"발광 요정", AttributeType.Light, EnemyType.Support,Lo,"아군 회복+빛 버프","최우선 제거"));
            M(new MonsterDef("M35",4,"룬 수호상", AttributeType.Light, EnemyType.Defense,VH,"빛(방어)/어둠(공격) 전환","반대 속성",AttributeType.Dark));
            M(new MonsterDef("M36",4,"정원의 감시자",AttributeType.Light,EnemyType.Elite,VH,"분신 소환+저주 결계","동시 처치+해제",AttributeType.Curse));
            // 5구역 9종
            M(new MonsterDef("M37",5,"시간의 와해자",AttributeType.Time,EnemyType.Ranged,Mi,"슬로우탄→자신 가속","B 시간 둔화 역이용"));
            M(new MonsterDef("M38",5,"중력 파편", AttributeType.Gravity,EnemyType.Special,Mi,"끌어당기기→파편→반전","대쉬+B 분리 안전"));
            M(new MonsterDef("M39",5,"연쇄의 쇠사슬",AttributeType.Chain,EnemyType.Special,Mi,"합체 감지→강제 분리","빠르게 합체 복귀"));
            M(new MonsterDef("M40",5,"시간의 분신",AttributeType.Time, EnemyType.Special,Lo,"3초 전 행동 재현","패턴 변경"));
            M(new MonsterDef("M41",5,"심장의 눈", AttributeType.Dark,  EnemyType.Ranged,Mi,"시야 추적 레이저","B 빛 안전 영역"));
            M(new MonsterDef("M42",5,"허무의 나방",AttributeType.Dark,  EnemyType.Flying,Lo,"속성 효과 무효화 5초","우선 처치",AttributeType.Wind));
            M(new MonsterDef("M43",5,"차원 균열체",AttributeType.None,  EnemyType.Special,Hi,"매 3초 속성 변경","약점 파악"));
            M(new MonsterDef("M44",5,"심장의 근위병",AttributeType.Light,EnemyType.Elite,HpTier.Extreme,"빛→어둠→시간정지","전 테크닉",AttributeType.Dark));
            return d;
        }
    }

    public static class BossTable
    {
        public static readonly Dictionary<string, BossDef> All = Build();
        public static BossDef Get(string id) => All.TryGetValue(id, out var v) ? v : null;
        public static BossDef ByZone(int zone)
        { foreach (var v in All.Values) if (v.Zone == zone) return v; return null; }

        static Dictionary<string, BossDef> Build()
        {
            var d = new Dictionary<string, BossDef>();
            void B(BossDef x) => d[x.Id] = x;

            B(new BossDef("BOSS1",1,"석화의 왕 고르간",3000,
                new[]{AttributeType.Earth,AttributeType.Rock},
                "왕관 석상, A×3배, 돌검+방패",
                new[]{
                    "P1(100~60%): 대검3연→충격파→방패 무적5초 / A 정면+B 분리 발코니",
                    "P2(60~30%): 4팔+바닥 진동+B 추적 / A 본체, B 팔 CC",
                    "P3(30~0%): 바위 회전+낙석+균열 약점3초 / 합체 돌진, 약점 3회 파괴"},
                "땅 룬+바위 룬+희귀 유물"));

            B(new BossDef("BOSS2",2,"심연의 여왕 나이아",5000,
                new[]{AttributeType.Water,AttributeType.Ice,AttributeType.Poison},
                "상반신 여성+하반신 해파리 촉수",
                new[]{
                    "P1(100~60%): 촉수3+레이저+독 웅덩이 / B 얼음 독 동결",
                    "P2(60~30%): 바닥 빙결8초→기둥 생존 / A→B 기둥 던지기",
                    "P3(30~0%): 독수면+축소+촉수6+독안개(B 시야0) / A가 B에 방향 지시"},
                "물+얼음+독 룬+희귀 유물"));

            B(new BossDef("BOSS3",3,"화로의 심판관 이그나투스",8000,
                new[]{AttributeType.Fire,AttributeType.Lightning,AttributeType.Explosion},
                "용암 인간형, 채찍+망치, 가슴 코어",
                new[]{
                    "P1(100~70%): 망치+채찍+브레스 / A 다리, B 발판 코어",
                    "P2(70~30%): 전기 그리드+채찍 끌기+폭발3연 / B 그리드 해독→A 전달",
                    "P3(30~0%): 코어 노출+30초 타이머+용암/낙뢰 / 30초내 코어 파괴"},
                "불+번개+폭발 룬+전설 유물"));

            B(new BossDef("BOSS4",4,"쌍면의 재판관 유디스",12000,
                new[]{AttributeType.Light,AttributeType.Dark,AttributeType.Holy,AttributeType.Curse},
                "2면 인간형, 빛검+어둠낫, 거울 코어",
                new[]{
                    "P1(100~70%): 빛검→전환→어둠낫 / 속성 타일 활용",
                    "P2(70~40%): 경계벽→강제 분리→분신 / A빛/B어둠 동시 처치(HP 연동)",
                    "P3(40~0%): 양면 동시+거울 코어5초 노출 / 코어→기절→전력"},
                "빛+어둠+성스러움+저주 룬+전설 유물"));

            B(new BossDef("BOSS5",5,"심연의 관리자 에테르나",15000,
                new[]{AttributeType.Time,AttributeType.Gravity,AttributeType.Chain},
                "반빛반어둠, 12룬 후광, 모래시계 코어",
                new[]{
                    "P1(100~75%): 빛검+어둠창+교차 레이저 / 타일 속성 활용",
                    "P2(75~50%): 빛/어둠벽→강제 분리→분신 / 동시 처치",
                    "P3(50~25%): 5초 되감기+2배속+시간 폭탄 / 폭탄 반사→보스 경직",
                    "P4(25~0%): 12룬 전속성 난사→무방비 / B 룬 수집→최종 합체 콤보"},
                "시간+중력+연쇄 룬+전설 유물 '미궁 관리자의 인장'+엔딩"));

            B(new BossDef("BOSSH",0,"미궁의 설계자 아키텍트",50000,
                new[]{AttributeType.None},
                "엔드리스 100층+24룬 보유 시 등장. 무속성(전 저항)",
                new[]{
                    "P1: 1~4구역 보스 패턴 빠르게 순환",
                    "P2: 에테르나 패턴+시간/중력 동시",
                    "P3: 미궁 실시간 변형+보스+환경 동시 공략"},
                "설계자의 청사진(변이 1개 거부)+칭호 미궁 정복자"));
            return d;
        }
    }
}
