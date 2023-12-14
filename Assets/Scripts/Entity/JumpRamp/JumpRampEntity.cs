using Cysharp.Threading.Tasks;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class JumpRampEntity : BaseEntity
    {
        [SerializeField] private GameObject bar;
        [SerializeField] private JumpFloor floor;
        [SerializeField] private float height;
        [SerializeField]　private SwitchEntity[] eventGenerators;
        [Header("ジャンプ台の速度")] [SerializeField] private float speed;

        [Header("ジャンプ台が停止してから使用可能になるまでの時間")] [SerializeField]
        private float waitTime = 0f;

        private bool _isRunning;

        /// <summary>
        /// エディタ拡張用
        /// </summary>
        public void SetBarHeight()
        {
            var barTransform = bar.transform;
            barTransform.localScale = new Vector3(barTransform.localScale.x, height, barTransform.localScale.z);
            barTransform.localPosition = new Vector3(0, height / 2 - 0.5f, 0);
            floor.Height = height;
        }

        private void Awake()
        {
            floor.SetIsOnline(isOnline);
        }

        public void Setup()
        {
            floor.Setup(IsOwner);
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
            //既に起動していたら何もしない
            if (_isRunning) return;

            //全てのスイッチがONになっているか確認
            foreach (var generator in eventGenerators)
            {
                if (!generator.Trigger.Value) return;
            }

            //全てのスイッチがONになっていたらジャンプ台を起動
            Run();
        }

        /// <summary>
        /// ジャンプ台を起動させる関数
        /// </summary>
        private void Run()
        {
            if (_isRunning) return;
            UniTask.Void(async () =>
            {
                _isRunning = true;
                //ジャンプ台を起動
                floor.Run(speed);
                //1フレーム待機(Floorが動き出すのを待つため)
                await UniTask.DelayFrame(1, cancellationToken: this.GetCancellationTokenOnDestroy());
                //床のY座標が止まるまで待機
                await floor.StopObservable.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy(),
                    useFirstValue: true);
                //waitTime秒待機
                await UniTask.Delay((int)(waitTime * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
                _isRunning = false;
            });
        }

        protected override void ChangeToOfflineInternal()
        {
            base.ChangeToOfflineInternal();
            var floorGameObject = floor.gameObject;
            if (floorGameObject.GetComponent<PhotonView>() != null)
            {
                Destroy(floorGameObject.GetComponent<PhotonView>());
            }

            if (floorGameObject.GetComponent<PUN2_RigidbodySync>() != null)
            {
                Destroy(floorGameObject.GetComponent<PUN2_RigidbodySync>());
            }
        }
        
        protected override void SetIsSimulateInternal(bool isSimulate)
        {
            floor.SetIsSimulateActive(isSimulate);
            base.SetIsSimulateInternal(isSimulate);
        }
    }
}