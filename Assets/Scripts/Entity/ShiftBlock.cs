using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class ShiftBlock : BaseEntity
    {
        [SerializeField]　private SwitchEntity[] eventGenerators;
        [SerializeField] private Transform toPosTransform;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float speed;

        private const float Epsilon = 0.1f;
        private Vector3 _fromPos;
        private Vector3 _toPos;

        private CancellationTokenSource _cts;
        private Transform _transform;

        public void Setup()
        {
            _transform = transform;
            _fromPos = _transform.position;
            _toPos = toPosTransform.position;
            foreach (var generator in eventGenerators)
            {
                generator.Trigger.Skip(1).Subscribe(_ => CheckSwitch()).AddTo(gameObject);
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
                rb.velocity = (toPos - transform.position).normalized * speed;
                var velocity = rb.velocity;
                var cancellationToken = new CancellationTokenSource();
                var whenAnyCts =
                    CancellationTokenSource.CreateLinkedTokenSource(stopCts.Token, cancellationToken.Token);
                await UniTask.WhenAny(
                    UniTask.WaitUntil(() => Vector3.Distance(transform.position, toPos) < Epsilon,
                        cancellationToken: whenAnyCts.Token),
                    UniTask.Create(async () =>
                    {
                        while (true)
                        {
                            await UniTask.Yield();
                            whenAnyCts.Token.ThrowIfCancellationRequested();
                            //進んでいる方向に別のEntityが存在する場合
                            var hits = Physics2D.RaycastAll(transform.position, velocity, _transform.localScale.x / 2);
                            if (hits != null)
                            {
                                var isHit = false;
                                foreach (var hit in hits)
                                {
                                    if (hit.collider != null)
                                    {
                                        if (!hit.collider.gameObject.Equals(gameObject))
                                        {
                                            if (hit.collider.gameObject.GetComponent<BaseEntity>())
                                            {
                                                if (hit.collider.GetComponent<BaseEntity>().CheckIsCollide(velocity))
                                                {
                                                    //移動を一時的に停止する
                                                    Debug.Log(hit.collider.gameObject.name);
                                                    rb.velocity = Vector2.zero;
                                                    isHit = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (isHit) continue;
                            }

                            rb.velocity = velocity;
                        }
                    }));
                cancellationToken.Cancel();
                await UniTask.DelayFrame(1, cancellationToken: stopCts.Token);
                rb.velocity = Vector2.zero;
                transform.position = toPos;
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
    }
}