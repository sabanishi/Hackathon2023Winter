using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerOfflineOperator : MonoBehaviour
    {
        [SerializeField] private PlayerMover mover;

        private bool _isActive;
        private PlayerOperateKeySet _keySet;
        
        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }

        public void SetCanControl(bool canControl)
        {
            mover.SetControlAuthority(canControl);
        }

        public void SetKeySet(PlayerOperateKeySet keySet)
        {
            _keySet = keySet;
        }
        
        private void Update()
        {
            if (!_isActive) return;
            var keyConditions = new KeyConditions();
            keyConditions.SetKeySet(_keySet);
            keyConditions.UpdateCondition();
            mover.Move(keyConditions);
        }
        
    }
}