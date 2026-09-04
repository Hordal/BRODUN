// MageController.cs — 캐릭터 B(다리 불편한 법사). 원거리 마법/감정/기기/다운-리바이브 (문서 01-2,3)
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;
using BroDungeon.Utilities;

namespace BroDungeon.Characters
{
    public class MageController : CharacterBase
    {
        public override CharacterType Type => CharacterType.B_Mage;

        [Header("B 고유 (문서 01-2: 자력 이동 불가, 기는 것만)")]
        public float crawlSpeed = 2.5f; // 분리 시 느린 기기 이동
        public LayerMask enemyMask;

        [Header("다운/리바이브 (문서 01-3, I-06)")]
        public DownState State = DownState.Alive;
        readonly Timer _downTimer = new Timer();

        public float Mana { get; private set; } = Constants.B_BASE_MANA;
        public float MaxMana { get; private set; } = Constants.B_BASE_MANA;
        float _baseMaxMana = Constants.B_BASE_MANA;

        // 캐스팅/쿨감 개인 스탯 (문서 01-2) — 스탯 시트에서 파생
        public float CastTimeMul => Stats.CastTimeMul;
        public float CooldownReduction => Stats.CooldownReduction;

        // 마나 소비 배율(저주: 폭주의 지팡이 2배 소비 등). 1 = 정상.
        public float ManaCostMultiplier = 1f;

        // 저주 속박의 목걸이(CB04): 분리 시 스킬 불가.
        public bool SkillLockWhenSeparated;
        public bool IsSeparatedState; // MergeSystem이 갱신

        protected override void Awake()
        {
            baseMaxHp = Constants.B_BASE_HP;
            maxHp = Constants.B_BASE_HP;
            baseAtk = Constants.B_BASE_ATK;
            baseDefense = Constants.B_BASE_DEF;
            base.Awake();
            Mana = MaxMana;
        }

        /// 장비/스탯 변경 시 최대 마나 재계산(HP와 함께 RecomputeDerived에서 호출).
        protected override void OnDerivedRecomputed()
        {
            float ratio = MaxMana > 0f ? Mana / MaxMana : 1f;
            MaxMana = Stats.FinalMaxMana(_baseMaxMana);
            Mana = Mathf.Clamp(MaxMana * ratio, 0f, MaxMana);
        }

        protected override void Update()
        {
            base.Update();
            if (State == DownState.Down) _downTimer.Tick(Time.deltaTime);
            if (Mana < MaxMana) Mana = Mathf.Min(MaxMana, Mana + ManaRegen * Time.deltaTime);
        }
        public float ManaRegen = 5f;

        /// 분리 시 기기 이동 (느림). 합체 시엔 A가 운반하므로 미사용.
        public void Crawl(float axis)
        {
            if (State != DownState.Alive || !StatusCtrl.CanAct) return;
            float spd = crawlSpeed * Stats.MoveSpeedMul * StatusCtrl.SpeedMultiplier() * ExternalSpeedMultiplier;
            Body.velocity = new Vector2(axis * spd, Body.velocity.y);
        }

        public bool TrySpendMana(float cost)
        {
            if (Mana < cost) return false;
            Mana -= cost; return true;
        }

        // ── 다운/리바이브 (I-06) ──
        protected override void OnHpZero(ICombatant source)
        {
            // 분리 모드에서 B 단독 0 → 다운. 합체 모드는 A가 흡수하므로 여기 도달 안 함.
            EnterDown();
        }

        void EnterDown()
        {
            if (State != DownState.Alive) return;
            State = DownState.Down;
            EventBus.Publish(new CharacterDownedEvent { IsB = true });
            BondSystem.Instance?.Add(Constants.BOND_ON_B_HIT); // 피격 -2
            _downTimer.Start(Constants.B_DOWN_DURATION, OnDownExpired); // 8초 [TBD]
        }

        void OnDownExpired()
        {
            // 8초 내 미회수 → 사망 (문서 01-3)
            State = DownState.Dead;
            BondSystem.Instance?.Add(Constants.BOND_ON_B_DEATH); // -10
            GameManagerRef?.OnBDeath(); // A 단독 디버프 -20% [TBD], 다음 거점까지 B 부재
        }

        /// A 접촉 회수 시 호출 (MergeSystem). HP 30% 부활 [TBD].
        public void ReviveOnMerge()
        {
            if (State != DownState.Down) return;
            _downTimer.Stop();
            State = DownState.Alive;
            SetCurrentHp(maxHp * Constants.B_REVIVE_HP_RATIO); // HP 30% 부활 [TBD]
            BondSystem.Instance?.Add(-2); // 유대 -2 (문서 01-3 표)
            EventBus.Publish(new CharacterRevivedEvent { IsB = true });
        }

        public float DownRemaining => _downTimer.Remaining;
        public override bool IsAlive => State != DownState.Dead && CurrentHp > 0f || State == DownState.Down;

        public BroDungeon.Core.GameManager GameManagerRef;
    }
}
