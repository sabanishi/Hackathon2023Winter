using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class SwitchEntity : BaseEntity, IEventGenerator
    {
        [SerializeField] private bool isCircle;
        [SerializeField] private bool isPermanent;

        private ReactiveProperty<bool> _trigger;

        public IReadOnlyReactiveProperty<bool> Trigger => _trigger;

        private void Awake()
        {
            _trigger = new ReactiveProperty<bool>(false);
        }

        private void OnDestroy()
        {
            _trigger.Dispose();
        }

        private void Update()
        {
            if (isOnline && !IsOwner) return;
            CheckTrigger();
        }

        private void CheckTrigger()
        {
            //自身の上にあるEntityをRayを飛ばすことにより取得する
            var hit = Physics2D.Raycast(transform.position, Vector2.up, 0.2f);
            if (hit.collider != null)
            {
                //IShapableが乗っている時、Triggerをtrueにする
                if (hit.collider.gameObject.GetComponent<IShapable>() != null)
                {
                    var shape = hit.collider.gameObject.GetComponent<IShapable>();
                    if (shape.IsCircle == isCircle)
                    {
                        _trigger.Value = true;
                        return;
                    }
                }
            }

            if (isPermanent) return;
            _trigger.Value = false;
        }
    }
}