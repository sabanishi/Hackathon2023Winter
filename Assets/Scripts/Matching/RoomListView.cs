using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Matching
{
    public class RoomListView : MonoBehaviourPunCallbacks
    {
        private const int MaxElementCount = 10;
        [SerializeField] private RoomListViewElement elementPrefab;
        [SerializeField] private GameObject content;

        private List<RoomListViewElement> _elements;
        private RoomList _roomList;

        private Subject<RoomInfo> _joinRoomSubject;

        /**Elementがクリックされた際の処理*/
        public IObservable<RoomInfo> OnJoinRoomAsObservable => _joinRoomSubject;

        public void Setup()
        {
            _roomList = new RoomList();
            _elements = new List<RoomListViewElement>();
            _roomList.Setup();
            _joinRoomSubject = new();

            for (int i = 0; i < MaxElementCount; i++)
            {
                var element = Instantiate(elementPrefab, content.transform);
                element.Setup();
                element.OnClickAsObservable.Subscribe(x =>
                {
                    SoundManager.PlaySE(SE_Enum.CLICK);
                    _joinRoomSubject.OnNext(x);
                }).AddTo(gameObject);
                _elements.Add(element);
            }
        }

        public void Cleanup()
        {
            foreach (var element in _elements)
            {
                element.Cleanup();
                Destroy(element.gameObject);
            }

            _joinRoomSubject.Dispose();
            _roomList.Cleanup();
        }

        public override void OnRoomListUpdate(List<RoomInfo> changedRoomList)
        {
            _roomList.Update(changedRoomList);

            //存在する部屋の数だけ要素を表示する
            var i = 0;
            foreach (var roomInfo in _roomList)
            {
                if (i >= MaxElementCount) break;
                _elements[i].Show(roomInfo);
                i++;
            }

            //残りの要素は非表示にする
            for (; i < MaxElementCount; i++)
            {
                _elements[i].Hide();
            }
        }
    }
}