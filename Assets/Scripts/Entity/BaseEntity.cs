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

            SetIsOnline(false);
            ChangeToOfflineInternal();
        }

        protected virtual void ChangeToOfflineInternal()
        {
        }

        /// <summary>
        /// rotate方向にオブジェクトが存在するかを返す
        /// </summary>
        public bool CheckIsCollide(Vector2 rotate)
        {
            var catchTransform = transform;
            var hit = Physics2D.Raycast(catchTransform.position, rotate, catchTransform.localScale.x * 7 / 4);
            return hit.collider != null;
        }

        /// <summary>
        /// 物理演算を有効化するかを切り替える
        /// </summary>
        public void SetIsSimulate(bool isSimulate)
        {
            SetIsSimulateInternal(isSimulate);
        }

        protected virtual void SetIsSimulateInternal(bool isSimulate)
        {
        }
    }
}