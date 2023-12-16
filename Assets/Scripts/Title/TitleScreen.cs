using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Sabanishi.Common;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Title
{
    public class TitleScreen : BaseScreen
    {
        [SerializeField]private Button clickSoloButton;
        [SerializeField]private Button clickDuoButton;
        [SerializeField] private Button clickExit;
        [SerializeField] private InteractiveShape interactiveShape;

        private bool _isOnline;
        private bool _isTransitioning;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            SoundManager.PlayBGM(BGM_Enum.LOADING);
            _isTransitioning = false;
            clickSoloButton.SafeOnClickAsObservable().Subscribe(_ => GoToNextScreen(false)).AddTo(gameObject);
            clickDuoButton.SafeOnClickAsObservable().Subscribe(_ => GoToNextScreen(true)).AddTo(gameObject);
            clickExit.SafeOnClickAsObservable().Subscribe(_ => Quit()).AddTo(gameObject);
            await interactiveShape.Initialize();
            SoundManager.PlayBGM(BGM_Enum.TITLE);
        }

        private void GoToNextScreen(bool isOnline)
        {
            if (_isTransitioning) return;
            SoundManager.PlaySE(SE_Enum.CLICK);
            _isTransitioning = true;
            _isOnline = isOnline;
            var screenType = isOnline ? ScreenType.Matching : ScreenType.MainGame;
            ScreenTransition.Instance.Move(screenType).Forget();
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            return _isOnline
                ? null
                : new MainGameData(false, false,-1);
        }

        protected override async UniTask CloseAnimationInternal(CancellationToken token)
        {
            if (_isOnline)
            {
                await NowLoadingAnimation.Instance.CloseAnimation(0.5f, token);
            }
            else
            {
                await BlackoutScreenAnimation.Instance.CloseAnimation(0.5f, token);
            }
        }
        
        protected override async UniTask OpenAnimationInternal(CancellationToken token)
        {
            await NowLoadingAnimation.Instance.OpenAnimation(0.2f, token);
        }
    }
}