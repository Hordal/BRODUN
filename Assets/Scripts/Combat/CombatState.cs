// CombatState.cs — 전역 전투 상태 추적 (저주 비전투 드레인 등 조건부 효과용).
using UnityEngine;

namespace BroDungeon.Combat
{
    public static class CombatState
    {
        static float _lastCombatTime = -999f;
        public const float CombatTimeout = 4f; // 마지막 전투 후 이 시간까지 '전투 중'

        /// 공격/피격 발생 시 호출(CombatManager에서 자동).
        public static void Mark() => _lastCombatTime = Time.time;

        public static bool InCombat => Time.time - _lastCombatTime < CombatTimeout;
        public static bool OutOfCombat => !InCombat;
    }
}
