using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hackathon2023Winter.Entity;
using Hackathon2023Winter.Level;
using Hackathon2023Winter.Matching;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameScreen : BaseScreen
    {
        [SerializeField] private Camera myCamera;
        [SerializeField] private LevelEntityManager levelEntityManager;
        [SerializeField] private MainGameCommandManager commandManager;
        [SerializeField]private MainGameMoveStageManager moveStageManager;
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private PunMainGameScreen punMainGameScreenPrefab;
        [SerializeField] private PunMainGameScreenReceiver punMainGameScreenReceiverPrefab;

        private bool _isTransition;
        private MainGameData _mainGameData;
        private PunMainGameScreen _punMainGameScreen;
        private PunMainGameScreenReceiver _punMainGameScreenReceiver;
        private int _nextStageId = -1;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            SoundManager.PlayBGM(BGM_Enum.PUZZLE);
            _isTransition = false;
            if (screenData is MainGameData mainGameScreenData)
            {
                var isOnline = mainGameScreenData.IsOnline;
                var isHost = mainGameScreenData.IsHost;
                var levelId = mainGameScreenData.LevelId;
                var isHostCreateLevel = false;

                commandManager.Setup(isOnline);
                commandManager.OnCommandObservable.Subscribe(TakeCommand).AddTo(gameObject);
                moveStageManager.Setup(isOnline,isHost);
                moveStageManager.MoveStageObservable.Subscribe(GoToStage).AddTo(gameObject);

                if (levelId == -1)
                {
                    //ステージセレクト画面
                    instructionPanel.SetActive(!mainGameScreenData.IsGameToGame);
                }
                else
                {
                    instructionPanel.SetActive(false);
                }

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
                        _punMainGameScreenReceiver.OnCreateLevelObservable.Subscribe(_ => isHostCreateLevel = true)
                            .AddTo(gameObject);
                    }
                }

                //ステージIDに応じてx座標をずらす
                transform.position = new Vector3(levelId * 1000, 0, 0);

                _mainGameData = mainGameScreenData;
                levelEntityManager.Setup(isOnline, isHost);
                //オンラインでないかホストの場合はステージを生成する
                if (!isOnline || isHost)
                {
                    levelEntityManager.CreateLevel(isOnline, levelId);
                    _punMainGameScreen?.SendCreateLevel();
                }
                
                levelEntityManager.OnClearObservable.Subscribe(x => GameClear(x.clearObj, x.goalEntity).Forget())
                    .AddTo(gameObject);
                levelEntityManager.OnEnterObservable.Subscribe(x => moveStageManager.OnCanStageSelect(x.stageId,x.isCircle||!isOnline)).AddTo(gameObject);
                levelEntityManager.OnExitObservable.Subscribe(x => moveStageManager.OnCannotStageSelect(x.isCircle||!isOnline)).AddTo(gameObject);
            }
            else
            {
                Debug.LogError("MainGameScreenに渡されたデータが不正です");
            }
        }

        protected override async UniTask OpenDealInternal(CancellationToken token)
        {
            if (_mainGameData.LevelId == -1)
            {
                //ステージセレクト画面
                instructionPanel.SetActive(true);
            }
            else
            {
                instructionPanel.SetActive(false);
            }
            //ユーザーの入力受付を開始する
            levelEntityManager.SetCanInput(true);
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            instructionPanel.SetActive(false);
            commandManager.Cleanup();
            moveStageManager.Cleanup();
            return new MainGameData(isOnline: _mainGameData.IsOnline, isHost: _mainGameData.IsHost,
                levelId: _nextStageId,true);
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

        /// <summary>
        /// 指定したステージに遷移する
        /// </summary>
        private void GoToStage(int stageId)
        {
            if (_isTransition) return;
            _isTransition = true;
            _nextStageId = stageId;

            if (_mainGameData.IsOnline)
            {
                _punMainGameScreen.GoToStage(stageId);
            }

            ScreenTransition.Instance.MoveFromGameToGame().Forget();
        }

        private async UniTask GameClear(GameObject clearObject, GoalEntity goalEntity)
        {
            if (_isTransition) return;
            SoundManager.PlaySE(SE_Enum.WAVE,true);
            var goalObject = goalEntity.gameObject;
            var token = this.GetCancellationTokenOnDestroy();
            _isTransition = true;
            _nextStageId = -1;

            //全てのEntityの物理演算を切る
            levelEntityManager.SetIsSimulateActive(false);
            //Goalに触れたオブジェクトをゴール位置の位置までサイズを小さくしながら近づける
            float clearObjTime = 0.1f;
            UniTask.WhenAll(
                clearObject.transform.DOMove(goalObject.transform.position, clearObjTime).SetEase(Ease.InCubic)
                    .ToUniTask(cancellationToken: token),
                clearObject.transform.DOScale(0.01f, clearObjTime).ToUniTask(cancellationToken: token));
            if (token.IsCancellationRequested) return;

            //全てのEntityをゴールの位置までサイズを小さくしながら近づける
            float entityTime = 0.8f;
            float intervalTime = 0.1f;
            float rotateSpeed = 300f;
            var entities = levelEntityManager.GetEntities();
            //entitiesをgoalObjectに近い順に並び替える
            entities.Sort((a, b) =>
            {
                var aPos = a.transform.position;
                var bPos = b.transform.position;
                var aDis = Vector3.Distance(aPos, goalObject.transform.position);
                var bDis = Vector3.Distance(bPos, goalObject.transform.position);
                return aDis.CompareTo(bDis);
            });
            //順にゴールまでスケールを小さくしながら移動させる
            int i = 0;
            int counter = 0;
            foreach (var entity in entities)
            {
                if (entity.gameObject == clearObject || entity.gameObject == goalObject)
                {
                    counter++;
                    continue;
                }
                var entityTransform = entity.transform;
                var time = entityTime * (Random.value / 4 + i * 0.2f + 0.75f);
                var interval = intervalTime * (Random.value / 4 * i * 0.2f + 0.75f);
                var rotateAngle = 360f * (Random.value / 4 * i * 0.2f + 0.75f);
                UniTask.Void(async () =>
                {
                    await UniTask.WhenAll(
                        entityTransform.DOMove(goalObject.transform.position, time).SetEase(Ease.InCubic)
                            .ToUniTask(cancellationToken: token),
                        entityTransform.DOScale(0.01f, time).SetEase(Ease.InQuart).ToUniTask(cancellationToken: token),
                        entityTransform.DORotate(new Vector3(0,0,rotateAngle),time,RotateMode.FastBeyond360).SetEase(Ease.InQuad).ToUniTask(cancellationToken:token));
                    counter++;
                });
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
                if (token.IsCancellationRequested) return;
                i++;
            }
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, token);
            UniTask.Void(async () =>
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    levelEntityManager?.SendPlayerScaleInfo();
                    await UniTask.Yield();
                }
            });

            await UniTask.WaitUntil(() => counter == entities.Count, cancellationToken: token);
            cancellationTokenSource.Cancel();
            
            //TODO:GoalのScaleが小さくなるアニメーション
            await goalEntity.PlayGoalAnimation(0.8f, token);
            
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