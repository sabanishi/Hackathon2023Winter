using UniRx;

namespace Hackathon2023Winter.Entity
{
    public interface IEventGenerator
    {
        public IReadOnlyReactiveProperty<bool> Trigger { get; }
        public void SendFireEvent();
    }
}