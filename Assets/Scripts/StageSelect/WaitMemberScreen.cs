using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Matching;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using Sabanishi.Common;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.StageSelect
{
    public class WaitMemberScreen : BaseScreen
    {
        [SerializeField] private Button cancelButton;
        [SerializeField] private PunWaitMemberScreen punWaitMemberScreenPrefab;

        private MainGameData _mainGameData;
        private PunWaitMemberScreen _punWaitMemberScreen;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            SoundManager.PlayBGM(BGM_Enum.TITLE);
            if (screenData is MainGameData mainGameData)
            {
                _mainGameData = mainGameData;
            }
            else
            {
                Debug.LogError("StageSelectDataが渡されていません");
            }

            cancelButton.SafeOnClickAsObservable().Subscribe(_ => Cancel()).AddTo(gameObject);
            _punWaitMemberScreen = PhotonNetwork
                .Instantiate(punWaitMemberScreenPrefab.name, Vector3.zero, Quaternion.identity)
                .GetComponent<PunWaitMemberScreen>();
            _punWaitMemberScreen.Setup();
            _punWaitMemberScreen.OnJoinedRoomSubject.Subscribe(_ => GoToStageSelect()).AddTo(gameObject);
        }

        private void GoToStageSelect()
        {
            ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
        }

        private void Cancel()
        {
            //Photonから退出する
            RoomConnector.Instance.LeaveRoom();
            ScreenTransition.Instance.Move(ScreenType.Title).Forget();
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            if (_punWaitMemberScreen != null)
            {
                _punWaitMemberScreen.Cleanup();
                PhotonNetwork.Destroy(_punWaitMemberScreen.gameObject);
            }

            return _mainGameData;
        }
    }
}