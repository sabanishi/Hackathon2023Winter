using System;
using Photon.Pun;
using Photon.Realtime;
using UniRx;

namespace Hackathon2023Winter.Matching
{
    public class PunWaitMemberScreen:MonoBehaviourPunCallbacks
    {
        private Subject<Unit> _onJoinedRoomSubject;
        public IObservable<Unit> OnJoinedRoomSubject => _onJoinedRoomSubject;

        public void Setup()
        {
            _onJoinedRoomSubject = new();
        }

        public void Cleanup()
        {
            _onJoinedRoomSubject.Dispose();
        }
        
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            _onJoinedRoomSubject.OnNext(Unit.Default);
        }
    }
}