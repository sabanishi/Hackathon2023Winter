using System;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameCommandReceiver:MonoBehaviour
    {
        private Subject<MainGameCommandType> _receiverMakeRequestSubject;
        private Subject<MainGameCommandType> _receiverTakeRequestSubject;
        public IObservable<MainGameCommandType> ReceiverMakeRequestObservable => _receiverMakeRequestSubject;
        public IObservable<MainGameCommandType> ReceiverTakeRequestObservable => _receiverTakeRequestSubject;

        private void Awake()
        {
            _receiverMakeRequestSubject = new Subject<MainGameCommandType>();
            _receiverTakeRequestSubject = new Subject<MainGameCommandType>();
        }

        private void OnDestroy()
        {
            _receiverMakeRequestSubject?.Dispose();
            _receiverTakeRequestSubject?.Dispose();
        }

        public void ReceiverMakeRequest(MainGameCommandType type)
        {
            _receiverMakeRequestSubject.OnNext(type);
        }

        public void ReceiverTakeRequest(MainGameCommandType type)
        {
            _receiverTakeRequestSubject.OnNext(type);
        }
    }
}