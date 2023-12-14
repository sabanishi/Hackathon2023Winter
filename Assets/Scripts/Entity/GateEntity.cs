using System;
using Sabanishi.Common;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class GateEntity:BaseEntity
    {
        [SerializeField] private int stageId;
        
        private Subject<(int,bool)> _onEnterSubject;
        private Subject<(int,bool)> _onExitSubject;
        public IObservable<(int stageId, bool isCircle)> OnEnterObservable => _onEnterSubject;
        public IObservable<(int stageId, bool isCircle)> OnExitObservable => _onExitSubject;

        private void Awake()
        {
            _onEnterSubject = new Subject<(int,bool)>();
            _onExitSubject = new Subject<(int,bool)>();
        }

        private void OnDestroy()
        {
            _onEnterSubject?.Dispose();
            _onExitSubject?.Dispose();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            //オンラインかつHostでない時、処理を行わない
            if (isOnline && !IsOwner) return;

            if (!other.CompareTag(TagName.Player)) return;
            
            var isCircle = other.gameObject.GetComponent<PlayerEntity>()?.IsCircle ?? true;
            _onEnterSubject.OnNext((stageId,isCircle));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            //オンラインかつHostでない時、処理を行わない
            if (isOnline && !IsOwner) return;

            if (!other.CompareTag(TagName.Player)) return;
            var isCircle = other.gameObject.GetComponent<PlayerEntity>()?.IsCircle ?? true;
            _onExitSubject.OnNext((stageId,isCircle));
        }
    }
}