using System;
using Photon.Pun;
using UniRx;

namespace Hackathon2023Winter.MainGame
{
    public class PunMainGameScreenReceiver:MonoBehaviourPun
    {
        private Subject<int> _onGoToObservable;
        public IObservable<int> OnGoToObservable => _onGoToObservable;
        
        private Subject<Unit> _onCreateLevelObservable;
        public IObservable<Unit> OnCreateLevelObservable => _onCreateLevelObservable;
        
        private void Awake()
        {
            _onGoToObservable = new Subject<int>();
            _onCreateLevelObservable = new Subject<Unit>();
        }

        private void OnDestroy()
        {
            _onGoToObservable?.Dispose();
            _onCreateLevelObservable?.Dispose();
        }

        public void GoTo(int id)
        {
            _onGoToObservable.OnNext(id);
        }
        
        public void ReceiveCreateLevel()
        {
            _onCreateLevelObservable.OnNext(Unit.Default);
        }
    }
}