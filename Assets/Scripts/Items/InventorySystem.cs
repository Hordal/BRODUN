// InventorySystem.cs — 인벤토리/보관함/감정 (문서 02-7, 05-1)
// A 획득 미감정 아이템은 B가 있어야 식별 가능(문서 01-2, 02-7).
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;
using BroDungeon.Utilities;

namespace BroDungeon.Items
{
    public class InventoryEntry
    {
        public string ItemId;
        public bool Identified;     // 감정 여부
        public int Enhancement;     // +0~+5
        public AttributeType Enchant = AttributeType.None; // 룬 부여
        public bool Cursed;
    }

    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        readonly List<InventoryEntry> _bag = new List<InventoryEntry>();   // 런 인벤
        public IReadOnlyList<InventoryEntry> Bag => _bag;

        public void Add(string itemId, bool identified = false)
        {
            var def = ItemTable.Get(itemId);
            _bag.Add(new InventoryEntry
            {
                ItemId = itemId,
                Identified = identified || (def != null && def.Grade == Grade.Common), // 일반은 즉시
                Cursed = def != null && def.IsCursed
            });
            EventBus.Publish(new ItemPickedUpEvent { ItemId = itemId });
        }

        public void Remove(InventoryEntry entry) => _bag.Remove(entry);

        /// B 감정 (문서 02-7: 일반 즉시 / 희귀 1런 / 전설 2런).
        public bool Identify(InventoryEntry entry)
        {
            if (entry.Identified) return false;
            entry.Identified = true; // 실제 소요 런 수는 거점 연구실 큐에서 관리
            return true;
        }

        public int IdentifyCost(InventoryEntry entry)
        {
            var def = ItemTable.Get(entry.ItemId);
            if (def == null) return 0;
            switch (def.Grade) { case Grade.Rare: return 1; case Grade.Legendary: return 2; default: return 0; }
        }

        /// 사망 시 미장착 인벤 50% 유실(문서 06-7).
        public void ApplyDeathLoss()
        {
            for (int i = _bag.Count - 1; i >= 0; i--)
                if (Random.value < 0.5f) _bag.RemoveAt(i);
        }
    }
}
