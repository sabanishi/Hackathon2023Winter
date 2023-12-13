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

        private bool _isTransition;
        private MainGameData _mainGameData;
        private PunMainGameScreen _punMainGameScreen;
        private int _nextStageId=-1;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            _isTransition = false;
            if (screenData is MainGameData mainGameScreenData)
            {
                _mainGameData = mainGameScreenData;
                levelEntityManager.Setup(mainGameScreenData.IsOnline, mainGameScreenData.IsHost);
                //オンラインでないかホストの場合はステージを生成する
                if (!mainGameScreenData.IsOnline || mainGameScreenData.IsHost)
                {
                    levelEntityManager.CreateLevel(mainGameScreenData.IsOnline,mainGameScreenData.LevelId);
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
                levelEntityManager.OnGoToObservable.Subscribe(x => GoToStage(x).Forget()).AddTo(gameObject);
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

            return new MainGameData(isOnline: _mainGameData.IsOnline, isHost: _mainGameData.IsHost,levelId: _nextStageId);
        }

        private async UniTask GoToStage(int stageId)
        {
            if (_isTransition) return;
            _isTransition= true;
            _nextStageId = stageId;
            
            if (_mainGameData.IsOnline)
            {
                _punMainGameScreen.GoToStage();
            }
            else
            {
                ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
            }
        }

        private async UniTask GameClear(GameObject clearObject)
        {
            if (_isTransition) return;
            _isTransition= true;
            _nextStageId = -1;

            //TODO:クリア演出

            //ステージセレクト画面に遷移する
            if (_mainGameData.IsOnline)
            {
                _punMainGameScreen.GoToStage();
            }
            else
            {
                ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
            }
        }
    }
}