using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerOnlineOperator : MonoBehaviourPun
    {
        [SerializeField] private PlayerMover mover;

        private bool _isActive;
        private bool _canInput;

        private KeyConditions _keyConditions;

        public void SetKeyConditions(KeyConditions keyConditions)
        {
            _keyConditions = keyConditions;
        }

        public void SetActive(bool isActive)
        {
            photonView.RPC(nameof(RPC_SetActive), RpcTarget.All, isActive);
        }

        public void SetMoverActive(bool isActive)
        {
            mover.SetControlAuthority(isActive);
        }

        [PunRPC]
        public void RPC_SetActive(bool isActive)
        {
            _isActive = isActive;
        }

        private PlayerOperateKeySet _keySet;
        private bool _canControl;

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
                _canControl = true;
            }
        }

        /// <summary>
        /// 操作権を要求する
        /// </summary>
        [PunRPC]
        private void GetOwnership()
        {
            _canControl = true;
        }

        private void Update()
        {
            if (!_isActive) return;
            if (!_canInput) return;
            //キー入力をMoverに渡す
            if (_keyConditions == null) return;
            if (photonView.IsMine)
            {
                mover.Move(_keyConditions);

                //UP/Down系をfalseにする
                _keyConditions.IsLeftDown = false;
                _keyConditions.IsRightDown = false;
                _keyConditions.IsJumpDown = false;
                _keyConditions.IsLeftUp = false;
                _keyConditions.IsRightUp = false;
                _keyConditions.IsJumpUp = false;
            }
            else
            {
                if (mover is PlayerRectMover)
                {
                    mover.Move(_keyConditions);
                }
            }
        }
        
        public void SetCanInput(bool canInput)
        {
            _canInput = canInput;
        }
    }
}