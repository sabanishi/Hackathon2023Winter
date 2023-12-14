using System;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameMoveStageReceiver:MonoBehaviour
    {
        private Subject<int> _receiverCanStageSelectSubject;
        public IObservable<int> ReceiverCanStageSelectObservable => _receiverCanStageSelectSubject;
        private Subject<Unit> _receiverCannotStageSelectSubject;
        public IObservable<Unit> ReceiverCannotStageSelectObservable => _receiverCannotStageSelectSubject;
        private Subject<int> _receiverGoToStageSubject;
        public IObservable<int> ReceiverGoToStageObservable => _receiverGoToStageSubject;

        private void Awake()
        {
            _receiverCannotStageSelectSubject = new Subject<Unit>();
            _receiverCanStageSelectSubject = new Subject<int>();
            _receiverGoToStageSubject = new Subject<int>();
        }

        private void OnDestroy()
        {
            _receiverCannotStageSelectSubject?.Dispose();
            _receiverCanStageSelectSubject?.Dispose();
            _receiverGoToStageSubject?.Dispose();
        }

        public void ReceiverCanStageSelect(int stageId)
        {
            _receiverCanStageSelectSubject.OnNext(stageId);
        }
        
        public void ReceiverCannotStageSelect()
        {
            _receiverCannotStageSelectSubject.OnNext(Unit.Default);
        }
        
        public void ReceiverGoToStage(int stageId)
        {
            _receiverGoToStageSubject.OnNext(stageId);
        }
    }
}