using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class MovableEntity : BaseEntity, IShapable
    {
        [SerializeField] private bool isCircle;
        public bool IsCircle => isCircle;
    }
}