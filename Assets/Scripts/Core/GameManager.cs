// GameManager.cs — 게임 흐름/상태 총괄 싱글턴 (문서 06-5)
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameMode Mode = GameMode.Story;
        public Difficulty Difficulty = Difficulty.Normal;
        public NetMode NetMode = NetMode.Single;
        public bool IsHost => NetMode != NetMode.CoopClient;

        public int CurrentZone = 1, CurrentFloor = 1;
        public bool GameOver { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }

        /// 새 런 시작 시 상태 초기화(RunManager가 호출).
        public void ResetForNewRun(GameMode mode)
        {
            Mode = mode;
            GameOver = false;
            CurrentZone = 1;
            CurrentFloor = 1;
        }

        // ── 게임오버 / B 사망 처리 (문서 01-3, I-06) ──
        public void OnPartyWipe()
        {
            if (GameOver) return;
            GameOver = true;
            EventBus.Publish(new GameOverEvent());
            // 모드별 사망 처리(문서 06-7): 스토리=거점복귀+유실, 엔드리스=완전리셋
            ApplyDeathLoss();
        }

        /// B 단독 사망(8초 미회수). A 단독 디버프 -20% [TBD], 다음 거점까지 B 부재.
        public void OnBDeath()
        {
            Debug.Log("[Game] B 사망 → A 단독 디버프 적용, 다음 거점까지 B 부재");
            // WarriorController.ExternalAtkMultiplier에 (1 - A_SOLO_DEBUFF) 적용은
            // 캐릭터 와이어링 단계에서 연결.
        }

        void ApplyDeathLoss()
        {
            // 게임오버는 런 종료다. 죽은 런을 저장하면 다음 실행에 부활하는 버그가 되므로 삭제.
            // 실제 유실(인벤/골드)·영구 저장은 RunManager.OnGameOver(EventBus)가 처리.
            // (엔드리스는 미궁 결정 지급 — RunManager/엔드리스 매니저에서 처리)
            SaveManager.Instance?.DeleteRun();
        }

        // ── 층/구역 진행 ──
        public void OnFloorCleared()
        {
            EventBus.Publish(new FloorClearedEvent { Zone = CurrentZone, Floor = CurrentFloor });
            SaveManager.Instance?.SaveRun(); // 층 클리어 자동 저장(1회용)
        }

        public void OnBossDefeated(string bossId)
        {
            EventBus.Publish(new BossDefeatedEvent { BossId = bossId, Zone = CurrentZone });
            UnlockSlotForZone(CurrentZone); // 성장: 보스 처치 시 슬롯 해금(문서 02-7)
        }

        void UnlockSlotForZone(int zone)
        {
            var perm = SaveManager.Instance?.Data.permanent;
            if (perm == null) return;
            string key = $"slot_zone{zone}";
            if (!perm.unlockedSlots.Contains(key)) perm.unlockedSlots.Add(key);
        }
    }
}
