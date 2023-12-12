namespace Hackathon2023Winter.Screen
{
    public class MainGameData:IScreenData
    {
        public bool IsOnline { get; }
        public int StageId { get; }

        public MainGameData(bool isOnline,int stageId)
        {
            IsOnline = isOnline;
            StageId = stageId;
        }
    }
}