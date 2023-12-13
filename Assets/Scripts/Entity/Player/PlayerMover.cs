using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public abstract class PlayerMover : MonoBehaviour
    {
        [SerializeField] protected Rigidbody2D rb;

        [Header("ジャンプ時に与える衝撃")] [SerializeField]
        protected float jumpPower = 5f;

        protected bool CanControl;
        
        public void SetControlAuthority(bool canControl)
        {
            CanControl = canControl;
        }

        /// <summary>
        /// PlayerOperatorんから毎フレーム呼び出される
        /// </summary>
        public abstract void Move(KeyConditions keyCondition);
    }
}