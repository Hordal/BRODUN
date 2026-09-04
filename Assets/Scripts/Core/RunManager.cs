// RunManager.cs — 런 시작/종료 라이프사이클 오케스트레이션 (문서 06-7).
// 런 시작: 런버프 초기화 → 영구강화(C풀) 재적용 → 유대 로드 → 던전 시작.
// 런 종료(게임오버): 런버프 제거 → 사망 유실 → 영구 저장.
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;
using BroDungeon.Items;
using BroDungeon.Dungeon;
using BroDungeon.Characters;

namespace BroDungeon.Core
{
    public class RunManager : MonoBehaviour
    {
        public EquipmentSystem equipment;
        public AltarSystem altar;
        public BondSystem bond;
        public RoomManager rooms;

        void OnEnable() => EventBus.Subscribe<GameOverEvent>(OnGameOver);
        void OnDisable() => EventBus.Unsubscribe<GameOverEvent>(OnGameOver);

        /// 새 런 시작. seed=던전 시드(멀티 동기화).
        public void StartRun(GameMode mode, int seed)
        {
            GameManager.Instance?.ResetForNewRun(mode);

            // 이전 런의 런 한정 버프(제단 A/B풀) 제거
            if (equipment != null)
            {
                equipment.ClearExternal(CharacterType.A_Warrior);
                equipment.ClearExternal(CharacterType.B_Mage);
            }

            // 영구 강화(제단 C풀) 재적용
            altar?.ApplyPermanentUpgrades();

            // 유대 영구 수치 로드
            var perm = SaveManager.Instance?.Data.permanent;
            if (bond != null && perm != null) bond.Load(perm.bondPoints);

            // 룬 보유/조합 발견 로드(storage·permanent에서 복원)
            RuneSystem.Instance?.LoadFrom(SaveManager.Instance?.Data.storage);
            RuneSystem.Instance?.LoadDiscovered(perm);

            rooms?.StartRun(seed);
        }

        /// 게임오버 시 자동 호출(EventBus). 런 정리 + 유실 처리.
        void OnGameOver(GameOverEvent _)
        {
            // 런 한정 버프 제거
            if (equipment != null)
            {
                equipment.ClearExternal(CharacterType.A_Warrior);
                equipment.ClearExternal(CharacterType.B_Mage);
            }

            // 유실 처리(문서 06-7): 미장착 인벤 50%, 골드 20%
            InventorySystem.Instance?.ApplyDeathLoss();
            CurrencyManager.Instance?.ApplyDeathLoss();

            // 유대 + 룬 영구 저장
            PersistPermanent();
        }

        /// 거점 복귀(클리어 후) — 영구 저장 + 유대/룬 기록.
        public void OnReturnToHub() => PersistPermanent();

        /// 유대·룬(보유/조합)을 세이브 구조에 기록 후 영구 저장.
        void PersistPermanent()
        {
            var data = SaveManager.Instance?.Data;
            if (data != null)
            {
                if (bond != null) data.permanent.bondPoints = bond.Points;
                RuneSystem.Instance?.SaveTo(data.storage);
                RuneSystem.Instance?.SaveDiscovered(data.permanent);
            }
            SaveManager.Instance?.SavePermanent();
        }
    }
}
