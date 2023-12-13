using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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