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
        /// PlayerOperatorから毎フレーム呼び出される
        /// </summary>
        /// <returns>ジャンプできる体制だったらtrue</returns>
        public abstract bool Move(KeyConditions keyCondition);
    }
}