using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Photon.Pun;
using Sabanishi.Common;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.StageSelect
{
    public class StageSelectScreenPresenter : MonoBehaviour
    {
        [SerializeField] private Button tmpButton;
        [SerializeField] private TMP_Text tmpText;

        private HostOperatePasser _hostOperatePasser;
        private const string HostOperatePasserPrefabName = "HostOperatePasser";
        private HostOperateReceiver _hostOperateReceiver;

        private bool _isTransitioning;

        public void Setup(bool isOnline, bool isHost)
        {
            _isTransitioning = false;
            if (isOnline)
            {
                //オンラインの時、ホストの操作を受け渡す機構を作成する
                if (isHost)
                {
                    _hostOperatePasser = PhotonNetwork
                        .Instantiate(HostOperatePasserPrefabName, Vector3.zero, Quaternion.identity)
                        .GetComponent<HostOperatePasser>();
                    _hostOperatePasser.transform.parent = transform;
                }
                else
                {
                    _hostOperateReceiver = new GameObject("HostOperateReceiver")
                        .AddComponent<HostOperateReceiver>();
                    _hostOperateReceiver.tag = TagName.HostOperateReceiver;
                    _hostOperateReceiver.Setup();
                    _hostOperateReceiver.transform.parent = transform;
                    _hostOperateReceiver.OnTmpButtonClicked.Subscribe(_ => GoToNextScreen()).AddTo(gameObject);
                }
            }

            //オフラインまたはホストの時、ボタンを押せるようにする
            tmpButton.interactable = !isOnline || isHost;
            if (isOnline)
            {
                tmpText.text = isHost ? "You are host." : "You are guest.";
            }
            else
            {
                tmpText.text = "You are offline.";
            }

            tmpButton.SafeOnClickAsObservable().Subscribe(_ => GoToNextScreen()).AddTo(gameObject);
        }

        private void GoToNextScreen()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            _hostOperatePasser?.OnTmpButtonClick();
            ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
        }

        public void Cleanup()
        {
            _hostOperateReceiver?.Cleanup();
            if (_hostOperatePasser != null)
            {
                PhotonNetwork.Destroy(_hostOperatePasser.gameObject);
            }
        }
    }
}