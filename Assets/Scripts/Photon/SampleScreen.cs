using Cysharp.Threading.Tasks;
using Photon.Pun;
using Pun2Task;
using UnityEngine;

namespace Hackathon2023Winter
{
    public class SampleScreen:MonoBehaviourPunCallbacks
    {
        [SerializeField]private MatchingView matchingView;
        private void Start()
        {
            UniTask.Void(async () =>
            {
                PhotonNetwork.NickName = "Player";
                await Pun2TaskNetwork.ConnectUsingSettingsAsync();
                matchingView.Setup();
            });
            PhotonNetwork.NickName = "Player";
            PhotonNetwork.ConnectUsingSettings();
            matchingView.Setup();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedRoom()
        {
            var position = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3));
            PhotonNetwork.Instantiate("Avatar", position, Quaternion.identity);
        }
    }
}