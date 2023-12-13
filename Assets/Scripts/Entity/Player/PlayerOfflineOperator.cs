using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PlayerOfflineOperator : MonoBehaviour
    {
        [SerializeField] private PlayerMover mover;

        private bool _isActive;
        private PlayerOperateKeySet _keySet;
        private bool _canInput;

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
            if (!_canInput) return;
            var keyConditions = new KeyConditions();
            keyConditions.SetKeySet(_keySet);
            keyConditions.UpdateCondition();
            mover.Move(keyConditions);
        }
        
        public void SetCanInput(bool canInput)
        {
            _canInput = canInput;
        }
    }
}