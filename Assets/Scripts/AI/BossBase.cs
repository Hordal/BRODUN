// BossBase.cs — 보스 베이스 (페이즈 FSM + 액션 스케줄러). 문서 04-2.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;
using BroDungeon.Combat;
using BroDungeon.Utilities;

namespace BroDungeon.AI
{
    public class BossBase : MonoBehaviour, ICombatant
    {
        public string bossId = "BOSS1";
        public Transform target;

        [Header("전투")]
        public LayerMask playerMask;
        public Combat.ProjectilePool projectilePool;
        public EnemyBase summonPrefab; // 소환용(없으면 소환 스킵)

        protected BossDef Def;
        protected float hp, maxHp, defense = 10f;
        protected float atkBase = 60f;
        protected StatusController status;
        public int Phase { get; protected set; } = 1;

        float[] _thresholds;                 // 페이즈 전환 HP 비율
        List<BossAction>[] _phaseActions;    // 페이즈별 액션
        int _actionIdx;                      // 현재 페이즈 내 액션 인덱스
        int _step;                           // 0=선택,1=예고,2=후딜
        float _stepTimer;
        BossAction _cur;

        public Transform Transform => transform;
        public bool IsAlive => hp > 0f;
        public float CurrentHp => hp;
        public float MaxHp => maxHp;
        public virtual float Defense => defense;
        public StatusController Status => status;

        protected virtual void Awake()
        {
            status = new StatusController(this);
            Def = BossTable.Get(bossId);
            maxHp = Def?.MaxHp ?? 3000f;
            hp = maxHp;

            int zone = Def?.Zone ?? 1;
            atkBase = zone <= 0 ? 200f : new[] { 60f, 60f, 90f, 120f, 150f, 180f }[Mathf.Clamp(zone, 0, 5)];

            // 페이즈 수 기반 임계값(균등 분할)
            int phases = Def != null && Def.PhasePatterns.Length > 0 ? Def.PhasePatterns.Length : 3;
            _thresholds = new float[phases - 1];
            for (int i = 0; i < _thresholds.Length; i++) _thresholds[i] = (float)(phases - 1 - i) / phases;

            _phaseActions = BossPatternLibrary.Build(bossId, atkBase);
        }

        protected virtual void Update()
        {
            status.Tick(Time.deltaTime);
            if (status.CanAct) RunScheduler(Time.deltaTime);
        }

        public virtual void TakeDamage(float amount, AttributeType attribute, ICombatant source, bool isDoT = false)
        {
            if (!IsAlive) return;
            if (status.Invulnerable) return; // ShieldUp 중 무적
            hp = Mathf.Max(0f, hp - amount);
            if (hp <= 0f) { Die(source); return; } // 사망 우선: 죽는 프레임에 최종 페이즈 전환 연출 방지
            CheckPhase();
        }

        public void Heal(float amount) => hp = Mathf.Min(maxHp, hp + amount);
        public void ApplyStatus(StatusInstance s) => status.Apply(s);

        void CheckPhase()
        {
            float ratio = hp / maxHp;
            for (int i = 0; i < _thresholds.Length; i++)
                if (ratio > _thresholds[i]) { SetPhase(i + 1); return; }
            SetPhase(_thresholds.Length + 1);
        }

        protected virtual void SetPhase(int p)
        {
            if (p <= Phase) return; // 페이즈는 단조 증가(회복 시 역행·스케줄러 리셋 방지)
            Phase = p;
            _actionIdx = 0; _step = 0; _stepTimer = 0f; // 스케줄러 리셋
            OnPhaseChanged(p);
        }

        protected virtual void OnPhaseChanged(int phase)
            => Debug.Log($"[Boss {bossId}] 페이즈 {phase}: {GetPhaseDesc(phase)}");

        public string GetPhaseDesc(int phase)
            => Def != null && phase - 1 < Def.PhasePatterns.Length ? Def.PhasePatterns[phase - 1] : "";

        // ── 액션 스케줄러: 선택 → 예고 → 실행 → 후딜 반복 ──
        void RunScheduler(float dt)
        {
            var actions = CurrentPhaseActions();
            if (actions == null || actions.Count == 0) return;

            switch (_step)
            {
                case 0: // 다음 액션 선택
                    _cur = actions[_actionIdx % actions.Count];
                    _actionIdx++;
                    _stepTimer = _cur.Telegraph;
                    _step = 1;
                    OnTelegraph(_cur);
                    break;
                case 1: // 예고 대기
                    _stepTimer -= dt;
                    if (_stepTimer <= 0f) { ExecuteAction(_cur); _stepTimer = _cur.Recovery; _step = 2; }
                    break;
                case 2: // 후딜
                    _stepTimer -= dt;
                    if (_stepTimer <= 0f) _step = 0;
                    break;
            }
        }

