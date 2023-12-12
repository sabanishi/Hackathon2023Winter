namespace Hackathon2023Winter.Screen
{
    /// <summary>
    /// StageSelectScreenに渡すデータ
    /// </summary>
    public class StageSelectData : IScreenData
    {
        public bool IsOnline { get; }
        public bool IsHost { get; }

        public StageSelectData(bool isOnline, bool isHost)
        {
            IsOnline = isOnline;
            IsHost = isHost;
        }
    }
}