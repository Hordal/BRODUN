// DungeonGenerator.cs — 절차적 맵 생성 (문서 03-1, 06-5). 시드 기반(멀티 동기화).
using System;
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Dungeon
{
    public class RoomNode
    {
        public int Index;
        public RoomType Type;
        public EventRoomType EventType;
        public Vector2Int GridPos;
        public List<int> Connections = new List<int>();
        public bool Cleared;
    }

    public class FloorLayout
    {
        public int Zone, Floor, Seed;
        public List<RoomNode> Rooms = new List<RoomNode>();
        public int StartIndex, BossIndex;
    }

    /// <summary>시드 기반 층 생성. 같은 시드 → 같은 맵(호스트/클라 일치, 문서 01-4).</summary>
    public class DungeonGenerator : MonoBehaviour
    {
        /// 문서 06-5 절차 생성 6단계 중 1~2단계: 방 그래프 + 유형 배정.
        public FloorLayout Generate(int zone, int floor, int seed)
        {
            var rng = new System.Random(seed);
            var layout = new FloorLayout { Zone = zone, Floor = floor, Seed = seed };

            int roomCount = rng.Next(Constants.ROOMS_PER_FLOOR_MIN, Constants.ROOMS_PER_FLOOR_MAX + 1);
            bool isBossFloor = floor == Constants.FLOORS_PER_ZONE[zone - 1]; // 구역 마지막 층

            // 1) Random Walk로 방 그래프 배치
            var pos = Vector2Int.zero;
            var occupied = new HashSet<Vector2Int>();
            for (int i = 0; i < roomCount; i++)
            {
                var node = new RoomNode { Index = i, GridPos = pos };
                layout.Rooms.Add(node);
                occupied.Add(pos);
                if (i > 0) { node.Connections.Add(i - 1); layout.Rooms[i - 1].Connections.Add(i); }
                pos += RandomStep(rng, occupied, pos);
            }

            // 2) 방 유형 가중치 배정 (문서 03-1)
            layout.StartIndex = 0;
            layout.Rooms[0].Type = RoomType.Start;

            if (isBossFloor)
            {
                layout.BossIndex = roomCount - 1;
                layout.Rooms[layout.BossIndex].Type = RoomType.Boss;
            }

            for (int i = 1; i < roomCount; i++)
            {
                if (layout.Rooms[i].Type == RoomType.Boss) continue;
                layout.Rooms[i].Type = PickRoomType(rng);
                if (layout.Rooms[i].Type == RoomType.Event)
                    layout.Rooms[i].EventType = PickEventType(rng, zone);
            }

            EnsureAltarOrRestBeforeBoss(layout, rng, isBossFloor);
            return layout;
        }

        Vector2Int RandomStep(System.Random rng, HashSet<Vector2Int> occupied, Vector2Int from)
        {
            Vector2Int[] dirs = { Vector2Int.right, Vector2Int.up, Vector2Int.down };
            for (int t = 0; t < 6; t++)
            {
                var d = dirs[rng.Next(dirs.Length)];
                if (!occupied.Contains(from + d)) return d;
            }
            return Vector2Int.right;
        }

        RoomType PickRoomType(System.Random rng)
        {
            int roll = rng.Next(100);
            int acc = 0;
            // Combat40 / Puzzle25 / Trap15 / Event15 / Boss5(층당 1개라 일반 배정 제외)
            acc += Constants.ROOM_WEIGHT[0]; if (roll < acc) return RoomType.Combat;
            acc += Constants.ROOM_WEIGHT[1]; if (roll < acc) return RoomType.Puzzle;
            acc += Constants.ROOM_WEIGHT[2]; if (roll < acc) return RoomType.Trap;
            return RoomType.Event;
        }

        EventRoomType PickEventType(System.Random rng, int zone)
        {
            // 문서 03-1 이벤트 비율
            int roll = rng.Next(100);
            if (roll < 30) return EventRoomType.Altar;
            if (roll < 55) return EventRoomType.Shop;
            if (roll < 75) return EventRoomType.Rest;
            if (roll < 85) return EventRoomType.RuneLab;
            if (roll < 95) return EventRoomType.NPC;
            if (roll < 98) return EventRoomType.SacrificeAltar;
            return zone >= 4 ? EventRoomType.TwinAltar : EventRoomType.Altar; // 쌍둥이는 4구역+
        }

        // 보스 전 제단/휴식 1개 보장(문서 05-2)
        void EnsureAltarOrRestBeforeBoss(FloorLayout layout, System.Random rng, bool isBossFloor)
        {
            if (!isBossFloor) return;
            bool has = layout.Rooms.Exists(r =>
                r.Type == RoomType.Event &&
                (r.EventType == EventRoomType.Altar || r.EventType == EventRoomType.Rest));
            if (!has && layout.Rooms.Count >= 2)
            {
                var r = layout.Rooms[layout.BossIndex - 1];
                r.Type = RoomType.Event;
                r.EventType = rng.Next(2) == 0 ? EventRoomType.Altar : EventRoomType.Rest;
            }
        }

        public static int MakeSeed(int zone, int floor, int runSeed)
            => unchecked(runSeed * 73856093 ^ zone * 19349663 ^ floor * 83492791);
    }
}
