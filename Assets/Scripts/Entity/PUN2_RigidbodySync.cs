using Hackathon2023Winter.Calc;
using UnityEngine;
using Photon.Pun;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// Rigidbodyの同期を行う
    /// </summary>
    public class PUN2_RigidbodySync : MonoBehaviourPun, IPunObservable
    {
        [SerializeField]private float warpDistance = 30f;
        [Header("座標をビットパッキングする際にかける係数")][SerializeField]private int posCoefficient = 1000;
        private Transform _transform;
        private Rigidbody2D _r;
        private Vector2 _latestPos;
        private float _latestRot;
        private Vector2 _velocity;
        private float _angularVelocity;
        private Vector2 _scale;

        private bool _valuesReceived = false;

        private void Start()
        {
            _transform = transform;
            _r = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (photonView.IsMine || !_valuesReceived) return;
            //positionのズレが大き過ぎたらワープする
            var nowPos = _transform.localPosition;
            var pos_2d = new Vector2(nowPos.x, nowPos.y);
            if (Vector2.Distance(pos_2d, _latestPos) > warpDistance)
            {
                _transform.localPosition = new Vector3(_latestPos.x, _latestPos.y, 0);
            }
            else
            {
                //補間
                var coPos = Vector2.Lerp(pos_2d, _latestPos, Time.deltaTime * 20);
                _transform.localPosition = new Vector3(coPos.x, coPos.y, 0);
            }
            _transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(_transform.eulerAngles.z, _latestRot, Time.deltaTime * 20));
            _transform.localScale = new Vector3(_scale.x,_scale.y, 1f);
            _r.velocity = _velocity;
            _r.angularVelocity = _angularVelocity;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (!photonView.IsMine) return;
                var pos = _transform.localPosition;
                var rotate = _transform.eulerAngles.z;
                var scale = _transform.localScale;
                var velocity = _r.velocity;
                var angularVelocity = _r.angularVelocity;

                var coefficientPos = pos*posCoefficient;
                coefficientPos += Vector3.one * 10000;
                
                var intPos = new Vector2Int(Clamp(coefficientPos.x,0,60000),
                    Clamp(coefficientPos.y,0,60000));
                var intRotate = Clamp(rotate*100,0,36000);
                var intScale = new Vector2Int(Clamp(scale.x*100,0,4000),Clamp(scale.y*100,0,4000));
                var intVelocity = new Vector2Int(Clamp((velocity.x+100)*100,0,20000),Clamp((velocity.y+100)*100,0,20000));
                var intAngularVelocity = Clamp((angularVelocity+100)*100,0,72000);
                
                long pack1 = BitPackingCalc.SerializePack_1(intPos,intVelocity);
                long pack2 = BitPackingCalc.SerializePack_2(intRotate, intAngularVelocity, intScale);
                
                stream.SendNext(pack1);
                stream.SendNext(pack2);
            }
            else
            {
                long pack1 = (long)stream.ReceiveNext();
                long pack2 = (long)stream.ReceiveNext();
                
                var (intPos,intVelocity) = BitPackingCalc.DeserializePack_1(pack1);
                var (intRotate, intAngularVelocity,intScale) = BitPackingCalc.DeserializePack_2(pack2);

                var coefficientPos = intPos - Vector2.one * 10000;
                coefficientPos /= ((float)posCoefficient*10);
                
                _latestPos = new Vector2(coefficientPos.x,coefficientPos.y);
                _latestRot = intRotate/100f;
                _velocity = new Vector2((intVelocity.x/100f)-100,(intVelocity.y/100f)-100);
                _angularVelocity = (intAngularVelocity/100f)-100;
                _scale = new Vector2(intScale.x/100f,intScale.y/100f);

                _valuesReceived = true;
            }
        }
        
        private int Clamp(float value, int min, int max)
        {
            var near= Mathf.FloorToInt(value);
            return Mathf.Clamp(near, min, max);
        }
    }
}