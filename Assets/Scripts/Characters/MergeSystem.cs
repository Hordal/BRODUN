// MergeSystem.cs — 합체/분리 전환 FSM (문서 01-3). 코어 메카닉.
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Characters
{
    /// <summary>A↔B 합체/분리 상태 전환, 분리 거리 디버프, 자동 합체 복귀 관리.</summary>
    public class MergeSystem : MonoBehaviour
    {
        public WarriorController A;
        public MageController B;

        public MergeState State { get; private set; } = MergeState.Merged;
        public NetMode netMode = NetMode.Single;

        [Header("분리 디버프 (문서 01-3)")]
        public float separationDebuffAtk = 0.85f; // 거리 초과 시 A 공격 배율

        public Transform backMount; // 합체 시 B가 업히는 위치(A 등 자식 Transform)
        Transform _backMount => backMount;

        void OnEnable() => EventBus.Subscribe<BondChangedEvent>(OnBondChanged);
        void OnDisable() => EventBus.Unsubscribe<BondChangedEvent>(OnBondChanged);

        // 합체 중 유대 등급이 오르면 BondBonus를 즉시 갱신(분리→재합체 전까지 stale 방지).
        void OnBondChanged(BondChangedEvent e)
        {
            if (State != MergeState.Merged) return;
            float bonus = BondSystem.Instance ? BondSystem.Instance.StatBonus : 0f;
            if (A != null) A.BondBonus = bonus;
            if (B != null) B.BondBonus = bonus;
        }

        void Update()
        {
            if (State == MergeState.Merged) CarryB();
            else HandleSeparated();
        }

        // ── 합체: A가 B를 등에 업음 ──
        void CarryB()
        {
            if (B == null) return;
            B.transform.position = _backMount ? _backMount.position
                                              : A.transform.position + Vector3.up * 0.8f;
            B.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        // 분리 직후 즉시 재합체 방지 유예(B가 A 등 위치에서 떨어질 시간).
        float _reMergeGrace;

        // ── 분리: 거리 디버프 + 자동 합체 복귀 ──
        void HandleSeparated()
        {
            if (A == null || B == null) return;
            if (_reMergeGrace > 0f) _reMergeGrace -= Time.deltaTime;
            float dist = Vector2.Distance(A.transform.position, B.transform.position);

            // 거리 초과 디버프 (꼼수 방지). 유대 등급에 따라 완화.
            if (dist > Constants.SEPARATION_MAX_DISTANCE)
            {
                float reduce = BondSystem.Instance ? BondSystem.Instance.SeparationReduce : 0f;
                float effective = Mathf.Lerp(separationDebuffAtk, 1f, reduce);
                A.ExternalAtkMultiplier = effective;
                B.ExternalSpeedMultiplier = effective;
            }
            else { A.ExternalAtkMultiplier = 1f; B.ExternalSpeedMultiplier = 1f; }

            // 접촉 시 자동 합체 복귀 (다운 상태면 리바이브). 유예 중엔 무시.
            if (dist <= Constants.MERGE_CONTACT_RADIUS && _reMergeGrace <= 0f)
            {
                if (B.State == DownState.Down) B.ReviveOnMerge();
                if (B.State != DownState.Dead) Merge();
            }
        }

        // ── 전환 (쉬프트 키 / 합체 키) ──
        public void ToggleMerge()
        {
            if (State == MergeState.Merged) Separate();
            else TryMergeManual();
        }

        public void Separate()
        {
            if (State == MergeState.Separated) return;
            if (B.State == DownState.Dead) return; // B 부재 시 분리 불가
            State = MergeState.Separated;
            B.IsSeparatedState = true;
            A.ExternalAtkMultiplier = 1f;
            A.BondBonus = 0f; B.BondBonus = 0f; // 유대 보너스는 합체 시에만(문서 01-6)
            _reMergeGrace = 0.5f; // 즉시 재합체 방지
            // B를 A 옆/아래로 살짝 이격시켜 떨어뜨림
            float side = -Mathf.Sign(A.transform.localScale.x == 0 ? 1 : A.transform.localScale.x);
            B.transform.position = A.transform.position + new Vector3(side * 1.4f, 0.3f, 0);
            EventBus.Publish(new MergeStateChangedEvent { State = State });
        }

        void TryMergeManual()
        {
            if (Vector2.Distance(A.transform.position, B.transform.position) <= Constants.SEPARATION_MAX_DISTANCE)
                Merge();
        }

        public void Merge()
        {
            if (State == MergeState.Merged) return;
            State = MergeState.Merged;
            B.IsSeparatedState = false;
            A.ExternalAtkMultiplier = 1f;
            B.ExternalSpeedMultiplier = 1f;
            // 합체 시 유대 보너스 적용 (데미지 공식의 (1+유대) 항으로)
            float bonus = BondSystem.Instance ? BondSystem.Instance.StatBonus : 0f;
            A.BondBonus = bonus; B.BondBonus = bonus;
            BondSystem.Instance?.Add(Constants.BOND_ON_PROTECT); // 협동 행동
            EventBus.Publish(new MergeStateChangedEvent { State = State });
        }

        public bool IsMerged => State == MergeState.Merged;

        /// 던지기 직후 즉시 재합체 방지(ThrowSystem이 호출).
        public void NotifyThrown() => _reMergeGrace = 0.6f;
    }
}
