namespace Hackathon2023Winter.Entity
{
    public interface ISwitchTarget
    {
        public void Enter();
        public void Exit();
        public void PassSwitchReference(SwitchEntity switchEntity);
    }
}