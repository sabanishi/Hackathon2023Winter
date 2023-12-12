using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// ユーザーが動かすことのできるEntityの基底クラス
    /// </summary>
    public class PlayerEntity : BaseEntity,IShapable
    {
        [SerializeField] private PlayerOfflineOperator offlineOperator;
        [SerializeField] private PlayerOnlineOperator onlineOperator;
        [SerializeField] private bool isCircle;

        public bool IsCircle => isCircle;

        /// <summary>
        /// PlayerEntityの初期化を行う
        /// </summary>
        /// <param name="isWASD">WASD移動の時、true</param>
        /// <param name="isHostControl">Hostが操作権を持つ時、true</param>
        public void Setup(bool isWASD, bool isHostControl)
        {
            var ketSet = isWASD ? PlayerOperateKeySet.WASDMode : PlayerOperateKeySet.ArrowMode;
            if (isOnline)
            {
                //オンラインモード
                onlineOperator.SetKeySet(ketSet);
                onlineOperator.SetControlAuthority(isHostControl);
            }
            else
            {
                //オフラインモード
                offlineOperator.SetKeySet(ketSet);
                offlineOperator.SetCanControl(true);
            }
        }

        protected override void ChangeToOfflineInternal()
        {
            if (gameObject.GetComponent<PhotonRigidbody2DView>() != null)
            {
                Destroy(gameObject.GetComponent<PhotonRigidbody2DView>());
            }
            base.ChangeToOfflineInternal();
        }
    }
}