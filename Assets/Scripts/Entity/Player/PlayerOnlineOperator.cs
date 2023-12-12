using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerOnlineOperator : MonoBehaviourPun
    {
        [SerializeField] private PlayerMover mover;

        public void SetKeySet(PlayerOperateKeySet keySet)
        {
            photonView.RPC(nameof(RPC_SetKeySet), RpcTarget.All, keySet.Up, keySet.Down, keySet.Left, keySet.Right);
        }

        [PunRPC]
        public void RPC_SetKeySet(KeyCode up, KeyCode down, KeyCode left, KeyCode right)
        {
            var keySet = new PlayerOperateKeySet(up: up, down: down, left: left, right: right);
            mover.SetKeySet(keySet);
        }

        /// <summary>
        /// 操作権を与える
        /// </summary>
        /// <param name="isHostControl">Hostが操作できる時、true<br/>Clientが操作できる時、false</param>
        public void SetControlAuthority(bool isHostControl)
        {
            if (!isHostControl)
            {
                photonView.RPC(nameof(GetOwnership), RpcTarget.Others);
            }
            else
            {
                mover.SetControlAuthority(true);
            }
        }

        /// <summary>
        /// 操作権を要求する
        /// </summary>
        [PunRPC]
        private void GetOwnership()
        {
            photonView.RequestOwnership();
            mover.SetControlAuthority(true);
        }
    }
}