// StateMachine.cs — 몬스터/보스 FSM (문서 06-5 아키텍처)
namespace BroDungeon.AI
{
    public interface IState
    {
        void Enter();
        void Tick(float dt);
        void Exit();
    }

    public class StateMachine
    {
        public IState Current { get; private set; }

        public void Change(IState next)
        {
            if (Current == next) return;
            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick(float dt) => Current?.Tick(dt);
    }
}
