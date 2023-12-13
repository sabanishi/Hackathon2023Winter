using System;
using Photon.Pun;
using UniRx;

namespace Hackathon2023Winter.Matching
{
    /// <summary>
    /// 部屋から誰かが抜けたことを検知するクラス
    /// </summary>
    public class DisconnectDetector:MonoBehaviourPunCallbacks
    {
        private Subject<Unit> _disconnectSubject;
        public IObservable<Unit> OnDisconnectedAsObservable => _disconnectSubject;
        
        private void Awake()
        {
            _disconnectSubject = new Subject<Unit>();
        }

        private void OnDestroy()
        {
            _disconnectSubject.Dispose();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            _disconnectSubject.OnNext(Unit.Default);
        }
    }
}