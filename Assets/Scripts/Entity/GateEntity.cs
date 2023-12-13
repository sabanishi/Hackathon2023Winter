using System;
using Sabanishi.Common;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class GateEntity:BaseEntity
    {
        [SerializeField] private int stageId;
        
        private Subject<int> _goToSubject;

        /**ゴールに触れた時に、そのオブジェクトを引き渡すObservable*/
        public IObservable<int> OnGoToObservable => _goToSubject;

        private void Awake()
        {
            _goToSubject = new Subject<int>();
        }

        private void OnDestroy()
        {
            _goToSubject?.Dispose();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            //オンラインかつHostでない時、処理を行わない
            if (isOnline && !IsOwner) return;

            if (!other.CompareTag(TagName.Player)) return;
            //Playerがゴールに触れた時の処理
            _goToSubject.OnNext(stageId);
        }
    }
}