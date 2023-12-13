using UnityEngine;
using Photon.Pun;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// Rigidbodyの同期を行う
    /// </summary>
    public class PUN2_RigidbodySync : MonoBehaviourPun, IPunObservable
    {
        private Transform _transform;
        private Rigidbody2D _r;
        private Vector3 _latestPos;
        private Quaternion _latestRot;
        private Vector2 _velocity;
        private float _angularVelocity;

        private bool _valuesReceived = false;

        private void Start()
        {
            _transform = transform;
            _r = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (photonView.IsMine || !_valuesReceived) return;
            _transform.position = Vector3.Lerp(_transform.position, _latestPos, Time.deltaTime * 5);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, _latestRot, Time.deltaTime * 5);
            _r.velocity = _velocity;
            _r.angularVelocity = _angularVelocity;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_transform.position);
                stream.SendNext(_transform.rotation);
                stream.SendNext(_r.velocity);
                stream.SendNext(_r.angularVelocity);
            }
            else
            {
                _latestPos = (Vector3)stream.ReceiveNext();
                _latestRot = (Quaternion)stream.ReceiveNext();
                _velocity = (Vector2)stream.ReceiveNext();
                _angularVelocity = (float)stream.ReceiveNext();

                _valuesReceived = true;
            }
        }
    }
}