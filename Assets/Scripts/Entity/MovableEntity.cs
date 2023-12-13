using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class MovableEntity : BaseEntity, IShapable, IPushed
    {
        [SerializeField] private Rigidbody2D rb;
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
        
        public void ExitPush()
        {
            rb.velocity = Vector2.zero;
        }
        
        private void OnCollisionExit2D(Collision2D other)
        {
            //otherがIPushedを実装している時
            if (other.gameObject.TryGetComponent(out IPushed pushed))
            {
                pushed.ExitPush();
            }
        }
    }
}