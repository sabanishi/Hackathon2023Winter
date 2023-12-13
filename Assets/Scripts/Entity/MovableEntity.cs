using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class MovableEntity : BaseEntity, IShapable
    {
        [SerializeField] private bool isCircle;
        public bool IsCircle => isCircle;

        protected override void ChangeToOfflineInternal()
        {
            if (gameObject.GetComponent<PUN2_RigidbodySync>() != null)
            {
                Destroy(gameObject.GetComponent<PUN2_RigidbodySync>());
            }

            base.ChangeToOfflineInternal();
        }
    }
}