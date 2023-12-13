using System;
using Sabanishi.Common;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class GoalEntity : BaseEntity
    {
        private Subject<GameObject> _clearSubject;

        /**ゴールに触れた時に、そのオブジェクトを引き渡すObservable*/
        public IObservable<GameObject> OnClearObservable => _clearSubject;

        private void Awake()
        {
            _clearSubject = new Subject<GameObject>();
        }

        private void OnDestroy()
        {
            _clearSubject?.Dispose();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //オンラインかつHostでない時、処理を行わない
            if (isOnline && !IsOwner) return;

            if (!other.CompareTag(TagName.Player)) return;
            //Playerがゴールに触れた時の処理
            _clearSubject.OnNext(other.gameObject);
        }
    }
}