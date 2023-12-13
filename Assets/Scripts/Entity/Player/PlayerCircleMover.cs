using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerCircleMover:PlayerMover
    {
        [SerializeField]private float moveSpeed = 5.0f;
        [SerializeField]private Transform catchTransform;
        public override void Move(KeyConditions keyCondition)
        {
            if (!CanControl) return;

            if (keyCondition.IsLeft)
            {
                rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
                //左回転する
                catchTransform.Rotate(0,0,moveSpeed*Time.deltaTime);
            }
            else if(keyCondition.IsRight)
            {
                rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
                //右回転する
                catchTransform.Rotate(0,0,-moveSpeed*Time.deltaTime);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            
            if (keyCondition.IsJumpDown)
            {
                if (!CheckIsGround()) return;
                //上方向への衝撃を加える
                rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 自身が地面に接地しているかどうかを返す
        /// </summary>
        private bool CheckIsGround()
        {
            //下方向にRayを飛ばして地面に接地しているかどうかを判定する
            var hits = Physics2D.RaycastAll(catchTransform.position, Vector2.down, catchTransform.localScale.x);
            if (hits.IsNullOrEmpty()) return false;
            foreach (var hit in hits)
            {
                if (!hit.collider.gameObject.Equals(gameObject)) return true;
            }
            return false;
        }
    }
}