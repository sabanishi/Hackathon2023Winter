using System;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class JumpFloor:MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        public float Height;
        public IObservable<Vector3> StopObservable { get; private set; }

        private bool _isOnline;
        private bool _isOwner;
        
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
            //オンラインモードで自分のオブジェクトでない場合は処理を行わない
            if (_isOnline && !_isOwner)
            {
                return;
            }
            //一定以上の高さにならないようにする
            if (transform.localPosition.y > Height-0.5f)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, Height-0.5f, transform.localPosition.z);
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log("ダイナミック");
            }
        }

        public void Run(float speed)
        {
            //速度ベクトルを設定
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = new Vector2(0, speed);
        }
    }
}