using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Entity;
using Hackathon2023Winter.Level;
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
                //ステージIDに応じてx座標をずらす
                transform.position = new Vector3(mainGameScreenData.LevelId*1000, 0, 0);
                
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
                    if (mainGameScreenData.IsHost)
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
                    }
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
            levelEntityManager.Cleanup();
            if (_punMainGameScreen != null)
            {
                PhotonNetwork.Destroy(_punMainGameScreen.gameObject);
            }
            if (_punMainGameScreenReceiver != null)
            {
                PhotonNetwork.Destroy(_punMainGameScreenReceiver.gameObject);
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
    }
}