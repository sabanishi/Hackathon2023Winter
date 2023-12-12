using Hackathon2023Winter.Entity;
using Sabanishi.Common;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// Entityを描画するShaderに情報を渡すためのクラス
    /// </summary>
    public class EntityShaderBridge:MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private PunEntityShaderInfoPasser passerPrefab;
        [SerializeField]private PunEntityShaderInfoReceiver receiverPrefab;
        [SerializeField]private Camera renderCamera;
        
        private readonly int _sCirclePosition = Shader.PropertyToID("circlePosition");
        private readonly int _sCircleInfo = Shader.PropertyToID("circleInfo");
        private readonly int _sQuadPosition = Shader.PropertyToID("quadPosition");
        private readonly int _sQuadInfo = Shader.PropertyToID("quadInfo");
        
        private PunEntityShaderInfoPasser _passer;
        private PunEntityShaderInfoReceiver _receiver;
        
        private Vector2 _circleScale;
        private Vector2 _rectScale;
        
        private GameObject _playerCircle;
        private GameObject _playerRect;

        public void Setup(bool isOnline,bool isHost)
        {
            if (isOnline)
            {
                if (isHost)
                {
                    _passer = Photon.Pun.PhotonNetwork.Instantiate(passerPrefab.name, Vector3.zero, Quaternion.identity)
                        .GetComponent<PunEntityShaderInfoPasser>();
                    _passer.transform.parent = transform;
                    _passer.Setup();
                }
                else
                {
                    _receiver = Photon.Pun.PhotonNetwork.Instantiate(receiverPrefab.name, Vector3.zero, Quaternion.identity)
                        .GetComponent<PunEntityShaderInfoReceiver>();
                    _receiver.transform.parent = transform;
                    _receiver.Setup();
                    _receiver.CircleScale.Subscribe(x=>_circleScale = x).AddTo(gameObject);
                    _receiver.RectScale.Subscribe(x=>_rectScale = x).AddTo(gameObject);
                }
            }
        }

        public void Cleanup()
        {
            if (_passer != null)
            {
                Destroy(_passer.gameObject);
            }
            if (_receiver != null)
            {
                _receiver.Cleanup();
                Destroy(_receiver.gameObject);
            }
        }
        
        public void SetPlayerScale(Vector2 circleScale,Vector2 rectScale)
        {
            _circleScale = circleScale;
            _rectScale = rectScale;
            if (_passer != null)
            {
                _passer.SetScales(circleScale,rectScale);
            }
        }
        
        public void SetPlayerObject(GameObject circle,GameObject rect)
        {
            _playerCircle = circle;
            _playerRect = rect;
        }

        public void CalcShaderInfo()
        {
            if (_playerCircle == null || _playerRect == null)
            {
                SearchPlayers();
                if (_playerCircle == null || _playerRect == null) return;
            }
            (float x,float y,float r) circleInfo = CalcInfo(_playerCircle.transform);
            (float x,float y,float r) rectInfo = CalcInfo(_playerRect.transform);
            
            // シェーダに情報を渡す
            material.SetVector(_sCirclePosition, new Vector4(circleInfo.x,circleInfo.y,0,0));
            material.SetVector(_sCircleInfo, new Vector4(_circleScale.x,_circleScale.y,circleInfo.r,0));
            material.SetVector(_sQuadPosition, new Vector4(rectInfo.x,rectInfo.y,0,0));
            material.SetVector(_sQuadInfo, new Vector4(_rectScale.x,_rectScale.y,rectInfo.r,0));
        }

        private (float x,float y,float rotate) CalcInfo(Transform target)
        {
            var pos = renderCamera.WorldToViewportPoint(target.position);
            var rotate = -Mathf.Deg2Rad * target.rotation.eulerAngles.z;
            return (pos.x,pos.y,rotate);
        }

        private void SearchPlayers()
        {
            var players = GameObject.FindGameObjectsWithTag(TagName.Player);
            foreach (var obj in players)
            {
                if(obj.GetComponent<PlayerEntity>()==null)continue;
                if (obj.GetComponent<PlayerEntity>().IsCircle)
                {
                    _playerCircle = obj;
                }
                else
                {
                    _playerRect = obj;
                }
            }
        }
    }
}