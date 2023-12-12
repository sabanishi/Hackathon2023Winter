using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// Entityの基底クラス
    /// </summary>
    public class BaseEntity:MonoBehaviour
    {
        public EntityType Type { get; private set; }

        public void SetType(EntityType type)
        {
            Type = type;
        }
    }
}