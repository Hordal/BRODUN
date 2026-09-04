// ParrySystem.cs — 분리 패리 (문서 01-3,5). 공격받는 순간 분리하면 패리 판정.
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Characters;
using BroDungeon.Utilities;

namespace BroDungeon.Combat
{
    public class ParrySystem : MonoBehaviour
    {
        public MergeSystem merge;

        [Header("판정 윈도우 (문서: 타이밍 기반 고급 테크닉)")]
        public float parryWindow = 0.2f;     // 기본 판정 시간
        public float extraWindow = 0f;       // 유물 R12 패리의 직감 +0.3초

        float _windowTimer;
        public bool WindowOpen => _windowTimer > 0f;

        void Update()
        {
            if (_windowTimer > 0f) _windowTimer -= Time.deltaTime;
        }

        /// 분리 입력 순간 호출 → 패리 윈도우 오픈.
        public void OnSeparateInput()
        {
            _windowTimer = parryWindow + extraWindow;
        }

        /// 피격 직전 호출. 윈도우 내면 패리 성공(피해 무효 + 유대 +).
        public bool TryParry(ICombatant attacker)
        {
            if (!WindowOpen) return false;
            _windowTimer = 0f;
            BondSystem.Instance?.Add(Constants.BOND_ON_PARRY);
            // 반격/경직은 attacker 상태이상으로 부여
            attacker?.ApplyStatus(new StatusInstance { Type = StatusType.Stun, Duration = 1f });
            return true;
        }
    }
}
