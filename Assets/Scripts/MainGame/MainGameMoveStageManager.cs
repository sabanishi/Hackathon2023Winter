using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameMoveStageManager:MonoBehaviour
    {
        [SerializeField] private MainGameMoveStagePasser passerPrefab;
        [SerializeField] private MainGameMoveStageReceiver receiverPrefab;
        [SerializeField] private RectTransform explainPanelTransform;
        
        private static readonly KeyCode SpaceKey = KeyCode.Space;
        
        private const float AnimationTime = 0.2f;
        
        private bool _isActive;
        private bool _isOnline;
        private bool _isHost;
        private MainGameMoveStagePasser _passer;
        private MainGameMoveStageReceiver _receiver;
        private bool _isCanStageSelect;
        private int _stageId;
        private bool _canSelectStage;

        private Subject<int> _moveStageSubject;
        public IObservable<int> MoveStageObservable => _moveStageSubject;
        
        private Tween _tween;
        
        public void Setup(bool isOnline,bool isHost)
        {
            _moveStageSubject = new Subject<int>();
            _isActive = true;
            _isOnline = isOnline;
            _isHost = isHost;
            if (isOnline)
            {
                var myTransform = transform;
                _passer = PhotonNetwork.Instantiate(passerPrefab.name, Vector3.zero, Quaternion.identity)
                    .GetComponent<MainGameMoveStagePasser>();
                _passer.transform.parent = myTransform;
                _receiver = Instantiate(receiverPrefab, myTransform, true);
                _receiver.ReceiverCanStageSelectObservable.Subscribe(x =>
                {
                    SetCanStageSelect(true,x);
                }).AddTo(gameObject);
                _receiver.ReceiverCannotStageSelectObservable.Subscribe(x =>
                {
                    SetCanStageSelect(false);
                }).AddTo(gameObject);
                _receiver.ReceiverGoToStageObservable.Subscribe(x =>
                {
                    _moveStageSubject.OnNext(x);
                }).AddTo(gameObject);
            }
        }

        public void Cleanup()
        {
            SetCanStageSelect(false);
            _moveStageSubject?.Dispose();
            _isActive = false;

            if (_passer != null)
            {
                PhotonNetwork.Destroy(_passer.gameObject);
            }

            if (_receiver != null)
            {
                Destroy(_receiver.gameObject);
            }
        }

        /// <summary>
        /// Stageを選択できる状態にする
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="isHost"></param>
        public void OnCanStageSelect(int stageId,bool isHost)
        {
            if (isHost)
            {
                SetCanStageSelect(true,stageId);
            }
            else
            {
                _passer?.SendCanStageSelect(stageId);
            }
        }

        /// <summary>
        /// Stageを選択できない状態にする
        /// </summary>
        /// <param name="isHost"></param>
        public void OnCannotStageSelect(bool isHost)
        {
            if (isHost)
            {
                SetCanStageSelect(false);
            }
            else
            {
                _passer?.SendCannotStageSelect();
            }
        }

        private void SetCanStageSelect(bool canStageSelect, int stageId = 0)
        {
            _canSelectStage = canStageSelect;
            _stageId = stageId;
            if (!_canSelectStage)
            {
                SetPanelActive(false);
            }
            else
            {
                SetPanelActive(true);
            }
        }
        
        private void SetPanelActive(bool isActive)
        {
            if (isActive)
            {
                SoundManager.PlaySE(SE_Enum.COMMANDPANEL);
            }
            KillTween();
            var posY = isActive ? 300 : 600;
            _tween = explainPanelTransform?.DOAnchorPos(new Vector2(-400, posY), AnimationTime);
        }

        private void Update()
        {
            if (!_isActive) return;
            if (!_canSelectStage) return;

            if (Input.GetKeyDown(SpaceKey))
            {
                if (!_isOnline || _isHost)
                {
                    //画面遷移を行う
                    _moveStageSubject.OnNext(_stageId);
                }
                else
                {
                    //Hostに画面遷移を行うようにリクエストを送る
                    _passer.SendGoToStage(_stageId);
                }
            }
        }
        
        private void OnDestroy()
        {
            KillTween();
        }
        
        private void KillTween()
        {
            if (_tween != null || !_tween.IsActive()) return;
            _tween.Kill();
        }
    }
}