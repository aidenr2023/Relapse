namespace _Scripts.Util.Interfaces
{
    public interface IGameReset
    {
        public EventVariable OnGameReset { get; }
        
        public void GameResetAction();
    }
}