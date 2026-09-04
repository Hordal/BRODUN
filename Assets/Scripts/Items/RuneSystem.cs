// RuneSystem.cs — 룬 보유/조합/부여/정제 (문서 02-2). B 주도(룬 공방).
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;

namespace BroDungeon.Items
{
    public class RuneSystem : MonoBehaviour
    {
        public static RuneSystem Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        // 속성별 보유 룬 개수(일반/정제/순수는 tier 인덱스로)
        readonly Dictionary<AttributeType, int[]> _runes = new Dictionary<AttributeType, int[]>();
        readonly HashSet<string> _discoveredCombos = new HashSet<string>();

        public void AddRune(AttributeType attr, int tier = 0, int amount = 1)
        {
            if (!_runes.TryGetValue(attr, out var arr)) { arr = new int[3]; _runes[attr] = arr; }
            arr[Mathf.Clamp(tier, 0, 2)] += amount;
        }

        public int RuneCount(AttributeType attr, int tier = 0)
            => _runes.TryGetValue(attr, out var arr) ? arr[Mathf.Clamp(tier, 0, 2)] : 0;

        // ── 조합 (소모 안 됨, 영구 보유 — 문서 02-2) ──
        public RuneCombo TryCombo(AttributeType a, AttributeType b)
        {
            var combo = RuneTable.FindCombo(a, b);
            if (combo == null) return null;
            if (RuneCount(a) <= 0 || RuneCount(b) <= 0) return null;
            _discoveredCombos.Add(combo.ResultName);
            return combo;
        }
        public bool IsDiscovered(RuneCombo c) => c != null && _discoveredCombos.Contains(c.ResultName);

        // ── 부여 (룬 1회 소비, 덮어쓰기 가능 — 문서 02-2) ──
        public bool Enchant(InventoryEntry entry, AttributeType rune)
        {
            var def = ItemTable.Get(entry.ItemId);
            if (def == null) return false;
            bool weaponOrArmor = def.Slot == EquipSlot.A_Weapon || def.Slot == EquipSlot.A_Armor;
            if (!weaponOrArmor) return false; // A 무기/방어구만
            if (RuneCount(rune) <= 0) return false;
            AddRune(rune, 0, -1);
            entry.Enchant = rune; // 덮어쓰기
            return true;
        }

        // ── 정제 (문서 02-2: 일반×3→정제, 정제×3→순수) ──
        public bool Refine(AttributeType attr, int fromTier)
        {
            if (fromTier < 0 || fromTier > 1) return false;
            if (RuneCount(attr, fromTier) < 3) return false;
            AddRune(attr, fromTier, -3);
            AddRune(attr, fromTier + 1, 1);
            return true;
        }

        // ── 영속화 (문서 06-7: 룬은 storage에 런 무관 영구 보관) ──
        /// 보유 룬 개수를 storage.runes("속성:티어"→개수)로 직렬화.
        public void SaveTo(StorageData storage)
        {
            if (storage == null) return;
            storage.runes.Clear();
            foreach (var kv in _runes)
                for (int t = 0; t < kv.Value.Length; t++)
                    if (kv.Value[t] > 0) storage.runes[$"{kv.Key}:{t}"] = kv.Value[t];
        }

        public void LoadFrom(StorageData storage)
        {
            if (storage == null) return;
            _runes.Clear();
            foreach (var kv in storage.runes)
            {
                var parts = kv.Key.Split(':');
                if (parts.Length != 2) continue;
                if (!System.Enum.TryParse<AttributeType>(parts[0], out var attr)) continue;
                if (!int.TryParse(parts[1], out var tier)) continue;
                AddRune(attr, tier, kv.Value);
            }
        }

        /// 조합 발견 기록은 permanent.discoveredRunes에 보관.
        public void SaveDiscovered(PermanentData perm)
        {
            if (perm != null) perm.discoveredRunes = new List<string>(_discoveredCombos);
        }

        public void LoadDiscovered(PermanentData perm)
        {
            _discoveredCombos.Clear();
            if (perm?.discoveredRunes != null)
                foreach (var r in perm.discoveredRunes) _discoveredCombos.Add(r);
        }
    }
}