        List<BossAction> CurrentPhaseActions()
        {
            if (_phaseActions == null) return null;
            int idx = Mathf.Clamp(Phase - 1, 0, _phaseActions.Length - 1);
            return _phaseActions[idx];
        }

        protected virtual void OnTelegraph(BossAction a) { /* VFX/예고 연출 훅 */ }

        void ExecuteAction(BossAction a)
        {
            Vector3 pos = transform.position;
            Vector2 toTarget = target != null ? ((Vector2)target.position - (Vector2)pos) : Vector2.right;
            Vector2 aim = toTarget.normalized;

            switch (a.Type)
            {
                case BossActionType.MeleeArc:
                    CombatManager.Instance.AreaAttack(pos + (Vector3)(aim * a.Radius * 0.5f), a.Radius, Req(a), playerMask);
                    break;
                case BossActionType.AreaSlam:
                    CombatManager.Instance.AreaAttack(pos, a.Radius, Req(a), playerMask);
                    break;
                case BossActionType.Beam:
                    // 근사: 타겟 방향 일직선 상의 여러 지점 광역
                    for (int i = 1; i <= 5; i++)
                        CombatManager.Instance.AreaAttack(pos + (Vector3)(aim * i * a.Radius / 5f), a.Radius / 4f, Req(a), playerMask);
                    break;
                case BossActionType.ProjectileAimed:
                    for (int i = 0; i < Mathf.Max(1, a.Count); i++) FireProjectile(aim, a);
                    break;
                case BossActionType.ProjectileSpread:
                    FireSpread(aim, a);
                    break;
                case BossActionType.ShieldUp:
                    ApplyStatus(new StatusInstance { Type = StatusType.Invuln, Duration = a.Param });
                    break;
                case BossActionType.Charge:
                    var rb = GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = aim * Mathf.Max(8f, a.Param);
                    CombatManager.Instance.AreaAttack(pos, a.Radius, Req(a), playerMask);
                    break;
                case BossActionType.Summon:
                    Summon(a);
                    break;
            }
        }

        void FireSpread(Vector2 aim, BossAction a)
        {
            int n = Mathf.Max(1, a.Count);
            float spread = 60f;
            float baseAng = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            for (int i = 0; i < n; i++)
            {
                float t = n == 1 ? 0.5f : (float)i / (n - 1);
                float ang = baseAng + Mathf.Lerp(-spread / 2f, spread / 2f, t);
                Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                FireProjectile(dir, a);
            }
        }

        void FireProjectile(Vector2 dir, BossAction a)
        {
            if (projectilePool != null)
                projectilePool.Fire(this, Req(a), transform.position, dir, playerMask);
            else if (target != null) // 풀 없으면 직접 판정 근사
            {
                var c = target.GetComponentInParent<ICombatant>();
                if (c != null && c.IsAlive) CombatManager.Instance.ProcessAttack(Req(a, c));
            }
        }

        void Summon(BossAction a)
        {
            if (summonPrefab == null) return;
            for (int i = 0; i < Mathf.Max(1, a.Count); i++)
            {
                var e = Instantiate(summonPrefab, transform.position + Vector3.right * (i - a.Count / 2f), Quaternion.identity);
                if (!string.IsNullOrEmpty(a.SummonId)) e.monsterId = a.SummonId;
                e.target = target;
            }
        }

        AttackRequest Req(BossAction a, ICombatant c = null) => new AttackRequest
        {
            Source = this, Target = c, BaseAtk = a.Damage,
            CritChance = 0f, CritDamage = 1f, Attribute = a.Attr
        };

        protected virtual void Die(ICombatant killer)
        {
            EventBus.Publish(new BossDefeatedEvent { BossId = bossId, Zone = Def?.Zone ?? 0 });
            Items.LootSystem.Instance?.GrantBossReward(Def);
            Core.GameManager.Instance?.OnBossDefeated(bossId);
            Destroy(gameObject);
        }
    }
}
