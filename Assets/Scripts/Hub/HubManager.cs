// HubManager.cs — 거점 시설 11개 + 확장 단계 (문서 05-1).
using System.Collections.Generic;
using UnityEngine;

namespace BroDungeon.Hub
{
    public enum Facility
    {
        Campfire, Storage, EquipBench, BLab, RuneWorkshop,
        Forge, TrainingGround, Library, FortuneTable, DimensionGate, RecordWall
    }

    public class HubManager : MonoBehaviour
    {
        public static HubManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        // 확장 단계: 구역 클리어 수 → 해금 시설(문서 05-1)
        public int ExpansionStage { get; private set; }

        readonly HashSet<Facility> _unlocked = new HashSet<Facility> { Facility.Campfire, Facility.Storage };

        public bool IsUnlocked(Facility f) => _unlocked.Contains(f);

        public void OnZoneCleared(int zone)
        {
            ExpansionStage = Mathf.Max(ExpansionStage, zone);
            switch (zone)
            {
                case 1: Unlock(Facility.EquipBench, Facility.BLab); break;
                case 2: Unlock(Facility.RuneWorkshop); break;
                case 3: Unlock(Facility.Forge, Facility.TrainingGround); break;
                case 4: Unlock(Facility.Library, Facility.FortuneTable); break;
                case 5: Unlock(Facility.DimensionGate, Facility.RecordWall); break;
            }
        }

        void Unlock(params Facility[] fs) { foreach (var f in fs) _unlocked.Add(f); }

        /// 거점 복귀 시 영구 저장 + 저주 장비 해제 허용(문서 02-7).
        public void OnReturnToHub()
        {
            Core.SaveManager.Instance?.SavePermanent();
        }

        /// 캠프파이어: HP/마나 회복(문서 05-1).
        public void RestAtCampfire(Characters.WarriorController a, Characters.MageController b)
        {
            if (a != null) a.Heal(a.MaxHp);
            if (b != null) b.Heal(b.MaxHp);
        }
    }
}
