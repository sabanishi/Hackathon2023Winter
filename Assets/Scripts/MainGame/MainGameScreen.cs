using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Entity;
using Hackathon2023Winter.Level;
using Hackathon2023Winter.Matching;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameScreen : BaseScreen
    {
        [SerializeField] private Camera myCamera;
        [SerializeField] private LevelEntityManager levelEntityManager;
        [SerializeField] private MainGameCommandManager commandManager;
        [SerializeField] private PunMainGameScreen punMainGameScreenPrefab;
        [SerializeField]private PunMainGameScreenReceiver punMainGameScreenReceiverPrefab;

        private bool _isTransition;
        private MainGameData _mainGameData;
        private PunMainGameScreen _punMainGameScreen;
        private PunMainGameScreenReceiver _punMainGameScreenReceiver;
        private int _nextStageId=-1;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            _isTransition = false;
            if (screenData is MainGameData mainGameScreenData)
            {
                var isOnline = mainGameScreenData.IsOnline;
                var isHost = mainGameScreenData.IsHost;
                var levelId = mainGameScreenData.LevelId;
                var isHostCreateLevel = false;
                
                commandManager.Setup(isOnline);
                commandManager.OnCommandObservable.Subscribe(TakeCommand).AddTo(gameObject);
                
                //Photonの通信を行う機構を作る
                if (isOnline)
                {
                    if (isHost)
                    {
                        _punMainGameScreen = PhotonNetwork
                            .Instantiate(punMainGameScreenPrefab.name, Vector3.zero, Quaternion.identity)
                            .GetComponent<PunMainGameScreen>();
                        _punMainGameScreen.transform.parent = transform;
                    }
                    else
                    {
                        _punMainGameScreenReceiver = PhotonNetwork
                            .Instantiate(punMainGameScreenReceiverPrefab.name, Vector3.zero, Quaternion.identity)
                            .GetComponent<PunMainGameScreenReceiver>();
                        _punMainGameScreenReceiver.transform.parent = transform;
                        _punMainGameScreenReceiver.OnGoToObservable.Subscribe(x =>
                        {
                            _nextStageId = x;
                            ScreenTransition.Instance.MoveFromGameToGame().Forget();
                        }).AddTo(gameObject);
                        _punMainGameScreenReceiver.OnCreateLevelObservable.Subscribe(_=>isHostCreateLevel=true).AddTo(gameObject);
                    }
                }
                
                //ステージIDに応じてx座標をずらす
                transform.position = new Vector3(levelId*1000, 0, 0);
                
                _mainGameData = mainGameScreenData;
                levelEntityManager.Setup(isOnline, isHost);
                //オンラインでないかホストの場合はステージを生成する
                if (!isOnline ||isHost)
                {
                    levelEntityManager.CreateLevel(isOnline,levelId);
                    _punMainGameScreen?.SendCreateLevel();
                }
                else
                {
                    //ホストがステージを生成するのを待つ
                    /*await UniTask.WhenAny(
                        UniTask.WaitUntil(() => isHostCreateLevel,cancellationToken:token),
                        UniTask.Delay(100, cancellationToken: token)
                        );*/
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

        protected override async UniTask OpenDealInternal(CancellationToken token)
        {
            //ユーザーの入力受付を開始する
            levelEntityManager.SetCanInput(true);
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            commandManager.Cleanup();
            return new MainGameData(isOnline: _mainGameData.IsOnline, isHost: _mainGameData.IsHost,levelId: _nextStageId);
        }

        public void Dispose()
        {
            levelEntityManager.Cleanup();
            if (_punMainGameScreen != null)
            {
                PhotonNetwork.Destroy(_punMainGameScreen.gameObject);
            }
            if (_punMainGameScreenReceiver != null)
            {
                PhotonNetwork.Destroy(_punMainGameScreenReceiver.gameObject);
            }
        }

        private async UniTask GoToStage(int stageId)
        {
            if (_isTransition) return;
            _isTransition= true;
            _nextStageId = stageId;
            
            if (_mainGameData.IsOnline)
            {
                
                _punMainGameScreen.GoToStage(stageId);
            }
            ScreenTransition.Instance.MoveFromGameToGame().Forget();
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
                _punMainGameScreen.GoToStage(-1);
            }
            ScreenTransition.Instance.MoveFromGameToGame().Forget();
        }
        
        public Camera GetCamera()
        {
            return myCamera;
        }

        /// <summary>
        /// CommandManagerからコマンドを受け取った時の処理
        /// </summary>
        private void TakeCommand(MainGameCommandType type)
        {
            switch (type)
            {
                case MainGameCommandType.Restart:
                    //最初からやり直す
                    _nextStageId = _mainGameData.LevelId;
                    ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
                    break;
                case MainGameCommandType.GoBack:
                    //ステージセレクト画面に戻る
                    _nextStageId = -1;
                    ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
                    break;
                case MainGameCommandType.Escape:
                    //Photonから接続してタイトルに戻る
                    if (_mainGameData.IsOnline)
                    {
                        RoomConnector.Instance.LeaveRoom();
                    }
                    ScreenTransition.Instance.Move(ScreenType.Title).Forget();
                    break;
                default:
                    Debug.LogError("MainGameScreenで不正なコマンドが渡されました");
                    break;
            }
        }
    }
}