using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using Pun2Task;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Matching
{
    public class MatchingScreen:BaseScreen
    {
        [SerializeField]private MatchingView matchingView;
        [SerializeField]private MatchingCallBack matchingCallBack;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            matchingView.Setup();
            matchingCallBack.Setup();
            matchingCallBack.OnJoinedRoomSubject.Subscribe(_ =>
            {
                //MainGameSceneに遷移
                ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
            }).AddTo(gameObject);
            
            PhotonNetwork.NickName = "Player";
            await Pun2TaskNetwork.ConnectUsingSettingsAsync(token:token);
            PhotonNetwork.JoinLobby();
        }

        protected override async UniTask OpenAnimationInternal(CancellationToken token)
        {
            await NowLoadingPanel.Instance.OpenAnimation(0.5f, token);
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            var mainGameData = new MainGameData(true, 0);
            return mainGameData;
        }
    }
}