using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sabanishi.Common;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class ShiftBlock : BaseEntity,ISwitchTarget
    {
        [SerializeField] private List<SwitchEntity> eventGenerators=new List<SwitchEntity>();
        [SerializeField] private Transform toPosTransform;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float speed;
        [SerializeField] private Transform parent;
        
        private List<ShiftBlockChildren> _children;
        private const float Epsilon = 0.1f;
        private Vector3 _fromPos;
        private Vector3 _toPos;

        private CancellationTokenSource _cts;
        private Transform _transform;

        private List<IInfulencedShiftBlock> _infulenced;

        private void Awake()
        {
            _children = new();
            foreach (Transform child in parent)
            {
                if (child.gameObject.TryGetComponent(typeof(ShiftBlockChildren), out var component))
                {
                    var shiftBlockChildren = (ShiftBlockChildren) component;
                    shiftBlockChildren.SetMaskActive(false);
                    _children.Add(shiftBlockChildren);
                }
            }

            _infulenced = new();
        }

        private void FixedUpdate()
        {
            foreach(var infulenced in _infulenced)
            {
                infulenced.AddVelocity(rb.velocity);
            }
        }

        public void Setup()
        {
            _transform = transform;
            _fromPos = _transform.position;
            _toPos = toPosTransform.position;
            foreach (var generator in eventGenerators)
            {
                generator.Trigger.Skip(1).ThrottleFrame(1).Subscribe(_ => CheckSwitch()).AddTo(gameObject);
            }
        }


        /// <summary>
        /// いずれかのスイッチのON/OFFが切り替わった時に呼ばれる関数
        /// </summary>
        private void CheckSwitch()
        {
            //全てのスイッチがONになっているか確認
            foreach (var generator in eventGenerators)
            {
                if (!generator.Trigger.Value)
                {
                    TurnOff();
                    return;
                }
            }

            //全てのスイッチがONになっていた場合
            TurnOn();
        }

        private void TurnOn()
        {
            GoTo(_toPos);
        }

        private void TurnOff()
        {
            GoTo(_fromPos);
        }

        /// <summary>
        /// 目的地まで移動する
        /// </summary>
        private void GoTo(Vector3 toPos)
        {
            if (_cts != null) _cts.Cancel();
            _cts = new CancellationTokenSource();
            var stopCts =
                CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, this.GetCancellationTokenOnDestroy());
            //toPosまでrigidbodyのvelocityを変化させながら移動する
            UniTask.Void(async () =>
            {
                Vector3 initPos = rb.position;
                var dir = (toPos - initPos).normalized;
                var velocity = dir * speed;
                rb.velocity = velocity;
                await UniTask.WaitUntil(() =>
                    {
                        Vector3 currPos = rb.position;
                        var flag = Vector3.Dot(dir, toPos - currPos)<Epsilon;
                        return flag;
                    },
                    cancellationToken:this.GetCancellationTokenOnDestroy());
                await UniTask.DelayFrame(1, cancellationToken: stopCts.Token);
                rb.velocity = Vector2.zero;
                rb.position = toPos;
            });
        }

        protected override void ChangeToOfflineInternal()
        {
            if (gameObject.GetComponent<PUN2_RigidbodySync>() != null)
            {
                Destroy(gameObject.GetComponent<PUN2_RigidbodySync>());
            }

            base.ChangeToOfflineInternal();
        }
        
        protected override void SetIsSimulateInternal(bool isSimulate)
        {
            rb.simulated = isSimulate;
            base.SetIsSimulateInternal(isSimulate);
        }

        public void Enter()
        {
            if (_children.IsNullOrEmpty()) return;
            foreach (var child in _children)
            {
                child.SetMaskActive(true);
            }
        }

        public void Exit()
        {
            if (_children.IsNullOrEmpty()) return;
            foreach (var child in _children)
            {
                child.SetMaskActive(false);
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out IInfulencedShiftBlock infulenced))
            {
                //上に乗っていたら加える
                if (other.transform.position.y > transform.position.y)
                {
                    _infulenced.Add(infulenced);
                }
            }
        }
        
        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out IInfulencedShiftBlock infulenced))
            {
                _infulenced.Remove(infulenced);
            }
        }

        public void PassSwitchReference(SwitchEntity switchEntity)
        {
            foreach (var generator in eventGenerators)
            {
                if (generator == switchEntity)
                {
                    return;
                }
            }
            eventGenerators.Add(switchEntity);
#if UNITY_EDITOR
            Undo.RecordObject(this, "Set Switch Target");
            EditorUtility.SetDirty(this);
#endif
        }
    }
}