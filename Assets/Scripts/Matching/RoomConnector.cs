using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using Pun2Task;
using Sabanishin.Common;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Matching
{
    /// <summary>
    /// Photonのルームに接続するための静的クラス
    /// </summary>
    public class RoomConnector:SingletonMonoBehaviour<RoomConnector>
    {
        [SerializeField] private DisconnectDetector disconnectDetectorPrefab;
        [SerializeField] private DisconnectPanel disconnectPanel;
        private DisconnectDetector _disconnectDetector;
        
        
        public void CreateRoom(string roomName,RoomOptions roomOptions,CancellationToken token,
            Action successCallback=null,Action failCallback=null)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await Pun2TaskNetwork.CreateRoomAsync(roomName: roomName, roomOptions: roomOptions, token: token);
                    successCallback?.Invoke();
                    CreateDisconnectDetector();
                }
                catch (Pun2TaskNetwork.ConnectionFailedException e)
                {
                    failCallback?.Invoke();
                }
            });
        }

        public void JoinRoom(string name,CancellationToken token,
            Action successCallback=null,Action failCallback=null)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await Pun2TaskNetwork.JoinRoomAsync(roomName: name, token: token);
                    successCallback?.Invoke();
                    CreateDisconnectDetector();
                }
                catch (Pun2TaskNetwork.ConnectionFailedException e)
                {
                    failCallback?.Invoke();
                }
            });
        }

        public void LeaveRoom()
        {
            if (_disconnectDetector != null)
            {
                PhotonNetwork.Destroy(_disconnectDetector.gameObject);
            }
            PhotonNetwork.LeaveRoom();
        }
        
        private void CreateDisconnectDetector()
        {
            if (_disconnectDetector != null)
            {
                PhotonNetwork.Destroy(_disconnectDetector.gameObject);
            }
            _disconnectDetector = PhotonNetwork.Instantiate(disconnectDetectorPrefab.name, Vector3.zero, Quaternion.identity)
                .GetComponent<DisconnectDetector>();
            _disconnectDetector.OnDisconnectedAsObservable.Subscribe(_=>disconnectPanel.Show()).AddTo(gameObject);
        }
    }
}