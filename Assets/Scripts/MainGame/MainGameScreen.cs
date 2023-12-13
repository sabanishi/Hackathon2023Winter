using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Level;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameScreen : BaseScreen
    {
        [SerializeField] private LevelEntityManager levelEntityManager;
        [SerializeField] private PunMainGameScreen punMainGameScreenPrefab;

        private bool _isClear;
        private MainGameData _mainGameData;
        private PunMainGameScreen _punMainGameScreen;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            _isClear = false;
            if (screenData is MainGameData mainGameScreenData)
            {
                _mainGameData = mainGameScreenData;
                levelEntityManager.Setup(mainGameScreenData.IsOnline, mainGameScreenData.IsHost);
                //オンラインでないかホストの場合はステージを生成する
                if (!mainGameScreenData.IsOnline || mainGameScreenData.IsHost)
                {
                    levelEntityManager.CreateLevel(mainGameScreenData.IsOnline);
                }

                //オンラインである時はPunMainGameScreenを生成する
                if (mainGameScreenData.IsOnline)
                {
                    _punMainGameScreen = PhotonNetwork
                        .Instantiate(punMainGameScreenPrefab.name, Vector3.zero, Quaternion.identity)
                        .GetComponent<PunMainGameScreen>();
                    _punMainGameScreen.transform.parent = transform;
                }

                //TODO:ゴールに触れた時の処理
                levelEntityManager.OnClearObservable.Subscribe(x => GameClear(x).Forget()).AddTo(gameObject);
            }
            else
            {
                Debug.LogError("MainGameScreenに渡されたデータが不正です");
            }
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            levelEntityManager.Cleanup();
            if (_punMainGameScreen != null)
            {
                PhotonNetwork.Destroy(_punMainGameScreen.gameObject);
            }

            return new StageSelectData(isOnline: _mainGameData.IsOnline, isHost: _mainGameData.IsHost);
        }

        private async UniTask GameClear(GameObject clearObject)
        {
            if (_isClear) return;
            _isClear = true;

            //TODO:クリア演出

            //ステージセレクト画面に遷移する
            if (_mainGameData.IsOnline)
            {
                _punMainGameScreen.GoToStageSelectScreen();
            }
            else
            {
                ScreenTransition.Instance.Move(ScreenType.StageSelect).Forget();
            }
        }
    }
}