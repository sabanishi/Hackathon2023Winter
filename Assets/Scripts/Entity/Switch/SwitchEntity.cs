using Photon.Pun;
using Sabanishi.Common;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hackathon2023Winter.Entity
{
    public class SwitchEntity : BaseEntity, IEventGenerator
    {
        [SerializeField] private bool isCircle;
        [SerializeField] private bool isPermanent;
        [SerializeField] private Transform child;
        [SerializeField] private BaseEntity[] targets;
        
        //踏まれた時のScaleの倍率
        private const float TriggeredScale = 0.4f;
        
        private ReactiveProperty<bool> _trigger;
        public IReadOnlyReactiveProperty<bool> Trigger => _trigger;
        private Vector3 _defaultScale;
        private bool _isFinish;

        private void Awake()
        {
            _trigger = new ReactiveProperty<bool>(false);
            _trigger.Skip(1).Subscribe(OnChangeTrigger).AddTo(gameObject);
            _defaultScale = child.localScale;
            _isFinish = false;
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
            var hits = Physics2D.RaycastAll(transform.position, Vector2.up, 0.2f);
            if (hits.IsNullOrEmpty()) return;
            foreach (var hit in hits)
            {
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
            }

            if (_isFinish) return;
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

        public void OnMouseEnterOrExit(bool isEnter)
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

        public void SendFireEvent()
        {
            if (isPermanent)
            {
                _isFinish = true;
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
            
            Undo.RecordObject(this, "Set Switch Target");
            EditorUtility.SetDirty(this);
        }
#endif
    }
}