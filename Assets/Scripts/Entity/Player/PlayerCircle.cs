using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerCircle : PlayerEntity
    {
        [SerializeField]private Transform pos0;
        [SerializeField]private Transform pos1;
        
        public override float GetSize()
        {
            return Vector3.Distance(pos0.position, pos1.position);
        }
    }
}