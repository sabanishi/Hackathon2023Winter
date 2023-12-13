using System;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Sabanishi.Common;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Matching
{
    public class DisconnectPanel:MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button okButton;

        private void Awake()
        {
            okButton.SafeOnClickAsObservable().Subscribe(_=>Disconnect()).AddTo(gameObject);
        }

        private void OnDestroy()
        {
        }

        private void Disconnect()
        {
            panel.SetActive(false);
            //Photonから退出する
            RoomConnector.Instance.LeaveRoom();
            //タイトルに戻る
            ScreenTransition.Instance.Move(ScreenType.Title).Forget();
        }

        public void Show()
        {
            panel.SetActive(true);
        }
    }
}