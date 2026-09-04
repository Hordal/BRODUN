// ThrowSystem.cs — 던지기 액션 (문서 01-3). A가 B를 집어 던짐, 차지로 파워 조절.
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Characters
{
    public class ThrowSystem : MonoBehaviour
    {
        public WarriorController A;
        public MageController B;
        public MergeSystem merge;

        [Header("던지기 파워 (쉬프트 차지)")]
        public float minPower = 8f;
        public float maxPower = 22f;
        public float maxChargeTime = 1.2f;

        float _charge;
        bool _charging;

        /// 차지 시작 (분리 상태에서 A가 B를 들고 있을 때).
        public void BeginCharge()
        {
            if (merge == null || merge.State != MergeState.Separated) return;
            _charging = true; _charge = 0f;
        }

        public void TickCharge(float dt)
        {
            if (_charging) _charge = Mathf.Min(maxChargeTime, _charge + dt);
        }

        /// 마우스 커서(aimWorld) 방향으로 발사. 파워는 차지 비율.
        public void Release(Vector2 aimWorld)
        {
            if (!_charging) return;
            _charging = false;
            if (A == null || B == null || B.State == DownState.Dead) return;

            float t = _charge / maxChargeTime;
            float power = Mathf.Lerp(minPower, maxPower, t);
            Vector2 dir = (aimWorld - (Vector2)A.transform.position).normalized;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right * A.transform.localScale.x; // 0방향 방지

            // 자동 합체 반경 밖으로 배치 + 던지기 유예(즉시 재합체 방지)
            var rb = B.GetComponent<Rigidbody2D>();
            B.transform.position = A.transform.position + (Vector3)(dir * (Data.Constants.MERGE_CONTACT_RADIUS + 0.5f));
            rb.velocity = dir * power;
            merge?.NotifyThrown();
            // 착지 후 즉시 B 조작 가능 (코옵: 2P가 바로 조작 — 문서 01-4)
        }

        public float ChargeRatio => _charging ? _charge / maxChargeTime : 0f;
    }
}
