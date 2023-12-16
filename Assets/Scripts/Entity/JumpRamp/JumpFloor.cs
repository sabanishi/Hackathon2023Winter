using System;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class JumpFloor : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField]private Shader shader;
        [SerializeField] private Shader barShader;
        [SerializeField]private SpriteRenderer spriteRenderer;
        [SerializeField]private SpriteRenderer barRenderer;
        [SerializeField]private float MaxShaderSpeed = 6.0f;
        public float Height;
        public IObservable<Vector3> StopObservable { get; private set; }

        private bool _isOnline;
        private bool _isOwner;
        private Material _material;
        private Material _barMaterial;
        
        private readonly int _objectType = Shader.PropertyToID("_ObjectType");
        private readonly int _velocityFlag = Shader.PropertyToID("velocity");
        
        private readonly int _barColorType = Shader.PropertyToID("_ColorType");

        private const int NormalColor = 2;
        private const int MouseOverColor = 3;

        private void Awake()
        {
            _material = new Material(shader);
            _material.SetInt(_objectType,NormalColor);
            spriteRenderer.material = _material;
            
            _barMaterial = new Material(barShader);
            _barMaterial.SetInt(_barColorType,NormalColor);
            barRenderer.material = _barMaterial;
        }

        public void SetIsOnline(bool isOnline)
        {
            _isOnline = isOnline;
        }

        public void Setup(bool isOwner)
        {
            _isOwner = isOwner;
            StopObservable = transform.ObserveEveryValueChanged(x => x.position)
                .Throttle(TimeSpan.FromSeconds(0.1f));
        }

        private void Update()
        {
            _material.SetFloat(_velocityFlag,rb.velocity.magnitude/MaxShaderSpeed);
            
            //オンラインモードで自分のオブジェクトでない場合は処理を行わない
            if (_isOnline && !_isOwner)
            {
                return;
            }

            //一定以上の高さにならないようにする
            if (transform.localPosition.y > Height - 0.5f)
            {
                transform.localPosition =
                    new Vector3(transform.localPosition.x, Height - 0.5f, transform.localPosition.z);
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        public void Run(float speed)
        {
            //速度ベクトルを設定
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = new Vector2(0, speed);
        }

        public void SetIsSimulateActive(bool isActive)
        {
            rb.simulated = isActive;
        }

        public void SetMouseTarget(bool isTarget)
        {
            _material.SetInt(_objectType, isTarget ? MouseOverColor : NormalColor);
            _barMaterial.SetInt(_barColorType,isTarget ? MouseOverColor : NormalColor);
        }
    }
}