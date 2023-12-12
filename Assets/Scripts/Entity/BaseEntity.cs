using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// Entityオブジェクトの基底クラス
    /// </summary>
    public class BaseEntity : MonoBehaviour
    {
        [SerializeField] protected EntityType type;
        [SerializeField] protected bool isOnline = true;

        protected bool IsOwner;

        public void SetIsOwner(bool isOwner)
        {
            IsOwner = isOwner;
        }
        
        public void SetType(EntityType type)
        {
            this.type = type;
        }

        private void SetIsOnline(bool isOnline)
        {
            this.isOnline = isOnline;
        }

        /// <summary>
        /// OfflineモードのEntityに変更する
        /// </summary>
        public void SwitchToOffline()
        {
            //PhotonViewの削除
            if (gameObject.GetComponent<PhotonView>() != null)
            {
                Destroy(gameObject.GetComponent<PhotonView>());
            }

            //PhotonTransformViewの削除
            if (gameObject.GetComponent<PhotonTransformView>() != null)
            {
                Destroy(gameObject.GetComponent<PhotonTransformView>());
            }

            if (gameObject.GetComponent<PhotonComponent>() != null)
            {
                Destroy(gameObject.GetComponent<PhotonComponent>());
                if (GetComponent<Rigidbody2D>() != null)
                {
                    GetComponent<Rigidbody2D>().isKinematic = false;
                }
            }

            SetIsOnline(false);
            ChangeToOfflineInternal();
        }

        protected virtual void ChangeToOfflineInternal()
        {
        }
    }
}