using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// ユーザーがPlayerEntityを操作する際のキーの設定
    /// </summary>
    public class PlayerOperateKeySet
    {
        public static readonly PlayerOperateKeySet WASDMode = new PlayerOperateKeySet(
            up: KeyCode.W,
            down: KeyCode.S,
            left: KeyCode.A,
            right: KeyCode.D
        );

        public static readonly PlayerOperateKeySet ArrowMode = new PlayerOperateKeySet(
            up: KeyCode.UpArrow,
            down: KeyCode.DownArrow,
            left: KeyCode.LeftArrow,
            right: KeyCode.RightArrow
        );

        public KeyCode Up { get; }
        public KeyCode Down { get; }
        public KeyCode Left { get; }
        public KeyCode Right { get; }

        public PlayerOperateKeySet(KeyCode up, KeyCode down, KeyCode left, KeyCode right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
    }
}