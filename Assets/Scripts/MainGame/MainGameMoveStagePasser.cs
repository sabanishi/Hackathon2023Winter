using System;
using Photon.Pun;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameMoveStagePasser:MonoBehaviourPun
    {
        private bool _isDestroy;
        private void OnDestroy()
        {
            _isDestroy = true;
        }

        public void SendCanStageSelect(int stageId)
        {
            photonView.RPC(nameof(RPC_SendCanStageSelect), RpcTarget.Others,stageId);
        }
        
        [PunRPC]
        private void RPC_SendCanStageSelect(int stageId)
        {
            var objs = GameObject.FindGameObjectsWithTag(TagName.MainGameMoveStageReceiver);
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent(typeof(MainGameMoveStageReceiver), out var receiverComponent))
                {
                    Debug.Log("SendCanStageSelect");
                    var receiver = (MainGameMoveStageReceiver) receiverComponent;
                    receiver.ReceiverCanStageSelect(stageId);
                }
            }
        }
        
        public void SendCannotStageSelect()
        {
            if(_isDestroy) return;
            photonView.RPC(nameof(RPC_SendCannotStageSelect), RpcTarget.Others);
        }
        
        [PunRPC]
        private void RPC_SendCannotStageSelect()
        {
            var objs = GameObject.FindGameObjectsWithTag(TagName.MainGameMoveStageReceiver);
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent(typeof(MainGameMoveStageReceiver), out var receiverComponent))
                {
                    var receiver = (MainGameMoveStageReceiver) receiverComponent;
                    receiver.ReceiverCannotStageSelect();
                }
            }
        }

        public void SendGoToStage(int stageId)
        {
            photonView.RPC(nameof(RPC_SendGoToStage), RpcTarget.Others,stageId);
        }

        [PunRPC]
        private void RPC_SendGoToStage(int stageId)
        {
            var objs = GameObject.FindGameObjectsWithTag(TagName.MainGameMoveStageReceiver);
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent(typeof(MainGameMoveStageReceiver), out var receiverComponent))
                {
                    var receiver = (MainGameMoveStageReceiver) receiverComponent;
                    receiver.ReceiverGoToStage(stageId);
                }
            }
        }
    }
}