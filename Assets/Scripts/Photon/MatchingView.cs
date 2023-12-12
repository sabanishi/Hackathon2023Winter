using Photon.Pun;
using Photon.Realtime;
using Sabanishi.Common;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter
{
    public class MatchingView:MonoBehaviourPunCallbacks
    {

        [SerializeField] private RoomListView roomListView;
        [SerializeField]private TMP_InputField roomNameInputField;
        [SerializeField] private Button createRoomButton;
        [SerializeField]private CanvasGroup canvasGroup;

        public void Setup()
        {
            roomListView.Setup();
            roomListView.OnJoinRoomAsObservable.Subscribe(JoinRoom).AddTo(gameObject);
            createRoomButton.SafeOnClickAsObservable().Subscribe(_ => OnCreateRoomButtonClicked()).AddTo(gameObject);
            roomNameInputField.onValueChanged.AddListener(OnRoomNameInputFieldChanged);
            canvasGroup.interactable = false;
        }

        public void Cleanup()
        {
            roomListView.Cleanup();
        }
        
        private void OnCreateRoomButtonClicked()
        {
            canvasGroup.interactable = false;

            var roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        }
        
        private void OnRoomNameInputFieldChanged(string roomName)
        {
            //ルーム名が空なら作成ボタンを押せないようにする
            createRoomButton.interactable = !string.IsNullOrEmpty(roomName);
        }
        
        private void JoinRoom(RoomInfo roomInfo)
        {
            //ルーム参加処理中は入力を受け付けない
            canvasGroup.interactable = false;
            
            //ルームに参加する
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }
        
        public override void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby");
            //ロビーに参加したら、入力できるようにする
            canvasGroup.interactable = true;
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            //ルームの作成が失敗したら、再び入力を受け付ける
            canvasGroup.interactable = true;
            roomNameInputField.text = string.Empty;
        }

        public override void OnJoinedRoom()
        {
            //ルーム参加処理が完了したら、UIを非表示にする
            gameObject.SetActive(false);
        }
        
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            //ルーム参加処理が失敗したら、再び入力を受け付ける
            canvasGroup.interactable = true;
        }
    }
}