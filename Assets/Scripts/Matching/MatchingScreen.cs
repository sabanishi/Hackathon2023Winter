using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using Pun2Task;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Matching
{
    public class MatchingScreen : BaseScreen
    {
        [SerializeField] private MatchingCallback matchingCallback;
        [SerializeField] private MatchingScreenPresenter matchingScreenPresenter;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            SoundManager.PlayBGM(BGM_Enum.TITLE);
            matchingScreenPresenter.Setup();
            matchingCallback.Setup();
            matchingCallback.OnJoinedRoomSubject.Subscribe(_ =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    //ホストの場合はWaitMemberScreenに遷移
                    ScreenTransition.Instance.Move(ScreenType.WaitMember).Forget();
                }
                else
                {
                    //StageSelectScreenに遷移
                    ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
                }
            }).AddTo(gameObject);

            PhotonNetwork.NickName = "Player";
            //Photonに接続
            await Pun2TaskNetwork.ConnectUsingSettingsAsync(token: token);
            PhotonNetwork.SendRate = 50;
            PhotonNetwork.SerializationRate = 25;
            //ロビーに入る
            PhotonNetwork.JoinLobby();
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            matchingScreenPresenter.Cleanup();
            var mainGameData = new MainGameData(true, PhotonNetwork.IsMasterClient,-1,false);
            return mainGameData;
        }


        protected override async UniTask OpenAnimationInternal(CancellationToken token)
        {
            await NowLoadingAnimation.Instance.OpenAnimation(0.5f, token);
        }
    }
}