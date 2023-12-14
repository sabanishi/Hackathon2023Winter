using Photon.Pun;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameCommandPasser:MonoBehaviourPun
    {
        public void SendMakeRequest(MainGameCommandType type)
        {
            photonView.RPC(nameof(RPC_SendMakeRequest), RpcTarget.Others, type);
        }
        
        [PunRPC]
        private void RPC_SendMakeRequest(MainGameCommandType type)
        {
            var objs = GameObject.FindGameObjectsWithTag(TagName.MainGameCommandReceiver);
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent(typeof(MainGameCommandReceiver), out var receiverComponent))
                {
                    var receiver = (MainGameCommandReceiver) receiverComponent;
                    receiver.ReceiverMakeRequest(type);
                }
            }
        }
        
        public void SendTakeRequest(MainGameCommandType type)
        {
            photonView.RPC(nameof(RPC_SendTakeRequest), RpcTarget.Others, type);
        }
        
        [PunRPC]
        private void RPC_SendTakeRequest(MainGameCommandType type)
        {
            var objs = GameObject.FindGameObjectsWithTag(TagName.MainGameCommandReceiver);
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent(typeof(MainGameCommandReceiver), out var receiverComponent))
                {
                    var receiver = (MainGameCommandReceiver) receiverComponent;
                    receiver.ReceiverTakeRequest(type);
                }
            }
        }
    }
}