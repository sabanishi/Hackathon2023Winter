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
        [SerializeField] private KeyInputter keyInputter;

        public bool IsCircle => isCircle;

        /// <summary>
        /// PlayerEntityの初期化を行う
        /// </summary>
        /// <param name="isWASD">WASD移動の時、true</param>
        /// <param name="isHostControl">Hostが操作権を持つ時、true</param>
        public void Setup(bool isWASD, bool isHostControl)
        {
            var keySet = isWASD ? PlayerOperateKeySet.WASDMode : PlayerOperateKeySet.ArrowMode;
            if (isOnline)
            {
                //オンラインモード
                onlineOperator.SetActive(true);
                onlineOperator.SetControlAuthority(isHostControl);
                onlineOperator.SetMoverActive(true);
                keyInputter.SetKeySet(keySet);
                keyInputter.SetControlAuthority(isHostControl);
                
            }
            else
            {
                //オフラインモード
                offlineOperator.SetActive(true);
                offlineOperator.SetKeySet(keySet);
                offlineOperator.SetCanControl(true);
            }
        }

        protected override void ChangeToOfflineInternal()
        {
            if (gameObject.GetComponent<PUN2_RigidbodySync>() != null)
            {
                Destroy(gameObject.GetComponent<PUN2_RigidbodySync>());
            }

            if (keyInputter != null)
            {
                Destroy(keyInputter.gameObject);
            }
            
            base.ChangeToOfflineInternal();
        }
    }
}