// WarriorController.cs — 캐릭터 A(거구의 전사). 이동/점프/대쉬/근접콤보/방어/운반 (문서 01-2,3,5)
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;

namespace BroDungeon.Characters
{
    public class WarriorController : CharacterBase
    {
        public override CharacterType Type => CharacterType.A_Warrior;

        [Header("이동 (문서 01: 신체능력 우수)")]
        public float moveSpeed = 6f;
        public float jumpForce = 14f;
        public float dashForce = 18f;
        public float dashCooldown = 1.2f;
        public float groundCheckDistance = 1.1f;
        public LayerMask groundMask;
        public LayerMask enemyMask;

        [Header("근접 콤보 (문서 01-5: 강력한 콤보)")]
        public Transform attackOrigin;
        public float attackRadius = 1.5f;
        public float comboResetTime = 0.8f;

        [Header("방어")]
        public bool isBlocking;

        float _dashTimer;
        float _comboTimer;
        bool _grounded;

        ComboSystem _combo;
        public float Stamina { get; private set; } = Constants.A_BASE_STAMINA;
        public float MaxStamina => Constants.A_BASE_STAMINA + Stats.Get(BroDungeon.Combat.StatType.StaminaFlat);

        protected override void Awake()
        {
            baseMaxHp = Constants.A_BASE_HP;
            maxHp = Constants.A_BASE_HP;
            baseAtk = Constants.A_BASE_ATK;
            baseDefense = Constants.A_BASE_DEF;
            base.Awake();
            _combo = new ComboSystem(comboResetTime);
        }

        protected override void Update()
        {
            base.Update();
            if (_dashTimer > 0f) _dashTimer -= Time.deltaTime;
            _combo.Tick(Time.deltaTime);
            CheckGrounded();
        }

        // ── 입력 진입점 (InputManager / Netcode가 호출) ──
        public void Move(float axis)
        {
            if (!StatusCtrl.CanAct) return;
            float spd = moveSpeed * Stats.MoveSpeedMul * StatusCtrl.SpeedMultiplier() * ExternalSpeedMultiplier;
            Body.velocity = new Vector2(axis * spd, Body.velocity.y);
            if (Mathf.Abs(axis) > 0.01f) transform.localScale = new Vector3(Mathf.Sign(axis), 1, 1);
        }

        public void Jump()
        {
            if (_grounded && StatusCtrl.CanAct)
                Body.velocity = new Vector2(Body.velocity.x, jumpForce);
        }

        public void Dash()
        {
            if (_dashTimer > 0f || Stamina < 20f || !StatusCtrl.CanAct) return;
            _dashTimer = dashCooldown;
            Stamina -= 20f;
            Body.velocity = new Vector2(transform.localScale.x * dashForce, Body.velocity.y);
        }

        /// 근접 공격(콤보). 3콤보째 특수 효과 트리거(무기 SO 연동).
        public void Attack()
        {
            if (!StatusCtrl.CanAct) return;
            int step = _combo.Advance();
            var hits = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, enemyMask);
            foreach (var h in hits)
            {
                var c = h.GetComponentInParent<ICombatant>();
                if (c == null || !c.IsAlive) continue;
                var req = new AttackRequest
                {
                    Source = this, Target = c,
                    BaseAtk = FinalAttackBase * ExternalAtkMultiplier,
                    GearAtk = 0f, AltarBonus = AltarAttackBonus,
                    SkillMul = 0f, AttributeBonus = AttributeSynergyBonus, BondBonus = BondBonus,
                    CritChance = CritChanceTotal, CritDamage = CritDamageTotal,
                    PierceRatio = PierceTotal,
                    Attribute = WeaponAttribute
                };
                CombatManager.Instance.ProcessAttack(in req);
            }
            if (step >= 3) On3rdComboHit?.Invoke(attackOrigin.position);
        }

        public System.Action<Vector3> On3rdComboHit; // 무기별 3콤보 폭발 등
        public AttributeType WeaponAttribute = AttributeType.None;

        // 저주 광전사의 부적(CA06): 저체력일수록 공격↑ (최대 +80%).
        public bool LowHpRageActive;
        // 저주 고독의 팔찌(CA04): 분리 시 +50% / 합체 시 -20% (CursedItemHandler가 갱신).
        public float CursedConditionalAtkPct;
        protected override float ExtraAttackPct =>
            (LowHpRageActive ? Mathf.Lerp(0f, 0.80f, 1f - currentHp / Mathf.Max(1f, maxHp)) : 0f)
            + CursedConditionalAtkPct;

        public void SetBlock(bool on) => isBlocking = on;
        public override float Defense => isBlocking ? base.Defense * 2f : base.Defense;

        void CheckGrounded()
        {
            _grounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundMask);
            if (_grounded && Stamina < MaxStamina)
                Stamina = Mathf.Min(MaxStamina, Stamina + 10f * Time.deltaTime);
        }

        protected override void OnHpZero(ICombatant source)
        {
            // 문서 01-3: A HP 0 → 게임오버
            GameManagerRef?.OnPartyWipe();
        }

        public BroDungeon.Core.GameManager GameManagerRef;
    }
}
