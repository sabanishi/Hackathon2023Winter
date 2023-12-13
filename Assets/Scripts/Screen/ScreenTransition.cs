using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.MainGame;
using Sabanishin.Common;
using UnityEngine;

namespace Hackathon2023Winter.Screen
{
    /// <summary>
    /// Screenの遷移を管理するクラス
    /// </summary>
    public class ScreenTransition : SingletonMonoBehaviour<ScreenTransition>
    {
        [SerializeField] private ScreenType loadScreenType;

        private bool _isTransitioning;

        private CancellationTokenSource _cancellationTokenSource;

        /**ScreenTypeとResourcesパスを対応させるDict*/
        private readonly Dictionary<ScreenType, string> _screenPathDictionary = new()
        {
            { ScreenType.Title, "Screen/TitleScreen" },
            { ScreenType.Matching, "Screen/MatchingScreen" },
            { ScreenType.MainGame, "Screen/MainGameScreen" },
            { ScreenType.WaitMember ,"Screen/WaitMemberScreen"}
        };

        private BaseScreen _nowScreen;

        private void Start()
        {
            Move(loadScreenType).Forget();
        }

        /// <summary>
        /// 次の画面に遷移する
        /// </summary>
        /// <param name="nextScreenType">読み込むシーンを判別するための列挙型</param>
        /// <param name="isForce">遷移処理の最中でも強制的に次の画面に遷移する</param>
        public async UniTask Move(ScreenType nextScreenType, bool isForce = false)
        {
            if (_isTransitioning && !isForce) return;
            _isTransitioning = true;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();
            var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token,
                this.GetCancellationTokenOnDestroy()).Token;
            IScreenData screenData = null;

            //現在の画面がある場合は破棄する
            if (_nowScreen != null)
            {
                await _nowScreen.CloseDeal(token);
                await _nowScreen.CloseAnimation(token);
                screenData = await _nowScreen.Dispose(token);
                Destroy(_nowScreen.gameObject);
            }

            //次の画面を生成する
            BaseScreen nextScreen = null;
            var isGetScreenPrefab = TryGetScreenPrefab(nextScreenType, out var screenPrefab);
            if (isGetScreenPrefab)
            {
                var nextScreenObject = Instantiate(screenPrefab);
                if (!nextScreenObject.TryGetComponent(out nextScreen))
                {
                    Debug.LogError($"ScreenPrefab(ScreenType: {nextScreenType})にBaseScreenを継承したコンポーネントがアタッチされていません");
                }
            }

            //次の画面のセットアップ
            if (nextScreen != null)
            {
                _nowScreen = nextScreen;
                var nextScreenDestroyToken = nextScreen.gameObject.GetCancellationTokenOnDestroy();
                var nextScreenCts = CancellationTokenSource.CreateLinkedTokenSource(token, nextScreenDestroyToken);
                await nextScreen.Initialize(screenData, nextScreenCts.Token);
                await nextScreen.OpenAnimation(nextScreenCts.Token);
                await nextScreen.OpenDeal(nextScreenCts.Token);
            }

            _isTransitioning = false;
        }

        /// <summary>
        /// MainGameScreenからMainGameScreenに遷移する
        /// </summary>
        public async UniTask MoveFromGameToGame()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();
            var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token,
                this.GetCancellationTokenOnDestroy()).Token;
            IScreenData screenData = null;

            //現在の画面の終了処理
            var beforeScreen = _nowScreen;
            if (_nowScreen != null)
            {
                await _nowScreen.CloseDeal(token);
                screenData = await _nowScreen.Dispose(token);
            }

            //次の画面を生成する
            BaseScreen nextScreen = null;
            var nextScreenType = ScreenType.MainGame;
            var isGetScreenPrefab = TryGetScreenPrefab(nextScreenType, out var screenPrefab);
            if (isGetScreenPrefab)
            {
                var nextScreenObject = Instantiate(screenPrefab);
                if (!nextScreenObject.TryGetComponent(out nextScreen))
                {
                    Debug.LogError($"ScreenPrefab(ScreenType: {nextScreenType})にBaseScreenを継承したコンポーネントがアタッチされていません");
                }
            }

            //次の画面のセットアップ
            if (nextScreen != null)
            {
                _nowScreen = nextScreen;
                var nextScreenDestroyToken = nextScreen.gameObject.GetCancellationTokenOnDestroy();
                var nextScreenCts = CancellationTokenSource.CreateLinkedTokenSource(token, nextScreenDestroyToken);
                await nextScreen.Initialize(screenData, nextScreenCts.Token);
                //遷移アニメーションの開始
                if(beforeScreen is MainGameScreen mainGameScreen && nextScreen is MainGameScreen nextMainGameScreen)
                {
                    await ShowMainGameTransitionAnimation(mainGameScreen.GetCamera(),nextMainGameScreen.GetCamera());
                }
                
                await nextScreen.OpenDeal(nextScreenCts.Token);
            }
            
            //前の画面を破棄する
            if (beforeScreen != null)
            {
                Destroy(beforeScreen.gameObject);
            }

            _isTransitioning = false;
        }

        private async UniTask ShowMainGameTransitionAnimation(Camera beforeCamera, Camera afterCamera)
        {
            Debug.Log("MainGameTransitionAnimation");
            //3秒待機
            await UniTask.Delay(3000);
        }

        /// <summary>
        /// Screenプレハブの取得を試みる
        /// </summary>
        private bool TryGetScreenPrefab(ScreenType screenType, out GameObject screenPrefab)
        {
            screenPrefab = null;
            if (!_screenPathDictionary.TryGetValue(screenType, out var path))
            {
                Debug.LogError($"ScreenType:{screenType} に対応するパスがありません");
                return false;
            }

            screenPrefab = Resources.Load<GameObject>(path);
            if (screenPrefab == null)
            {
                Debug.LogError($"ScreenPrefab(パス: {path}) のロードに失敗しました");
            }

            return true;
        }
    }
}