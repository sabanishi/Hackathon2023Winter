using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameScreen:BaseScreen
    {
        [SerializeField]private MainGameManager mainGameManager;
        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            mainGameManager.Setup();
        }
    }
}