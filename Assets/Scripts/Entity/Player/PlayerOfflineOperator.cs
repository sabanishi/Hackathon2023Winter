using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerOfflineOperator : MonoBehaviour
    {
        [SerializeField] private PlayerMover mover;

        public void SetCanControl(bool canControl)
        {
            mover.SetControlAuthority(canControl);
        }

        public void SetKeySet(PlayerOperateKeySet keySet)
        {
            mover.SetKeySet(keySet);
        }
        
    }
}