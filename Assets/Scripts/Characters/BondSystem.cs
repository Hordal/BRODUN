// BondSystem.cs — 유대 시스템 (문서 01-6). 런 간 유지되는 영구 수치.
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Characters
{
    public class BondSystem : MonoBehaviour
    {
        public static BondSystem Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else { Destroy(gameObject); return; } }

        [SerializeField] int _points;
        public int Points => _points;
        public int Tier => CalcTier(_points); // 0~4 (등급 1~5)

        /// 합체 시 전 스탯 보너스 배율 (문서 01-6).
        public float StatBonus => Constants.BOND_STAT_BONUS[Tier];
        /// 분리 디버프 감소율.
        public float SeparationReduce => Constants.BOND_SEP_REDUCE[Tier];
        /// 콤보 보너스.
        public float ComboBonus => Constants.BOND_COMBO_BONUS[Tier];

        public bool HasComboSmash => Tier >= 2;   // 3등급
        public bool HasBondBarrier => Tier >= 3;  // 4등급
        public bool HasUltimate => Tier >= 4;     // 5등급

        public void Add(int delta)
        {
            int old = Tier;
            _points = Mathf.Max(0, _points + delta);
            EventBus.Publish(new BondChangedEvent { Points = _points, Tier = Tier });
            if (Tier > old) OnTierUp(Tier);
        }

        static int CalcTier(int pts)
        {
            for (int i = Constants.BOND_TIER_MIN.Length - 1; i >= 0; i--)
                if (pts >= Constants.BOND_TIER_MIN[i]) return i;
            return 0;
        }

        void OnTierUp(int tier)
        {
            // 해금: 1→대화, 2→합체 강타, 3→유대의 방벽, 4→궁극
            Debug.Log($"[Bond] 유대 등급 상승 → {tier + 1}등급");
        }

        public void Load(int points) => _points = points;
    }
}
