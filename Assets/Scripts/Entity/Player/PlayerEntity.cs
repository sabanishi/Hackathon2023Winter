using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// ユーザーが動かすことのできるEntityの基底クラス
    /// </summary>
    public abstract class PlayerEntity : BaseEntity, IShapable,IInfulencedShiftBlock
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerOfflineOperator offlineOperator;
        [SerializeField] private PlayerOnlineOperator onlineOperator;
        [SerializeField] private bool isCircle;
        [SerializeField] private KeyInputter keyInputter;

        public bool IsCircle => isCircle;
        
        private Vector2 _catchVelocity;

        /// <summary>
        /// PlayerEntityの初期化を行う
        /// </summary>
        /// <param name="isWASD">WASD移動の時、true</param>
        /// <param name="isHostControl">Hostが操作権を持つ時、true</param>
        public void Setup(bool isWASD, bool isHostControl)
        {
            var keySet = isWASD ? PlayerOperateKeySet.WASDMode : PlayerOperateKeySet.ArrowMode;
            if (isOnline)
            {
                //オンラインモード
                onlineOperator.SetActive(true);
                onlineOperator.SetControlAuthority(isHostControl);
                onlineOperator.SetMoverActive(true);
                keyInputter.SetKeySet(keySet);
                keyInputter.SetControlAuthority(isHostControl);
            }
            else
            {
                //オフラインモード
                offlineOperator.SetActive(true);
                offlineOperator.SetKeySet(keySet);
                offlineOperator.SetCanControl(true);
            }
        }

        protected override void ChangeToOfflineInternal()
        {
            if (gameObject.GetComponent<PUN2_RigidbodySync>() != null)
            {
                Destroy(gameObject.GetComponent<PUN2_RigidbodySync>());
            }

            if (keyInputter != null)
            {
                Destroy(keyInputter.gameObject);
            }

            base.ChangeToOfflineInternal();
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (isOnline && !IsOwner) return;
            //otherがIPushedを実装している時
            if (other.gameObject.TryGetComponent(out IPushed pushed))
            {
                pushed.ExitPush();
            }
        }

        public abstract float GetSize();

        public void SetCanInput(bool canInput)
        {
            if (isOnline)
            {
                onlineOperator.SetCanInput(canInput);
            }
            else
            {
                offlineOperator.SetCanInput(canInput);
            }
        }

        protected override void SetIsSimulateInternal(bool isSimulate)
        {
            rb.simulated = isSimulate;
            base.SetIsSimulateInternal(isSimulate);
        }
        
        public void AddVelocity(Vector2 velocity)
        {
            rb.velocity += new Vector2(velocity.x,0);
        }
    }
}