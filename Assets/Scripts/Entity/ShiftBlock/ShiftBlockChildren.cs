using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class ShiftBlockChildren:MonoBehaviour
    {
        [SerializeField] private GameObject maskObject;
        
        public void SetMaskActive(bool isActive)
        {
            maskObject.SetActive(isActive);
        }
    }
}