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

        private bool _isCreateRoom;
        private bool _isJoinRoom;
        private bool _isLeaveRoom;
        
        public void CreateRoom(string roomName,RoomOptions roomOptions,CancellationToken token,
            Action successCallback=null,Action failCallback=null)
        {
            if (_isCreateRoom) return;
            UniTask.Void(async () =>
            {
                _isCreateRoom = true;
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
                _isCreateRoom = false;
            });
        }

        public void JoinRoom(string name,CancellationToken token,
            Action successCallback=null,Action failCallback=null)
        {
            if (_isJoinRoom) return;
            UniTask.Void(async () =>
            {
                _isJoinRoom = true;
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
                _isJoinRoom = false;
            });
        }

        public void LeaveRoom()
        {
            
            //既に部屋から抜けている場合は何もしない
            if (_isLeaveRoom) return;
            UniTask.Void(async () =>
            {
                _isLeaveRoom = true;
                if (_disconnectDetector != null)
                {
                    PhotonNetwork.Destroy(_disconnectDetector.gameObject);
                }

                await Pun2TaskNetwork.LeaveRoomAsync();
                _isLeaveRoom = false;
            });
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