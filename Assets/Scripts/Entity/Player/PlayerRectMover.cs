using System;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerRectMover:PlayerMover
    {
        [SerializeField]private float moveSpeed = 5.0f;
        [SerializeField] private float rotateSpeed = 480f;
        [SerializeField]private Transform catchTransform;
        [SerializeField] private float minTVelocity = -20f;
        
        public override bool Move(KeyConditions keyCondition)
        {
            var isJump = false;
            if (!CanControl) return isJump;
            if (keyCondition.IsLeft)
            {
                rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
                //左回転する
                catchTransform.Rotate(0,0,rotateSpeed*Time.deltaTime);
            }
            else if(keyCondition.IsRight)
            {
                rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
                //右回転する
                catchTransform.Rotate(0,0,-rotateSpeed*Time.deltaTime);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            
            if (keyCondition.IsJumpDown)
            {
                if (!CheckIsGround()) return false;
                //上方向への衝撃を加える
                rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                isJump = true;
            }
            
            if(rb.velocity.y < minTVelocity)
            {
                rb.velocity = new Vector2(rb.velocity.x, minTVelocity);
            }

            return isJump;
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