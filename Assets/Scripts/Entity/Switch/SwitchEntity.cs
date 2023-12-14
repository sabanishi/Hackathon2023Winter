using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hackathon2023Winter.Entity
{
    public class SwitchEntity : BaseEntity, IEventGenerator
    {
        [SerializeField] private bool isCircle;
        [SerializeField] private bool isPermanent;
        [SerializeField] private PunSwitchColorChanger punSwitchColorChanger;
        [SerializeField] private Tilemap wires;
        [SerializeField] private Transform child;

        private ReactiveProperty<bool> _trigger;

        private static readonly Color CircleColor = Color.red;
        private static readonly Color RectColor = Color.green;
        private static readonly Color DefaultColor = Color.white;
        //踏まれた時のScaleの倍率
        private const float TriggeredScale = 0.4f;

        public IReadOnlyReactiveProperty<bool> Trigger => _trigger;
        private Vector3 _defaultScale;

        private void Awake()
        {
            _trigger = new ReactiveProperty<bool>(false);
            _trigger.Skip(1).Subscribe(OnChangeTrigger).AddTo(gameObject);
            if (punSwitchColorChanger != null)
            {
                punSwitchColorChanger.Setup();
                punSwitchColorChanger.OnColorChangeObservable.Subscribe(OnChangeWireColorDeal).AddTo(gameObject);
            }
            _defaultScale = child.localScale;
        }

        private void OnDestroy()
        {
            _trigger.Dispose();
            punSwitchColorChanger?.Cleanup();
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

        /// <summary>
        /// Triggerの値が変化した時に走る処理
        /// </summary>
        private void OnChangeTrigger(bool isOn)
        {
            //Scaleの処理
            child.localScale = isOn
                ? new Vector3(_defaultScale.x, _defaultScale.y*TriggeredScale, _defaultScale.z)
                : _defaultScale;
            var scaleY = child.localScale.y;
            child.localPosition = new Vector3(0,-0.5f+scaleY/2, 0);
                
            
            //導線の処理
            if (isOnline)
            {
                punSwitchColorChanger.ChangeColor(isOn);
            }
            else
            {
                OnChangeWireColorDeal(isOn);
            }
        }
        

        private void OnChangeWireColorDeal(bool isOn)
        {
            if (isOn)
            {
                wires.color = isCircle ? CircleColor : RectColor;
            }
            else
            {
                wires.color = DefaultColor;
            }
        }

        protected override void ChangeToOfflineInternal()
        {
            if (gameObject.TryGetComponent(typeof(PunSwitchColorChanger), out var component))
            {
                var colorChanger = (PunSwitchColorChanger) component;
                Destroy(colorChanger);
            }
            if(child.TryGetComponent(typeof(PhotonView),out var view))
            {
                Destroy(view);
            }
            if(child.TryGetComponent(typeof(PhotonTransformView),out var transformView))
            {
                Destroy(transformView);
            }
            
            base.ChangeToOfflineInternal();
        }
    }
}