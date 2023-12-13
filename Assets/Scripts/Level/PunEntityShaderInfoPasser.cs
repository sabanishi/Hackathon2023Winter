using Photon.Pun;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// Photon下でHostのEntityの情報をClientに渡すためのクラス
    /// </summary>
    public class PunEntityShaderInfoPasser : MonoBehaviourPun, IPunObservable
    {
        private Vector2 _circleScale;
        private Vector2 _rectScale;

        private bool _isReceived;

        public void SetScales(Vector2 circleScale, Vector2 rectScale)
        {
            _circleScale = circleScale;
            _rectScale = rectScale;
        }

        public void Setup()
        {
        }

        private void Update()
        {
            if (_isReceived)
            {
                var receivers = GameObject.FindGameObjectsWithTag(TagName.EntityShaderInfoReceiver);
                if (receivers.IsNullOrEmpty()) return;
                foreach (var obj in receivers)
                {
                    if (obj.TryGetComponent(typeof(PunEntityShaderInfoReceiver), out var receiverComponent))
                    {
                        var receiver = (PunEntityShaderInfoReceiver)receiverComponent;
                        receiver?.SetScales(_circleScale, _rectScale);
                        Debug.Log(receiver);
                        if (receiver != null)
                        {
                            _isReceived = false;
                        }
                    }
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_circleScale);
                stream.SendNext(_rectScale);
            }
            else
            {
                _circleScale = (Vector2)stream.ReceiveNext();
                _rectScale = (Vector2)stream.ReceiveNext();
                _isReceived = true;
            }
        }
    }
}