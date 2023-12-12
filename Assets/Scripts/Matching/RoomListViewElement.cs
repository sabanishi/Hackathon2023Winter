using System;
using Photon.Realtime;
using Sabanishi.Common;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Matching
{
    /// <summary>
    /// RoomListViewの最小要素 <br />
    /// 各部屋の情報を表示する
    /// </summary>
    public class RoomListViewElement : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text playerCountText;

        private RoomInfo _roomInfo;
        private Subject<RoomInfo> _onClickSubject;
        public IObservable<RoomInfo> OnClickAsObservable => _onClickSubject;

        public void Setup()
        {
            _onClickSubject = new Subject<RoomInfo>();
            button.SafeOnClickAsObservable().Subscribe(_ =>
            {
                if (_roomInfo == null) return;
                _onClickSubject.OnNext(_roomInfo);
            }).AddTo(gameObject);
            Hide();
        }

        public void Cleanup()
        {
            _onClickSubject.Dispose();
        }

        public void Show(RoomInfo roomInfo)
        {
            _roomInfo = roomInfo;
            nameText.text = roomInfo.Name;
            playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _roomInfo = null;
            gameObject.SetActive(false);
        }
    }
}