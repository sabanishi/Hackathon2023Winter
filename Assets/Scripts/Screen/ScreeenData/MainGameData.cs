namespace Hackathon2023Winter.Screen
{
    /// <summary>
    /// MainGameScreenに渡すデータ
    /// </summary>
    public class MainGameData : IScreenData
    {
        public bool IsOnline { get; }
        public bool IsHost { get; }

        public int LevelId { get; }

        public MainGameData(bool isOnline, bool isHost, int levelId)
        {
            IsOnline = isOnline;
            IsHost = isHost;
            LevelId = levelId;
        }
    }
}