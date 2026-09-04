// PuzzleTable.cs — 퍼즐 30종 데이터 (문서 03-4). 구역별 6×5.
using System.Collections.Generic;

namespace BroDungeon.Dungeon
{
    public struct PuzzleDef
    {
        public string Id, Name, Mechanic, Solution;
        public int Zone;
        public PuzzleDef(string id, int zone, string n, string mech, string sol)
        { Id = id; Zone = zone; Name = n; Mechanic = mech; Solution = sol; }
    }

    public static class PuzzleTable
    {
        public static readonly Dictionary<string, PuzzleDef> All = Build();
        public static PuzzleDef Get(string id) => All[id];

        static Dictionary<string, PuzzleDef> Build()
        {
            var d = new Dictionary<string, PuzzleDef>();
            void P(string id,int z,string n,string m,string s)=>d[id]=new PuzzleDef(id,z,n,m,s);
            // 1구역 입문 6
            P("P01",1,"무거운 압력판","합체","합체로 무거운 판, 상자로 가벼운 판");
            P("P02",1,"좁은 균열","분리+던지기","B 던져 균열 너머 레버");
            P("P03",1,"이중 레버","분리+전환","A/B 배치→동시 조작");
            P("P04",1,"바위 밀기","합체(힘)","합체 A 힘으로");
            P("P05",1,"감정 비밀문","B 고유","B 문자 해독→비밀 방");
            P("P06",1,"발판 유지","분리+무게","B 발판→A 건넘→회수");
            // 2구역 활용 6
            P("P07",2,"수위 조절","던지기+분리","B 높은 레버 던짐→수위 조절→발판");
            P("P08",2,"얼음 징검다리","속성(얼음)","B 얼음→수면 동결→A 이동(5초)");
            P("P09",2,"독안개 미로","분리+B 고유","B 밖에서 지도→A에 방향");
            P("P10",2,"수중 스위치","합체+무게","합체(무거움) 잠수→스위치");
            P("P11",2,"거울 반사","속성+분리","B 광원→A 거울 회전");
            P("P12",2,"수류 타기","합체+타이밍","B 경로 판단→A 점프");
            // 3구역 심화 6
            P("P13",3,"전기 회로","속성(번개)+분리","B 전기 공급→A 블록 밀어 회로 완성");
            P("P14",3,"용암 징검","던지기+타이밍","A 타이밍 B 던짐→B 레버");
            P("P15",3,"폭발 해체","B 고유+시간 제한","A 보호+B 배선 미니게임");
            P("P16",3,"컨베이어 상자","합체+분리 전환","A 밀기→B 정지 버튼");
            P("P17",3,"화염/빙결 통로","속성(불+얼음)","불→얼음 녹이기, 얼음→화염 소화");
            P("P18",3,"톱니 회전","합체(힘)+B 고유","A 회전(힘)+B 정답 분석");
            // 4구역 고급 6
            P("P19",4,"빛/어둠 확장","속성+분리","강제 분리, A 빛 B 어둠 동시 스위치");
            P("P20",4,"그림자 다리","분리+위치","B 빛→A 그림자 실체화=다리");
            P("P21",4,"저주 해독","속성(성스러움)+B","B 분석→역순→성스러움 해제");
            P("P22",4,"나비와 나방","관찰+판단","B만 구분→경로 안내");
            P("P23",4,"빛/어둠 거울","속성+분리+힘","A 거울(힘)+B 방향 분석");
            P("P24",4,"경계선 이동","분리+속성","A 빛+B 어둠 동시 밀기");
            // 5구역 최종 6
            P("P25",5,"시간 역행","시간 룬","A 이동→B 시간 고정→반복");
            P("P26",5,"중력 반전","중력 룬+분리","B를 반전 영역 던짐→천장 레버");
            P("P27",5,"차원 문 순서","포탈+B 고유","B 연결선 해독→순서 통과");
            P("P28",5,"룬 제단","전속성 룬","B 해독→룬 배치→보스방 개방");
            P("P29",5,"분신 협동","전 메카닉","시간 분신+4스위치 동시 조작");
            P("P30",5,"모래시계 탈출","전+시간 제한","60초 내 1~4구역 퍼즐 축약판");
            return d;
        }
    }
}
