using Photon.Pun;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.StageSelect
{
    /// <summary>
    /// StageSelectScreenでのHostの操作をClientに渡すためのクラス
    /// </summary>
    public class HostOperatePasser : MonoBehaviourPunCallbacks
    {
        private HostOperateReceiver _hostOperateReceiver;

        public HostOperateReceiver HostOperateReceiver
        {
            get
            {
                if (_hostOperateReceiver == null)
                {
                    _hostOperateReceiver = GameObject.FindGameObjectWithTag(TagName.HostOperateReceiver)
                        .GetComponent<HostOperateReceiver>();
                }

                return _hostOperateReceiver;
            }
        }

        public void OnTmpButtonClick()
        {
            photonView.RPC(nameof(OnTmpButtonClickedRPC), RpcTarget.Others);
        }

        [PunRPC]
        private void OnTmpButtonClickedRPC()
        {
            HostOperateReceiver.OnReceiveTmpButtonClicked();
        }
    }
}