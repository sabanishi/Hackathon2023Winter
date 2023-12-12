using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Level;
using Hackathon2023Winter.Screen;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameScreen : BaseScreen
    {
        [SerializeField] private LevelEntityManager levelEntityManager;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            if (screenData is MainGameData mainGameScreenData)
            {
                //オンラインでないかホストの場合はステージを生成する
                if (!mainGameScreenData.IsOnline || mainGameScreenData.IsHost)
                {
                    levelEntityManager.Setup();
                    levelEntityManager.CreateLevel(mainGameScreenData.IsOnline);
                }
            }
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            levelEntityManager.Cleanup();
            return null;
        }
    }
}