using Photon.Pun;
using Photon.Realtime;
using Sabanishi.Common;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Matching
{
    public class MatchingScreenPresenter : MonoBehaviourPunCallbacks
    {
        [SerializeField] private RoomListView roomListView;
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button createRoomButton;

        public void Setup()
        {
            roomListView.Setup();
            roomListView.OnJoinRoomAsObservable.Subscribe(JoinRoom).AddTo(gameObject);
            createRoomButton.SafeOnClickAsObservable().Subscribe(_ => OnCreateRoomButtonClicked()).AddTo(gameObject);
            roomNameInputField.onValueChanged.AddListener(OnRoomNameInputFieldChanged);
            canvasGroup.interactable = false;
            OnRoomNameInputFieldChanged(roomNameInputField.text);
        }

        public void Cleanup()
        {
            roomListView.Cleanup();
        }

        /// <summary>
        /// ルーム作成ボタンが押された際の処理
        /// </summary>
        private void OnCreateRoomButtonClicked()
        {
            canvasGroup.interactable = false;

            var roomOptions = new RoomOptions
            {
                MaxPlayers = 2
            };
            PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        }

        private void OnRoomNameInputFieldChanged(string roomName)
        {
            //ルーム名が空なら作成ボタンを押せないようにする
            createRoomButton.interactable = !string.IsNullOrEmpty(roomName);
        }

        private void JoinRoom(RoomInfo roomInfo)
        {
            //ルーム参加処理中は入力を受け付けないようにする
            canvasGroup.interactable = false;
            //ルームに参加する
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }

        public override void OnJoinedLobby()
        {
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
            canvasGroup.interactable = false;
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            //ルーム参加処理が失敗したら、再び入力を受け付ける
            canvasGroup.interactable = true;
        }
    }
}