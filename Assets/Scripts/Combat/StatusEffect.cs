// StatusEffect.cs — 상태이상/DoT/버프 관리 (속성 효과 파생)
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Combat
{
    /// <summary>단일 상태이상 인스턴스.</summary>
    public class StatusInstance
    {
        public StatusType Type;
        public float Duration;     // 남은 시간
        public float TickInterval; // DoT 주기(0=비DoT)
        public float TickDamage;   // 틱당 피해
        public float Magnitude;    // 둔화%/방어감소% 등
        public int Stacks = 1;
        float _tickTimer;

        public bool TickDamageDue(float dt)
        {
            Duration -= dt;
            if (TickInterval <= 0f) return false;
            _tickTimer += dt;
            if (_tickTimer >= TickInterval) { _tickTimer -= TickInterval; return true; }
            return false;
        }
        public bool Expired => Duration <= 0f;
    }

    /// <summary>한 대상의 상태이상 컨테이너. ICombatant가 보유.</summary>
    public class StatusController
    {
        readonly List<StatusInstance> _active = new List<StatusInstance>();
        readonly ICombatant _owner;

        public StatusController(ICombatant owner) { _owner = owner; }

        public bool Has(StatusType t) => _active.Exists(s => s.Type == t);
        public bool CanAct => !Has(StatusType.Freeze) && !Has(StatusType.Stun) && !Has(StatusType.Fear);
        public bool Silenced => Has(StatusType.Silence);
        public bool Invulnerable => Has(StatusType.Invuln);

        public float SpeedMultiplier()
        {
            float m = 1f;
            foreach (var s in _active)
            {
                if (s.Type == StatusType.Slow) m *= (1f - s.Magnitude);
                if (s.Type == StatusType.Haste) m *= (1f + s.Magnitude);
            }
            return m;
        }

        public float DefenseModifier()
        {
            float m = 0f;
            foreach (var s in _active) if (s.Type == StatusType.Curse) m += s.Magnitude;
            return m; // 방어 감소 비율
        }

        public void Apply(StatusInstance inst)
        {
            var existing = _active.Find(s => s.Type == inst.Type);
            if (existing != null)
            {
                existing.Duration = UnityEngine.Mathf.Max(existing.Duration, inst.Duration);
                existing.Stacks = UnityEngine.Mathf.Min(existing.Stacks + 1, Constants.STATUS_MAX_STACKS);
                existing.Magnitude = UnityEngine.Mathf.Max(existing.Magnitude, inst.Magnitude);
            }
            else _active.Add(inst);
        }

        public void Cleanse() => _active.Clear();
        public void CleanseDebuffs() => _active.RemoveAll(IsDebuff);

        public void Tick(float dt)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var s = _active[i];
                if (s.TickDamageDue(dt))
                    _owner.TakeDamage(s.TickDamage * s.Stacks, AttributeType.None, null, isDoT: true);
                if (s.Expired) _active.RemoveAt(i);
            }
        }

        static bool IsDebuff(StatusInstance s) =>
            s.Type != StatusType.Shield && s.Type != StatusType.Haste && s.Type != StatusType.Invuln;
    }
}
