using Photon.Pun;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    /// <summary>
    /// Photon下でPUN2に紐づく操作を行うためのMainGameScreen直下のコンポーネント
    /// </summary>
    public class PunMainGameScreen : MonoBehaviourPunCallbacks
    {
        public void GoToStage(int stageId)
        {
            photonView.RPC(nameof(GoToStageRPC), RpcTarget.Others, stageId);
        }
        
        [PunRPC]
        private void GoToStageRPC(int stageId)
        {
            var receiver = GameObject.FindWithTag(TagName.PunMainGameScreenReceiver)?.GetComponent<PunMainGameScreenReceiver>();
            receiver?.GoTo(stageId);
        }
    }
}