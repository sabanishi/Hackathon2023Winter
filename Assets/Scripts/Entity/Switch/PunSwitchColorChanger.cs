using System;
using Photon.Pun;
using UniRx;

namespace Hackathon2023Winter.Entity
{
    public class PunSwitchColorChanger:MonoBehaviourPun
    {
        private Subject<bool> _onColorChangeSubject;
        public IObservable<bool> OnColorChangeObservable => _onColorChangeSubject;

        public void Setup()
        {
            _onColorChangeSubject = new Subject<bool>();
        }
        
        public void Cleanup()
        {
            _onColorChangeSubject?.Dispose();
        }
        
        public void ChangeColor(bool isOn)
        {
            photonView.RPC(nameof(RPC_ChangeColor), RpcTarget.All, isOn);
        }
        
        [PunRPC]
        private void RPC_ChangeColor(bool isOn)
        {
            _onColorChangeSubject.OnNext(isOn);
        }
    }
}