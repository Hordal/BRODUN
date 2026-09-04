// Timer.cs — 경량 카운트다운/쿨다운 유틸 (다운 타이머, 스킬 쿨 등)
using System;

namespace BroDungeon.Utilities
{
    /// <summary>매 프레임 Tick(dt) 호출되는 카운트다운 타이머.</summary>
    public class Timer
    {
        public float Duration { get; private set; }
        public float Remaining { get; private set; }
        public bool Running { get; private set; }
        public bool Finished => Running == false && Remaining <= 0f;
        public float Progress => Duration <= 0f ? 1f : 1f - Remaining / Duration;

        Action _onComplete;

        public void Start(float duration, Action onComplete = null)
        {
            Duration = duration; Remaining = duration; Running = true; _onComplete = onComplete;
        }

        public void Stop() { Running = false; Remaining = 0f; }

        public void Tick(float dt)
        {
            if (!Running) return;
            Remaining -= dt;
            if (Remaining <= 0f)
            {
                Remaining = 0f; Running = false;
                _onComplete?.Invoke();
            }
        }
    }

    /// <summary>쿨다운 게이트 (스킬/가젯).</summary>
    public class Cooldown
    {
        public float Total { get; private set; }
        public float Remaining { get; private set; }
        public bool Ready => Remaining <= 0f;
        public float Ratio => Total <= 0f ? 0f : Remaining / Total;

        public void Trigger(float seconds) { Total = seconds; Remaining = seconds; }
        public void Reset() => Remaining = 0f;
        public void Reduce(float pct) => Remaining *= (1f - pct);
        public void Tick(float dt) { if (Remaining > 0f) Remaining -= dt; }
    }
}
