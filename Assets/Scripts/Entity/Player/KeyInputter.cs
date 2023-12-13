using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// ユーザーのキー入力を受け取るクラス
    /// </summary>
    public class KeyInputter : MonoBehaviourPun, IPunObservable
    {
        private KeyConditions _keyConditions;
        private PlayerOperateKeySet _keySet;
        [SerializeField] private PlayerOnlineOperator onlineOperator;
        
        public void SetKeySet(PlayerOperateKeySet keySet)
        {
            photonView.RPC(nameof(RPC_SetKeySet), RpcTarget.All, keySet.Up, keySet.Down, keySet.Left, keySet.Right);
        }

        [PunRPC]
        public void RPC_SetKeySet(KeyCode up, KeyCode down, KeyCode left, KeyCode right)
        {
            var keySet = new PlayerOperateKeySet(up: up, down: down, left: left, right: right);
            _keySet = keySet;
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
        }

        /// <summary>
        /// 操作権を要求する
        /// </summary>
        [PunRPC]
        private void GetOwnership()
        {
            photonView.RequestOwnership();
        }

        //キー入力を渡す
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_keyConditions?.IsLeft);
                stream.SendNext(_keyConditions?.IsRight);
                stream.SendNext(_keyConditions?.IsJump);
            }
            else
            {
                if (stream.PeekNext() == null) return;
                var isLeft = false;
                var isRight = false;
                var isJump = false;
                if (_keyConditions != null)
                {
                    isLeft = _keyConditions.IsLeft;
                    isRight = _keyConditions.IsRight;
                    isJump = _keyConditions.IsJump;
                }

                var nowIsLeft = (bool)stream.ReceiveNext();
                var nowIsRight = (bool)stream.ReceiveNext();
                var nowIsJump = (bool)stream.ReceiveNext();
                _keyConditions = new KeyConditions();
                _keyConditions.SetKeySet(_keySet);
                _keyConditions.IsLeft = nowIsLeft;
                _keyConditions.IsRight = nowIsRight;
                _keyConditions.IsJump = nowIsJump;
                //Up/Downを判断する
                if (isLeft && !nowIsLeft)
                {
                    _keyConditions.IsLeftUp = true;
                }
                else
                {
                    _keyConditions.IsLeftUp = false;
                }

                if (isRight && !nowIsRight)
                {
                    _keyConditions.IsRightUp = true;
                }
                else
                {
                    _keyConditions.IsRightUp = false;
                }

                if (isJump && !nowIsJump)
                {
                    _keyConditions.IsJumpUp = true;
                }
                else
                {
                    _keyConditions.IsJumpUp = false;
                }

                if (!isLeft && nowIsLeft)
                {
                    _keyConditions.IsLeftDown = true;
                }
                else
                {
                    _keyConditions.IsLeftDown = false;
                }

                if (!isRight && nowIsRight)
                {
                    _keyConditions.IsRightDown = true;
                }
                else
                {
                    _keyConditions.IsRightDown = false;
                }

                if (!isJump && nowIsJump)
                {
                    _keyConditions.IsJumpDown = true;
                }
                else
                {
                    _keyConditions.IsJumpDown = false;
                }

                onlineOperator.SetKeyCondition2(_keyConditions);
            }
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            _keyConditions = new KeyConditions();
            _keyConditions.SetKeySet(_keySet);
            _keyConditions.UpdateCondition();
            onlineOperator.SetKeyConditions(_keyConditions);
        }
    }
}