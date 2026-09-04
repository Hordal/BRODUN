// EnemyStates.cs — 순찰/추적/공격 상태 (문서 06-5 FSM)
using UnityEngine;

namespace BroDungeon.AI
{
    public class PatrolState : IState
    {
        readonly EnemyBase _e;
        float _dir = 1f, _switch;
        public PatrolState(EnemyBase e) { _e = e; }

        public void Enter() { _switch = 2f; }
        public void Tick(float dt)
        {
            _switch -= dt;
            if (_switch <= 0f) { _dir = -_dir; _switch = 2f; }
            _e.MoveToward(_e.transform.position + Vector3.right * _dir);

            // 시야 내 타겟 발견 → 종별 상태로 전환
            if (_e.target != null && _e.TargetDistance < Mathf.Max(6f, _e.shootRange))
                _e.ChangeState(_e.IsRangedKind ? (IState)new RangedState(_e) : new ChaseState(_e));
        }
        public void Exit() { }
    }

    public class ChaseState : IState
    {
        readonly EnemyBase _e;
        public ChaseState(EnemyBase e) { _e = e; }

        public void Enter() { }
        public void Tick(float dt)
        {
            if (_e.target == null) { _e.ChangeState(new PatrolState(_e)); return; }
            if (_e.TargetInRange) { _e.ChangeState(new AttackState(_e)); return; }
            if (_e.TargetDistance > 10f) { _e.ChangeState(new PatrolState(_e)); return; }
            _e.MoveToward(_e.target.position);
        }
        public void Exit() { }
    }

    public class AttackState : IState
    {
        readonly EnemyBase _e;
        public AttackState(EnemyBase e) { _e = e; }

        public void Enter() { }
        public void Tick(float dt)
        {
            if (!_e.TargetInRange) { _e.ChangeState(new ChaseState(_e)); return; }
            if (_e.Type == BroDungeon.Data.EnemyType.Charge) _e.DoCharge();
            else _e.PerformAttack();
        }
        public void Exit() { }
    }

    /// 원거리/지원형: 선호 거리 유지하며 투사체/회복.
    public class RangedState : IState
    {
        readonly EnemyBase _e;
        public RangedState(EnemyBase e) { _e = e; }

        public void Enter() { }
        public void Tick(float dt)
        {
            if (_e.target == null) { _e.ChangeState(new PatrolState(_e)); return; }
            float dist = _e.TargetDistance;

            // 지원형은 사거리 무관하게 아군 회복 우선
            if (_e.Type == BroDungeon.Data.EnemyType.Support) { _e.MaintainDistance(_e.target.position); _e.PerformSupport(); return; }

            if (dist > _e.shootRange * 1.3f) { _e.ChangeState(new PatrolState(_e)); return; }
            _e.MaintainDistance(_e.target.position);
            if (dist <= _e.shootRange) _e.PerformRangedAttack();
        }
        public void Exit() { }
    }
}
