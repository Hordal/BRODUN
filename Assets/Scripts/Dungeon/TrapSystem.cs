// TrapSystem.cs — 함정 런타임 (문서 03-3). 합체/분리 상태에 따라 작동 분기.
// 주기적으로 반경 내 플레이어(ICombatant)에 피해. 무게 함정은 합체 시 피해 배율↑.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Characters;
using BroDungeon.Combat;

namespace BroDungeon.Dungeon
{
    public class TrapSystem : MonoBehaviour
    {
        public string trapId = "T01";
        public MergeSystem merge;

        [Header("작동")]
        public float damage = 20f;
        public float cycleTime = 2f;
        public float hitRadius = 1.2f;
        public LayerMask targetMask;            // 플레이어 레이어
        public AttributeType attribute = AttributeType.None; // 속성 함정(독/번개 등)
        public bool oneShot;                    // true면 1회만 작동(유인 보물함 등)
        [Tooltip("합체(무게 초과) 시 피해 배율 — 낙하바닥·천장프레스 등 무게 함정")]
        public float mergedDamageMul = 1f;

        TrapDef _def;
        float _timer;
        bool _spent;
        protected bool Armed;

        protected virtual void Awake()
        {
            TrapTable.All.TryGetValue(trapId, out _def); // 미등록 id여도 NRE/예외 없이 동작
        }

        protected virtual void Update()
        {
            if (_spent) return;
            _timer += Time.deltaTime;
            if (_timer >= cycleTime) { _timer = 0f; Fire(); }
        }

        /// 함정 작동: 반경 내 생존 대상에 피해(합체/분리 상태에 따라 분기).
        protected virtual void Fire()
        {
            Armed = true;
            bool merged = merge != null && merge.IsMerged;
            float dmg = damage * (merged ? mergedDamageMul : 1f);

            var hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, targetMask);
            var done = new HashSet<ICombatant>();
            foreach (var h in hits)
            {
                var c = h.GetComponentInParent<ICombatant>();
                if (c == null || !c.IsAlive || !done.Add(c)) continue; // 중복 타격 방지
                c.TakeDamage(dmg, attribute, null);
            }
            if (oneShot) _spent = true;
        }

        /// 함정 재무장(방 재진입/리셋 시).
        public void Rearm() { _spent = false; _timer = 0f; }

        public TrapDef Def => _def;
    }
}
