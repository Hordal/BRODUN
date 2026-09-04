// CharacterBase.cs — A/B 공통 캐릭터 베이스 (ICombatant 구현)
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Combat;
using BroDungeon.Utilities;

namespace BroDungeon.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class CharacterBase : MonoBehaviour, ICombatant
    {
        [Header("기본 스탯 (런타임 적용 전 SO/Constants에서 주입)")]
        public float maxHp;            // 최종 최대 HP(스탯 반영 후)
        protected float baseMaxHp;     // 기본 최대 HP(불변 기준값)
        public float baseAtk;
        public float baseDefense;
        [SerializeField] protected float currentHp;

        public abstract CharacterType Type { get; }

        protected Rigidbody2D Body;
        protected StatusController StatusCtrl;

        // ── 스탯/보너스 (EquipmentSystem·MergeSystem·AltarSystem이 주입) ──
        public StatSheet Stats { get; } = new StatSheet();
        public float AttributeSynergyBonus;  // (1+속성) 항 (시너지)
        public float AltarAttackBonus;       // 제단 가산 공격
        public float BondBonus;              // (1+유대) 항 (합체 시)

        // ── ICombatant ──
        public Transform Transform => transform;
        public virtual bool IsAlive => currentHp > 0f;
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        // 장비 방어 + 저주/디버프(StatusController.DefenseModifier) 반영.
        public virtual float Defense
            => Stats.FinalDefense(baseDefense) * (1f - (StatusCtrl?.DefenseModifier() ?? 0f));
        public StatusController Status => StatusCtrl;

        // 분리 패리(문서 01-3,5). 피격 순간 윈도우 내면 무효화. 부트스트랩이 주입.
        public Combat.ParrySystem parry;

        // ── 공격 계산용 파생값 (장비/스탯 반영) ──
        // ExtraAttackPct: 조건부 보너스(예: 저주 광전사 저체력 분노). 서브클래스가 재정의.
        protected virtual float ExtraAttackPct => 0f;
        public float FinalAttackBase => Stats.FinalAttack(baseAtk) * (1f + ExtraAttackPct);
        public float CritChanceTotal => Stats.CritChance;
        public float CritDamageTotal => Stats.CritDamage;
        public float PierceTotal => Stats.Pierce;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            StatusCtrl = new StatusController(this);
            if (baseMaxHp <= 0f) baseMaxHp = maxHp;
            RecomputeDerived();
            currentHp = maxHp;
        }

        /// 장비/제단 변경 시 최대 HP 등 파생 스탯 재계산(현재 HP 비율 유지).
        public void RecomputeDerived()
        {
            float ratio = maxHp > 0f ? currentHp / maxHp : 1f;
            maxHp = Stats.FinalMaxHp(baseMaxHp);
            currentHp = Mathf.Clamp(maxHp * ratio, 0f, maxHp);
            OnDerivedRecomputed();
        }
        protected virtual void OnDerivedRecomputed() { }

        protected virtual void Update()
        {
            StatusCtrl.Tick(Time.deltaTime);
        }

        public virtual void TakeDamage(float amount, AttributeType attribute, ICombatant source, bool isDoT = false)
        {
            if (!IsAlive) return;
            // 분리 패리: 피격 순간 윈도우가 열려 있으면 무효화(DoT 제외)
            if (!isDoT && parry != null && parry.TryParry(source)) return;
            currentHp = Mathf.Max(0f, currentHp - amount);
            OnDamaged(amount, source);
            if (currentHp <= 0f) OnHpZero(source);
        }

        /// 회복 배율(저주 장비: 회복 50%↓/회복 불가 등). 1 = 정상.
        public float HealMultiplier = 1f;

        public virtual void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount * Mathf.Max(0f, HealMultiplier));
        }

        public void ApplyStatus(StatusInstance status) => StatusCtrl.Apply(status);

        /// HP 직접 설정(부활/스크립트 전용. Heal과 달리 생존 가드 없음).
        protected void SetCurrentHp(float v) => currentHp = Mathf.Clamp(v, 0f, maxHp);

        protected virtual void OnDamaged(float amount, ICombatant source) { }
        protected abstract void OnHpZero(ICombatant source);

        /// 합체/분리 시스템이 디버프/보너스 주입할 때 사용.
        public float ExternalAtkMultiplier = 1f;
        public float ExternalSpeedMultiplier = 1f;
    }
}
