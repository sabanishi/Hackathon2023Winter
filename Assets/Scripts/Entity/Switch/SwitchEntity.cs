using Photon.Pun;
using Sabanishi.Common;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Hackathon2023Winter.Entity
{
    public class SwitchEntity : BaseEntity, IEventGenerator
    {
        [SerializeField] private bool isCircle;
        [SerializeField] private bool isPermanent;
        [SerializeField] private Transform child;
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private BaseEntity[] targets;


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
            _defaultScale = child.localScale;

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => OnMouseEnterOrExit(true));
            eventTrigger.triggers.Add(entry);

            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((data) => OnMouseEnterOrExit(false));
            eventTrigger.triggers.Add(exit);
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

        /// <summary>
        /// Triggerの値が変化した時に走る処理
        /// </summary>
        private void OnChangeTrigger(bool isOn)
        {
            //Scaleの処理
            child.localScale = isOn
                ? new Vector3(_defaultScale.x, _defaultScale.y * TriggeredScale, _defaultScale.z)
                : _defaultScale;
            var scaleY = child.localScale.y;
            child.localPosition = new Vector3(0, -0.5f + scaleY / 2, 0);

            if (isOn)
            {
                SoundManager.PlaySE(SE_Enum.SWITCH, true);
            }
        }

        protected override void ChangeToOfflineInternal()
        {
            if (child.TryGetComponent(typeof(PhotonView), out var view))
            {
                Destroy(view);
            }

            if (child.TryGetComponent(typeof(PhotonTransformView), out var transformView))
            {
                Destroy(transformView);
            }

            base.ChangeToOfflineInternal();
        }

        private void OnMouseEnterOrExit(bool isEnter)
        {
            if (targets.IsNullOrEmpty()) return;
            foreach (var target in targets)
            {
                if (target == null) continue;
                if (target is ISwitchTarget switchTarget)
                {
                    if (isEnter)
                    {
                        switchTarget.Enter();
                    }
                    else
                    {
                        switchTarget.Exit();
                    }
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor拡張用
        /// </summary>
        public void SetSwitchTarget()
        {
            if (targets.IsNullOrEmpty()) return;
            foreach (var target in targets)
            {
                if (target == null) continue;
                if (target is ISwitchTarget switchTarget)
                {
                    switchTarget.PassSwitchReference(this);
                }else
                {
                    Debug.LogError(
                        $"ISwitchTargetを実装していないEntityがSwitchEntityのTargetに設定されています: my_name={gameObject.name}, targetName:{target.name}");
                }
            }
        }
#endif
    }
}