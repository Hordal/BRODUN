// CurrencyManager.cs — 재화 체계 (문서 05-7). 9종 재화 + 채집/먼지/결정.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Items
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        readonly Dictionary<Currency, int> _amounts = new Dictionary<Currency, int>();

        public int Get(Currency c) => _amounts.TryGetValue(c, out var v) ? v : 0;

        public void Add(Currency c, int amount)
        {
            _amounts[c] = Mathf.Max(0, Get(c) + amount);
            EventBus.Publish(new CurrencyChangedEvent { Type = c, NewAmount = _amounts[c] });
        }

        public bool TrySpend(Currency c, int amount)
        {
            if (Get(c) < amount) return false;
            Add(c, -amount);
            return true;
        }

        /// 사망 시 유실 규칙 (문서 06-7): 골드 20%, 제물 유지.
        public void ApplyDeathLoss()
        {
            Add(Currency.Gold, -Mathf.RoundToInt(Get(Currency.Gold) * 0.2f));
            // 제물/강화석/룬파편 등은 유지
        }
    }
}
