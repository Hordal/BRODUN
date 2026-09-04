// ComboSystem.cs — A 근접 콤보 카운터 (커맨드 패턴 기반, 문서 06-5)
namespace BroDungeon.Combat
{
    /// <summary>일정 시간 내 연속 입력으로 콤보 스텝 증가. 미입력 시 리셋.</summary>
    public class ComboSystem
    {
        readonly float _resetTime;
        float _timer;
        public int Step { get; private set; }
        public int MaxStep = 3;

        public ComboSystem(float resetTime) { _resetTime = resetTime; }

        public int Advance()
        {
            Step = Step >= MaxStep ? 1 : Step + 1;
            _timer = _resetTime;
            return Step;
        }

        public void Tick(float dt)
        {
            if (Step == 0) return;
            _timer -= dt;
            if (_timer <= 0f) Step = 0;
        }

        public void Reset() { Step = 0; _timer = 0f; }
    }
}
