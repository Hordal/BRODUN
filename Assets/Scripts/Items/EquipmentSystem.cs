// EquipmentSystem.cs — 장착 슬롯 관리 + 스탯/시너지 적용 (문서 02-3,7 / I-05)
// 장비 효과 문자열 → ItemEffectParser → StatSheet 집계 → 캐릭터에 반영.
using System;
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Database;
using BroDungeon.Combat;

namespace BroDungeon.Items
{
    public class EquipmentSystem : MonoBehaviour
    {
        public Characters.WarriorController warrior;
        public Characters.MageController mage;
        public Combat.SkillSystem warriorSkills;
        public Combat.SkillSystem mageSkills;
        public CursedItemHandler cursedHandler; // 저주 장비 디메리트 처리

        readonly Dictionary<EquipSlot, InventoryEntry> _equipped = new Dictionary<EquipSlot, InventoryEntry>();

        // 장비 외 스탯 소스(제단 A풀/C풀, 유물 등). 재계산 시 함께 합산.
        readonly List<StatModifier> _extraA = new List<StatModifier>();
        readonly List<StatModifier> _extraB = new List<StatModifier>();

        // 동적 공급자(저주 핸들러 등): 호출 시점의 활성 모디파이어를 반환. 제거 가능.
        readonly List<Func<CharacterType, IEnumerable<StatModifier>>> _providers
            = new List<Func<CharacterType, IEnumerable<StatModifier>>>();

        public void RegisterProvider(Func<CharacterType, IEnumerable<StatModifier>> provider)
        {
            if (provider != null && !_providers.Contains(provider)) { _providers.Add(provider); Recalculate(); }
        }
        public void UnregisterProvider(Func<CharacterType, IEnumerable<StatModifier>> provider)
        {
            if (_providers.Remove(provider)) Recalculate();
        }

        /// 제단/유물 등이 영구·런 스탯을 주입하는 단일 진입점(누적형).
        public void AddExternalModifier(CharacterType who, StatModifier m)
        {
            (who == CharacterType.A_Warrior ? _extraA : _extraB).Add(m);
            Recalculate();
        }
        public void ClearExternal(CharacterType who)
        {
            (who == CharacterType.A_Warrior ? _extraA : _extraB).Clear();
            Recalculate();
        }

        public bool Equip(InventoryEntry entry)
        {
            var def = ItemTable.Get(entry.ItemId);
            if (def == null || !entry.Identified) return false; // 미감정 장착 불가
            // 같은 슬롯에 저주 장비가 이미 있으면 런 중 교체 불가(문서 02-7).
            // 그냥 덮어쓰면 CursedItemHandler.OnUnequipped가 호출되지 않아
            // 디메리트/공급자/루팅 배율이 영구히 남는다.
            if (_equipped.TryGetValue(def.Slot, out var prev) && prev.Cursed) return false;
            _equipped[def.Slot] = entry;
            if (def.IsCursed) cursedHandler?.OnEquipped(def.Id);
            Recalculate();
            return true;
        }

        public void Unequip(EquipSlot slot)
        {
            // 저주 장비는 런 중 해제 불가(거점 복귀 시만, 문서 02-7)
            if (_equipped.TryGetValue(slot, out var e) && e.Cursed) return;
            _equipped.Remove(slot);
            Recalculate();
        }

        /// 거점 복귀 시에만 저주 장비 강제 해제 가능(문서 02-7).
        public void ForceUnequipCursed(EquipSlot slot)
        {
            if (_equipped.TryGetValue(slot, out var e) && e.Cursed)
            {
                var def = ItemTable.Get(e.ItemId);
                if (def != null) cursedHandler?.OnUnequipped(def.Id);
                _equipped.Remove(slot);
                Recalculate();
            }
        }

        public InventoryEntry Get(EquipSlot slot) => _equipped.TryGetValue(slot, out var e) ? e : null;

        /// 장비/룬/스킬 변경 시 스탯·시너지 전체 재계산.
        public void Recalculate()
        {
            ApplyFor(CharacterType.A_Warrior, warrior, warriorSkills);
            ApplyFor(CharacterType.B_Mage, mage, mageSkills);
        }

        void ApplyFor(CharacterType who, Characters.CharacterBase character, Combat.SkillSystem skills)
        {
            if (character == null) return;

            character.Stats.Clear();
            var itemAttrs = new List<AttributeType>();
            var runeAttrs = new List<AttributeType>();

            foreach (var kv in _equipped)
            {
                if (!BelongsTo(kv.Key, who)) continue;
                var def = ItemTable.Get(kv.Value.ItemId);
                if (def == null) continue;

                // 효과 문자열 → 스탯 모디파이어 (이로운 효과만 파싱)
                character.Stats.AddRange(ItemEffectParser.Parse(def.BaseEffect));
                character.Stats.AddRange(ItemEffectParser.Parse(def.Special));
                // 저주 디메리트("체력 1","마나 2배 소비" 등)는 일반 파서로 오역되므로 제외.
                // → 전용 CursedItemHandler에서 별도 처리(TODO).

                // 강화 보너스(+1당 공격 고정 +5)
                if (kv.Value.Enhancement > 0)
                    character.Stats.Add(StatType.AttackFlat, kv.Value.Enhancement * 5f);

                if (def.Attr1 != AttributeType.None) itemAttrs.Add(def.Attr1);
                if (def.Attr2 != AttributeType.None) itemAttrs.Add(def.Attr2);
                if (kv.Value.Enchant != AttributeType.None) runeAttrs.Add(kv.Value.Enchant);
            }

            // 장착 스킬 속성(시너지 합산)
            var skillAttrs = new List<AttributeType>();
            if (skills != null)
                foreach (var id in skills.ActiveSkills)
                {
                    var sd = SkillTable.Get(id);
                    if (sd != null && sd.Attribute != AttributeType.None) skillAttrs.Add(sd.Attribute);
                }

            // 장비 외 스탯(제단/유물) 합산
            character.Stats.AddRange(who == CharacterType.A_Warrior ? _extraA : _extraB);

            // 동적 공급자(저주 핸들러 등) 합산
            foreach (var provider in _providers)
            {
                var mods = provider(who);
                if (mods != null) character.Stats.AddRange(mods);
            }

            // 시너지(I-05/I-07)
            var synergy = SynergyCalculator.Evaluate(itemAttrs, skillAttrs, runeAttrs);
            character.AttributeSynergyBonus = SynergyCalculator.TotalAttributeBonus(synergy);

            // 최대 HP/마나 등 파생 재계산
            character.RecomputeDerived();

            // A 무기 속성을 근접 공격에 반영
            if (who == CharacterType.A_Warrior && warrior != null)
            {
                var wpn = Get(EquipSlot.A_Weapon);
                var wdef = wpn != null ? ItemTable.Get(wpn.ItemId) : null;
                warrior.WeaponAttribute = wpn != null && wpn.Enchant != AttributeType.None
                    ? wpn.Enchant
                    : (wdef != null ? wdef.Attr1 : AttributeType.None);
            }
        }

        static bool BelongsTo(EquipSlot s, CharacterType who) =>
            who == CharacterType.A_Warrior
                ? (s == EquipSlot.A_Weapon || s == EquipSlot.A_Armor || s == EquipSlot.A_Accessory)
                : (s == EquipSlot.B_Weapon || s == EquipSlot.B_Book || s == EquipSlot.B_Accessory);
    }
}
