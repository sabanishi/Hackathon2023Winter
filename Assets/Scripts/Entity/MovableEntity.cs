using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class MovableEntity : BaseEntity, IShapable, IPushed,IInfulencedShiftBlock
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private bool isCircle;
        [SerializeField] private Shader myShader;
        [SerializeField]private SpriteRenderer spriteRenderer;
        [SerializeField] private float ShaderMaxSpeed = 1.0f;
        public bool IsCircle => isCircle;

        private Material _material;

        private readonly int _objectType = Shader.PropertyToID("_ObjectType");
        private readonly int _velocityFlag = Shader.PropertyToID("velocity");
        

        private void Awake()
        {
            _material = new Material(myShader);
            _material.SetInt(_objectType, isCircle ? 0 : 1);
            spriteRenderer.material = _material;
        }

        private void Update()
        {
            _material.SetFloat(_velocityFlag,rb.velocity.magnitude / ShaderMaxSpeed);
        }

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
            //rb.velocity = Vector2.zero;
        }
        
        private void OnCollisionExit2D(Collision2D other)
        {
            //otherがIPushedを実装している時
            if (other.gameObject.TryGetComponent(out IPushed pushed))
            {
                pushed.ExitPush();
            }
        }
        
        protected override void SetIsSimulateInternal(bool isSimulate)
        {
            rb.simulated = isSimulate;
            base.SetIsSimulateInternal(isSimulate);
        }
        
        public void AddVelocity(Vector2 velocity)
        {
            Debug.Log("AddVelocity");
            //rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }
}