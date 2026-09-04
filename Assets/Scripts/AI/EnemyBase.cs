// EnemyBase.cs — 일반 몬스터 베이스 (ICombatant + FSM). 데이터는 MonsterTable에서 주입.
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;
using BroDungeon.Combat;
using BroDungeon.Utilities;

namespace BroDungeon.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBase : MonoBehaviour, ICombatant
    {
        public string monsterId = "M01";
        public int zoneOverride = 0;        // 엔드리스 스케일링용
        public Transform target;            // 추적 대상(A/B)
        public float moveSpeed = 2.5f;
        public float attackRange = 1.2f;
        public float attackDamage = 15f;
        public float attackCooldown = 1.5f;

        [Header("원거리/지원 (Ranged·Support·Flying 등)")]
        public Combat.ProjectilePool projectilePool;
        public LayerMask playerMask;
        public float shootRange = 6f;       // 원거리 사거리
        public float preferredDistance = 4.5f; // 원거리가 유지하려는 거리
        public float healRange = 5f;        // 지원형 회복 범위

        protected MonsterDef Def;
        protected float hp, maxHp, defense = 5f;
        protected StatusController status;
        protected readonly StateMachine fsm = new StateMachine();
        protected float atkTimer;
        protected bool isFlying;

        public EnemyType Type => Def?.Type ?? EnemyType.Melee;
        public bool IsRangedKind => Type == EnemyType.Ranged || Type == EnemyType.Support;

        // ── ICombatant ──
        public Transform Transform => transform;
        public bool IsAlive => hp > 0f;
        public float CurrentHp => hp;
        public float MaxHp => maxHp;
        public virtual float Defense => defense * (1f + (status?.DefenseModifier() ?? 0f) * -1f);
        public StatusController Status => status;

        protected virtual void Awake()
        {
            status = new StatusController(this);
            Def = MonsterTable.Get(monsterId);
            int zone = zoneOverride > 0 ? zoneOverride : (Def?.Zone ?? 1);
            maxHp = Def != null ? MonsterTable.ResolveHp(zone, Def.Hp) : 100f;
            // 엔드리스 스케일링(문서 05-4)
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.Mode == GameMode.Endless)
            {
                int floor = Core.GameManager.Instance.CurrentFloor;
                maxHp *= Constants.EndlessHpMul(floor);
                attackDamage *= Constants.EndlessAtkMul(floor);
            }
            hp = maxHp;

            // 비행형은 중력 무시(부유)
            isFlying = Def != null && Def.Type == EnemyType.Flying;
            if (isFlying) GetComponent<Rigidbody2D>().gravityScale = 0f;

            // 방어형은 정면 방어가 강함(기본 방어 2배)
            if (Def != null && Def.Type == EnemyType.Defense) defense *= 2f;
        }

        protected virtual void Start()
        {
            // 종별 시작 상태
            if (IsRangedKind) fsm.Change(new RangedState(this));
            else if (Type == EnemyType.Charge) fsm.Change(new PatrolState(this));
            else fsm.Change(new PatrolState(this));
        }

        protected virtual void Update()
        {
            status.Tick(Time.deltaTime);
            if (atkTimer > 0f) atkTimer -= Time.deltaTime;
            if (status.CanAct) fsm.Tick(Time.deltaTime);
        }

        public virtual void TakeDamage(float amount, AttributeType attribute, ICombatant source, bool isDoT = false)
        {
            if (!IsAlive) return;
            hp = Mathf.Max(0f, hp - amount);
            if (hp <= 0f) Die(source);
            else if (!isDoT && fsm.Current is PatrolState) // 피격 시 종별 교전 상태
                fsm.Change(IsRangedKind ? (IState)new RangedState(this) : new ChaseState(this));
        }

        public void Heal(float amount) => hp = Mathf.Min(maxHp, hp + amount);
        public void ApplyStatus(StatusInstance s) => status.Apply(s);

        protected virtual void Die(ICombatant killer)
        {
            EventBus.Publish(new EnemyKilledEvent { EnemyId = monsterId, Killer = killer });
            if (Core.GameManager.Instance == null || Core.GameManager.Instance.IsHost)
                LootSystemSafe()?.RollDrops(Def, transform.position);
            Destroy(gameObject);
        }

        Items.LootSystem LootSystemSafe() => Items.LootSystem.Instance;

        // ── FSM 헬퍼 ──
        public bool TargetInRange => target != null && Vector2.Distance(transform.position, target.position) <= attackRange;
        public float TargetDistance => target != null ? Vector2.Distance(transform.position, target.position) : 9999f;

        public void MoveToward(Vector3 pos)
        {
            var rb = GetComponent<Rigidbody2D>();
            float spd = moveSpeed * status.SpeedMultiplier();
            float dirX = Mathf.Sign(pos.x - transform.position.x);
            if (isFlying)
            {
                Vector2 d = ((Vector2)pos - (Vector2)transform.position).normalized;
                rb.velocity = d * spd;
            }
            else rb.velocity = new Vector2(dirX * spd, rb.velocity.y);
            transform.localScale = new Vector3(dirX, 1, 1);
        }

        /// 원거리: 선호 거리 유지(가까우면 후퇴, 멀면 접근).
        public void MaintainDistance(Vector3 pos)
        {
            float dist = Vector2.Distance(transform.position, pos);
            if (dist < preferredDistance * 0.8f) MoveToward(transform.position * 2f - pos); // 후퇴
            else if (dist > preferredDistance) MoveToward(pos);
            else { var rb = GetComponent<Rigidbody2D>(); rb.velocity = new Vector2(0, isFlying ? 0 : rb.velocity.y); }
        }

        public virtual void PerformAttack()
        {
            if (atkTimer > 0f || target == null) return;
            atkTimer = attackCooldown;
            var c = target.GetComponentInParent<ICombatant>();
            if (c == null || !c.IsAlive) return;
            var req = MakeReq(c, attackDamage);
            CombatManager.Instance.ProcessAttack(in req);
        }

        /// 원거리 투사체 공격.
        public void PerformRangedAttack()
        {
            if (atkTimer > 0f || target == null) return;
            atkTimer = attackCooldown;
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            if (projectilePool != null)
                projectilePool.Fire(this, MakeReq(null, attackDamage), transform.position, dir, playerMask);
            else // 풀 없으면 직접 판정(근사)
            {
                var c = target.GetComponentInParent<ICombatant>();
                if (c != null && c.IsAlive) { var req = MakeReq(c, attackDamage); CombatManager.Instance.ProcessAttack(in req); }
            }
        }

        /// 지원형: 범위 내 가장 체력 낮은 아군 회복.
        public void PerformSupport()
        {
            if (atkTimer > 0f) return;
            atkTimer = attackCooldown;
            EnemyBase best = null; float worst = 1f;
            foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
            {
                if (e == this || !e.IsAlive) continue;
                if (Vector2.Distance(transform.position, e.transform.position) > healRange) continue;
                float r = e.hp / e.maxHp;
                if (r < worst) { worst = r; best = e; }
            }
            if (best != null) best.Heal(best.maxHp * 0.1f);
        }

        /// 돌진형: 타겟 방향으로 가속 돌진.
        public void DoCharge()
        {
            if (atkTimer > 0f || target == null) return;
            atkTimer = attackCooldown * 1.5f;
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            GetComponent<Rigidbody2D>().velocity = dir * moveSpeed * 4f;
        }

        AttackRequest MakeReq(ICombatant c, float dmg) => new AttackRequest
        {
            Source = this, Target = c, BaseAtk = dmg,
            CritChance = 0f, CritDamage = 1f, Attribute = Def?.Attr1 ?? AttributeType.None
        };

        public void ChangeState(IState s) => fsm.Change(s);
    }
}
