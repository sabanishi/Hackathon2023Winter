using System;
using Photon.Pun;
using UniRx;

namespace Hackathon2023Winter.MainGame
{
    public class PunMainGameScreenReceiver:MonoBehaviourPun
    {
        private Subject<int> _onGoToObservable;
        public IObservable<int> OnGoToObservable => _onGoToObservable;
        
        private void Awake()
        {
            _onGoToObservable = new Subject<int>();
        }

        private void OnDestroy()
        {
            _onGoToObservable?.Dispose();
        }

        public void GoTo(int id)
        {
            _onGoToObservable.OnNext(id);
        }

    }
}