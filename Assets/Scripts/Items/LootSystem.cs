// LootSystem.cs — 드롭 테이블/루팅 (문서 02-6 등급, 05-7 재화). 호스트 권한.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;

namespace BroDungeon.Items
{
    public class LootSystem : MonoBehaviour
    {
        public static LootSystem Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        [Header("드롭 확률 (행운/성장 보정 전 기본)")]
        public float itemDropChance = 0.25f;
        public float runeDropChance = 0.15f;
        public float dropRateBonus;       // 행운 속성/유물 (음수 가능: 저주 드롭-50%)
        public float gainMultiplier = 1f; // 골드/제물 획득 배율 (저주 탐욕의 반지 3배)

        /// 몬스터 처치 시 드롭 생성. 호스트만 호출(문서 01-4).
        public void RollDrops(MonsterDef enemy, Vector3 pos)
        {
            float luck = Mathf.Max(0f, 1f + dropRateBonus);
            // 골드/제물 (획득 배율 반영)
            int gold = Mathf.RoundToInt(Random.Range(5, 20) * gainMultiplier);
            CurrencyManager.Instance?.Add(Currency.Gold, gold);
            CurrencyManager.Instance?.Add(Currency.Sacrifice, Mathf.RoundToInt(Random.Range(3, 12) * gainMultiplier));

            // 아이템 드롭
            if (Random.value < itemDropChance * luck)
            {
                var grade = RollGrade(enemy.Type == EnemyType.Elite);
                var item = PickItem(grade);
                if (item != null) InventorySystem.Instance?.Add(item.Id, identified: false);
            }
            // 룬 드롭
            if (Random.value < runeDropChance * luck && enemy.Attr1 != AttributeType.None)
                RuneSystem.Instance?.AddRune(enemy.Attr1);
        }

        Grade RollGrade(bool elite)
        {
            float r = Random.value;
            if (elite) { if (r < 0.15f) return Grade.Legendary; if (r < 0.55f) return Grade.Rare; return Grade.Common; }
            if (r < 0.03f) return Grade.Legendary;
            if (r < 0.25f) return Grade.Rare;
            return Grade.Common;
        }

        ItemDef PickItem(Grade grade)
        {
            var pool = new List<ItemDef>();
            foreach (var v in ItemTable.All.Values)
                if (v.Grade == grade && !v.IsCursed) pool.Add(v);
            return pool.Count == 0 ? null : pool[Random.Range(0, pool.Count)];
        }

        /// 보스 처치 보상(문서 04-2).
        public void GrantBossReward(BossDef boss)
        {
            if (boss == null) return;
            if (boss.Attrs != null)
                foreach (var a in boss.Attrs) if (a != AttributeType.None) RuneSystem.Instance?.AddRune(a);
            CurrencyManager.Instance?.Add(Currency.BossMaterial, 1);
        }
    }
}
