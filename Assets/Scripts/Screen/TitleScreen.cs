using System.Threading;
using Cysharp.Threading.Tasks;
using Sabanishi.Common;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Screen
{
    public class TitleScreen:BaseScreen
    {
        [SerializeField]private Button offlineButton;
        [SerializeField]private Button onlineButton;

        private bool _isOnline;
        private bool _isTransitioning;

        protected override UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            _isTransitioning = false;
            offlineButton.SafeOnClickAsObservable().Subscribe(_=>GoToNextScreen(false)).AddTo(gameObject);
            onlineButton.SafeOnClickAsObservable().Subscribe(_=>GoToNextScreen(true)).AddTo(gameObject);
            
            return base.InitializeInternal(screenData, token);
        }

        private void GoToNextScreen(bool isOnline)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            _isOnline = isOnline;
            var screenType = isOnline ?  ScreenType.Matching : ScreenType.StageSelect;
            ScreenTransition.Instance.Move(screenType).Forget();
        }
        
        protected override UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            return UniTask.FromResult<IScreenData>(null);
        }
        
        protected override async UniTask CloseAnimationInternal(CancellationToken token)
        {
            if (_isOnline)
            {
                await NowLoadingPanel.Instance.CloseAnimation(0.5f, token);
            }
            else
            {
                await TmpScreenAnimation.Instance.CloseAnimation(0.5f, token);
            }
        }
    }
}