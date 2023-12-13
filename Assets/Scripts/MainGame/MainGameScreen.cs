using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Level;
using Hackathon2023Winter.Screen;
using UniRx;
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
                levelEntityManager.Setup(mainGameScreenData.IsOnline,mainGameScreenData.IsHost);
                //オンラインでないかホストの場合はステージを生成する
                if (!mainGameScreenData.IsOnline || mainGameScreenData.IsHost)
                {
                    levelEntityManager.CreateLevel(mainGameScreenData.IsOnline);
                }
                
                //TODO:ゴールに触れた時の処理
                levelEntityManager.OnClearObservable.Subscribe(x=>Debug.Log(x.name)).AddTo(gameObject);
            }
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            levelEntityManager.Cleanup();
            return null;
        }
    }
}