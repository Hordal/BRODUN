// EventBus.cs — 옵저버 패턴 전역 이벤트 (문서 06-5 아키텍처)
using System;
using System.Collections.Generic;

namespace BroDungeon.Utilities
{
    /// <summary>타입 기반 전역 이벤트 버스. 매니저 간 느슨한 결합.</summary>
    public static class EventBus
    {
        static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var t = typeof(T);
            _handlers[t] = _handlers.TryGetValue(t, out var d)
                ? Delegate.Combine(d, handler) : handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var t = typeof(T);
            if (_handlers.TryGetValue(t, out var d))
            {
                var nd = Delegate.Remove(d, handler);
                if (nd == null) _handlers.Remove(t); else _handlers[t] = nd;
            }
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var d))
                ((Action<T>)d)?.Invoke(evt);
        }

        public static void Clear() => _handlers.Clear();
    }

    // ── 게임 이벤트 정의 (struct: GC 없는 발행) ──
    public struct DamageDealtEvent { public object Source, Target; public float Amount; public bool Crit; }
    public struct EnemyKilledEvent { public string EnemyId; public object Killer; }
    public struct BossDefeatedEvent { public string BossId; public int Zone; }
    public struct MergeStateChangedEvent { public Data.MergeState State; }
    public struct BondChangedEvent { public int Points; public int Tier; }
    public struct CharacterDownedEvent { public bool IsB; }
    public struct CharacterRevivedEvent { public bool IsB; }
    public struct GameOverEvent { }
    public struct FloorClearedEvent { public int Zone; public int Floor; }
    public struct ItemPickedUpEvent { public string ItemId; }
    public struct CurrencyChangedEvent { public Data.Currency Type; public int NewAmount; }
}
