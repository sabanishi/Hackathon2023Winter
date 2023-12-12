using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// 各キーの状態を表すクラス
    /// </summary>
    public class KeyConditions
    {
        public bool IsLeft { get; set; }
        public bool IsRight { get; set; }
        public bool IsJump { get; set; }
        public bool IsLeftUp { get; set; }
        public bool IsRightUp { get; set; }
        public bool IsJumpUp { get; set; }
        public bool IsLeftDown { get; set; }
        public bool IsRightDown { get; set; }
        public bool IsJumpDown { get; set; }
        
        private PlayerOperateKeySet _keySet;
        
        public void SetKeySet(PlayerOperateKeySet keySet)
        {
            _keySet = keySet;
        }

        public void UpdateCondition()
        {
            IsLeftDown = Input.GetKeyDown(_keySet.Left);
            IsRightDown = Input.GetKeyDown(_keySet.Right);
            IsJumpDown = Input.GetKeyDown(_keySet.Up);
            IsLeft = Input.GetKey(_keySet.Left);
            IsRight = Input.GetKey(_keySet.Right);
            IsJump = Input.GetKey(_keySet.Up);
            IsLeftUp = Input.GetKeyUp(_keySet.Left);
            IsRightUp = Input.GetKeyUp(_keySet.Right);
            IsJumpUp = Input.GetKeyUp(_keySet.Up);
        }
    }
}