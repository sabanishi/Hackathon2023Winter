using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [Header("ジャンプ時に与える衝撃")] [SerializeField]
        private float jumpPower = 5f;

        [Header("動き始める時に与える衝撃")] [SerializeField]
        private float firstMoveImpulse = 5f;
        
        [SerializeField] private float A = 2.0f;
        [SerializeField] private float B = 5.0f;
        [SerializeField] private float D = 3.0f;
        [SerializeField] private float E = 1.0f;
        [SerializeField] private float F = 1.0f;
        
        private PlayerOperateKeySet _keySet;
        private bool _canControl;
        
        //キャッシュ用の一時変数
        private float _moveTimer;
        private bool _isMoving;
        private float _stopTimer;
        private float _currentSpeed;
        
        
        public void SetKeySet(PlayerOperateKeySet keySet)
        {
            _keySet = keySet;
        }
        
        public void SetControlAuthority(bool canControl)
        {
            _canControl = canControl;
        }

        private void Update()
        {
            if(!_canControl) return;
            
            if (Input.GetKeyDown(_keySet.Left))
            {
                rb.AddForce(new Vector2(-firstMoveImpulse,0),ForceMode2D.Impulse);
            }else if(Input.GetKeyDown(_keySet.Right))
            {
                rb.AddForce(new Vector2(firstMoveImpulse,0),ForceMode2D.Impulse);
            }
            if (Input.GetKey(_keySet.Left))
            {
            
                //左に回転する
                _moveTimer+= Time.deltaTime;
                float x = CalcMoveSpeed(_moveTimer/E);
                rb.AddTorque(x);
                _isMoving = true;
                _stopTimer = 0;
            }else if (Input.GetKey(_keySet.Right))
            {
                //右に回転する
                _moveTimer+= Time.deltaTime;
                float x = CalcMoveSpeed(_moveTimer/E);
                rb.AddTorque(-x);
                _isMoving = true;
                _stopTimer = 0;
            
            }
            else
            {
                //回転を止める
                if (_stopTimer == 0)
                {
                    _currentSpeed = rb.velocity.x;
                }
                _stopTimer += Time.deltaTime;
                float y = CalcStopSpeed(_stopTimer/F)*_currentSpeed;
                if (rb.velocity.x > 0)
                {
                    rb.velocity = new Vector2(y, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(y, rb.velocity.y);
                }
                _moveTimer = 0;
            }
            if (Input.GetKeyDown(_keySet.Up))
            {
                //上方向への衝撃を加える
                rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            }
        }

        private float CalcMoveSpeed(float input)
        {
            float x = Mathf.Clamp01(input);
            float y = (A-B) * Mathf.Pow(1 - x, D) + B;
            return y;
        }

        private float CalcStopSpeed(float input)
        {
            float x = Mathf.Clamp01(input);
            float y = Mathf.Pow(1-x, 2);
            return y;
        }
    }
}