// ICombatant.cs — 피해를 주고받는 모든 객체(캐릭터/적/보스) 공통 계약
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Combat
{
    public interface ICombatant
    {
        Transform Transform { get; }
        bool IsAlive { get; }
        float CurrentHp { get; }
        float MaxHp { get; }
        float Defense { get; }
        StatusController Status { get; }

        /// 피해 적용. attribute로 상성/상태이상 부여 판단. isDoT는 DoT 재귀 방지.
        void TakeDamage(float amount, AttributeType attribute, ICombatant source, bool isDoT = false);
        void Heal(float amount);
        void ApplyStatus(StatusInstance status);
    }
}
