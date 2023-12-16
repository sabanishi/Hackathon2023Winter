using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// ShiftBlockの影響を受けるオブジェクト
    /// </summary>
    public interface IInfulencedShiftBlock
    {
        public void AddVelocity(Vector2 velocity);
    }
}