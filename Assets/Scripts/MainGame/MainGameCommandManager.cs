using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    /// <summary>
    /// MainGameScreenでコマンド操作を行うためのモジュール
    /// </summary>
    public class MainGameCommandManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text explainText;
        [SerializeField] private MainGameCommandPasser passerPrefab;
        [SerializeField] private MainGameCommandReceiver receiverPrefab;
        [SerializeField] private RectTransform explainPanelTransform;

        private const KeyCode RestartKey = KeyCode.R;
        private const KeyCode GoBackKey = KeyCode.T;
        private const KeyCode EscapeKey = KeyCode.Escape;

        private const string MakeRestartRequestText = "Making Restart Request...\n Please Wait.";
        private const string TakeRestartRequestText = "Taking Restart Request.\n Please Press R Key.";
        private const string MakeGoBackRequestText = "Making GoBack Request...\n Please Wait.";
        private const string TakeGoBackRequestText = "Taking GoBack Request.\n Please Press T Key.";

        private const float AnimationTime = 0.2f;

        private bool _isActive;
        private bool _isOnline;
        private MainGameCommandPasser _passer;
        private MainGameCommandReceiver _receiver;

        private Subject<MainGameCommandType> _commandSubject;
        public IObservable<MainGameCommandType> OnCommandObservable => _commandSubject;

        private CommandState _commandState;
        private Tween _tween;
        private bool _isPanelActive;

        public void Setup(bool isOnline)
        {
            _isActive = true;
            _isOnline = isOnline;
            _commandSubject = new Subject<MainGameCommandType>();
            _commandState = new CommandState(MainGameCommandType.None, false);
            SetText(_commandState);
            if (isOnline)
            {
                var myTransform = transform;
                _passer = PhotonNetwork.Instantiate(passerPrefab.name, Vector3.zero, Quaternion.identity)
                    .GetComponent<MainGameCommandPasser>();
                _passer.transform.parent = myTransform;
                _receiver = Instantiate(receiverPrefab, myTransform, true);
                _receiver.ReceiverMakeRequestObservable.Subscribe(ReceiverMakeRequest).AddTo(gameObject);
                _receiver.ReceiverTakeRequestObservable.Subscribe(ReceiverTakeRequest).AddTo(gameObject);
            }
        }

        public void Cleanup()
        {
            SetText(new CommandState(MainGameCommandType.None, false));
            _isActive = false;
            _commandSubject?.Dispose();

            if (_passer != null)
            {
                PhotonNetwork.Destroy(_passer.gameObject);
            }

            if (_receiver != null)
            {
                Destroy(_receiver.gameObject);
            }
        }

        private void OnDestroy()
        {
            KillTween();
        }

        private void SetText(CommandState state)
        {
            switch (state.Type)
            {
                case MainGameCommandType.None:
                    explainText.text = "";
                    break;
                case MainGameCommandType.Restart:
                    explainText.text = state.IsMaking ? MakeRestartRequestText : TakeRestartRequestText;
                    break;
                case MainGameCommandType.GoBack:
                    explainText.text = state.IsMaking ? MakeGoBackRequestText : TakeGoBackRequestText;
                    break;
                case MainGameCommandType.Escape:
                    return;
                default:
                    Debug.LogError($"CommandStateが不正です: {state}");
                    break;
            }

            if (state.Type == MainGameCommandType.None)
            {
                SetPanelActive(false);
            }
            else
            {
                if (_isPanelActive)
                {
                    UniTask.Void(async () =>
                    {
                        SetPanelActive(false);
                        await UniTask.Delay(TimeSpan.FromSeconds(AnimationTime),
                            cancellationToken: this.GetCancellationTokenOnDestroy());
                        SetPanelActive(true);
                    });
                }
                else
                {
                    SetPanelActive(true);
                }
            }
        }

        private void SetPanelActive(bool isActive)
        {
            KillTween();
            var posY = isActive ? 300 : 600;
            _tween = explainPanelTransform?.DOAnchorPos(new Vector2(0, posY), AnimationTime);
            _isPanelActive = isActive;
        }

        private void Update()
        {
            if (!_isActive) return;

            if (Input.GetKeyDown(RestartKey))
            {
                if (_isOnline)
                {
                    CheckCommand(MainGameCommandType.Restart);
                }
                else
                {
                    _commandSubject.OnNext(MainGameCommandType.Restart);
                }
            }

            if (Input.GetKeyDown(GoBackKey))
            {
                if (_isOnline)
                {
                    CheckCommand(MainGameCommandType.GoBack);
                }
                else
                {
                    _commandSubject.OnNext(MainGameCommandType.GoBack);
                }
            }

            if (Input.GetKeyDown(EscapeKey))
            {
                _commandSubject.OnNext(MainGameCommandType.Escape);
            }
        }

        private void ReceiverMakeRequest(MainGameCommandType type)
        {
            _commandState = new CommandState(type: type, isMaking: false);
            SetText(_commandState);
        }
        
        private void ReceiverTakeRequest(MainGameCommandType type)
        {
            //コマンドを実行する
            _commandSubject.OnNext(type);
            _isActive = false;
        }

        private void CheckCommand(MainGameCommandType type)
        {
            if (_commandState.Type == type && !_commandState.IsMaking)
            {
                //相手に承認した事を伝えて、自身はそれを実行する
                _passer.SendTakeRequest(type);
                _commandSubject.OnNext(type);
            }
            else
            {
                //相手にRestartのリクエストを送る
                _commandState = new CommandState(type: type, isMaking: true);
                SetText(_commandState);
                _passer.SendMakeRequest(type);
            }
        }

        private void KillTween()
        {
            if (_tween != null || !_tween.IsActive()) return;
            _tween.Kill();
        }
    }

    public struct CommandState
    {
        public MainGameCommandType Type { get; }
        public bool IsMaking { get; }

        public CommandState(MainGameCommandType type, bool isMaking)
        {
            Type = type;
            IsMaking = isMaking;
        }
    }
}