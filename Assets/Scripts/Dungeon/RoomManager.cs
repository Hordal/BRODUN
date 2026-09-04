// RoomManager.cs — 현재 층/방 진행, 방 입장·클리어 (문서 03-1).
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Dungeon
{
    public class RoomManager : MonoBehaviour
    {
        public DungeonGenerator generator;
        public WeatherSystem weather;

        public FloorLayout Current { get; private set; }
        public RoomNode CurrentRoom { get; private set; }
        int _runSeed;

        public void StartRun(int runSeed)
        {
            _runSeed = runSeed;
            EnterFloor(1, 1);
        }

        public void EnterFloor(int zone, int floor)
        {
            int seed = DungeonGenerator.MakeSeed(zone, floor, _runSeed);
            Current = generator.Generate(zone, floor, seed);
            if (Core.GameManager.Instance != null)
            { Core.GameManager.Instance.CurrentZone = zone; Core.GameManager.Instance.CurrentFloor = floor; }
            weather?.RollForFloor(seed); // 층당 50% 환경 변이(문서 05-6)
            EnterRoom(Current.StartIndex);
        }

        public void EnterRoom(int index)
        {
            if (Current == null || index < 0 || index >= Current.Rooms.Count) return;
            CurrentRoom = Current.Rooms[index];
            // 방 유형별 컨텐츠 스폰은 해당 시스템(Combat/Trap/Puzzle/Altar)에 위임
        }

        public void ClearCurrentRoom()
        {
            if (CurrentRoom == null) return;
            CurrentRoom.Cleared = true;
            if (CurrentRoom.Type == RoomType.Boss) OnBossRoomCleared();
        }

        void OnBossRoomCleared()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            int zone = gm.CurrentZone;
            int floor = gm.CurrentFloor;
            gm.OnFloorCleared();

            // 다음 층/구역으로
            if (floor < FloorsInZone(zone)) EnterFloor(zone, floor + 1);
            else if (zone < Constants.ZONE_COUNT) EnterFloor(zone + 1, 1);
            else EventBus.Publish(new BossDefeatedEvent { BossId = "BOSS5", Zone = 5 }); // 엔딩
        }

        public void NextFloor()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            int zone = gm.CurrentZone;
            int floor = gm.CurrentFloor;
            if (floor < FloorsInZone(zone)) EnterFloor(zone, floor + 1);
            else if (zone < Constants.ZONE_COUNT) EnterFloor(zone + 1, 1);
        }

        // 구역 인덱스 안전 조회(0/범위초과 방지).
        static int FloorsInZone(int zone)
            => Constants.FLOORS_PER_ZONE[Mathf.Clamp(zone - 1, 0, Constants.FLOORS_PER_ZONE.Length - 1)];
    }
}
