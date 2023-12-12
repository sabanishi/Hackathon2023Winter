using System;
using Photon.Pun;
using UniRx;

namespace Hackathon2023Winter.Matching
{
    /// <summary>
    /// 部屋に入った時のコールバックを受け取るクラス
    /// </summary>
    public class MatchingCallback : MonoBehaviourPunCallbacks
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

        public override void OnJoinedRoom()
        {
            _onJoinedRoomSubject?.OnNext(Unit.Default);
        }
    }
}