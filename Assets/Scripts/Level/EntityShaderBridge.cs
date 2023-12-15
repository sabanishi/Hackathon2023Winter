using System;
using System.Collections.Generic;
using Hackathon2023Winter.Entity;
using Photon.Pun;
using Sabanishi.Common;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// Entityを描画するShaderに情報を渡すためのクラス
    /// </summary>
    public class EntityShaderBridge : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private PunEntityShaderInfoPasser passerPrefab;
        [SerializeField] private PunEntityShaderInfoReceiver receiverPrefab;
        [SerializeField] private Camera renderCamera;
        [SerializeField]private RenderTexture renderTexture_0;
        [SerializeField]private RenderTexture renderTexture_1;

        private readonly int _sCirclePosition = Shader.PropertyToID("circlePosition");
        private readonly int _sCircleInfo = Shader.PropertyToID("circleInfo");
        private readonly int _sQuadPosition = Shader.PropertyToID("quadPosition");
        private readonly int _sQuadInfo = Shader.PropertyToID("quadInfo");
        
        private readonly int _warpInfo = Shader.PropertyToID("warpInfo");
        private readonly int _isVanishing = Shader.PropertyToID("isVanishing");
        private readonly int _vanishTime = Shader.PropertyToID("vanishTime");
        private readonly int _warpSeed = Shader.PropertyToID("warpSeed");
        private readonly int _warpNum = Shader.PropertyToID("warpNum");

        private PunEntityShaderInfoPasser _passer;
        private PunEntityShaderInfoReceiver _receiver;

        private Vector2 _circleScale;
        private Vector2 _rectScale;

        private GameObject _playerCircle;
        private GameObject _playerRect;
        private List<(GameObject obj,bool isGoal,Vector2 seed)> _warpObjects;
        private GoalEntity _goalEntity;
        
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int SubTexture = Shader.PropertyToID("_SubTexture");
        public void Setup(bool isOnline, bool isHost)
        {
            material.SetTexture(MainTex, renderTexture_0);
            material.SetTexture(SubTexture,renderTexture_1 );
            if (isOnline)
            {
                if (isHost)
                {
                    _passer = PhotonNetwork.Instantiate(passerPrefab.name, Vector3.zero, Quaternion.identity)
                        .GetComponent<PunEntityShaderInfoPasser>();
                    _passer.transform.parent = transform;
                    _passer.Setup();
                }
                else
                {
                    _receiver = PhotonNetwork.Instantiate(receiverPrefab.name, Vector3.zero, Quaternion.identity)
                        .GetComponent<PunEntityShaderInfoReceiver>();
                    _receiver.transform.parent = transform;
                    _receiver.Setup();
                    _receiver.CircleScale.Subscribe(x =>
                    {
                        _circleScale = x;
                    }).AddTo(gameObject);
                    _receiver.RectScale.Subscribe(x => _rectScale = x).AddTo(gameObject);
                }
            }
        }

        public void Cleanup()
        {
            if (_passer != null)
            {
                PhotonNetwork.Destroy(_passer.gameObject);
            }

            if (_receiver != null)
            {
                _receiver.Cleanup();
                PhotonNetwork.Destroy(_receiver.gameObject);
            }
        }

        public void SetPlayerScale(Vector2 circleScale, Vector2 rectScale)
        {
            _circleScale = circleScale;
            _rectScale = rectScale;
            if (_passer != null)
            {
                _passer.SetScales(circleScale, rectScale);
            }
        }

        public void SetEntityObject(GameObject circle, GameObject rect,List<(GameObject,bool,Vector2)> warpObjects)
        {
            _playerCircle = circle;
            _playerRect = rect;
            _warpObjects = warpObjects;
        }

        public void CalcShaderInfo()
        {
            if (_playerCircle == null || _playerRect == null || _warpObjects == null)
            {
                SearchEntities();
                if (_playerCircle == null || _playerRect == null)
                {
                    material.SetVector(_sCirclePosition, new Vector4(-1, -1, 0, 0));
                    material.SetVector(_sCircleInfo, new Vector4(0.01f, 0.01f, 0, 0));
                    material.SetVector(_sQuadPosition, new Vector4(-1, -1, 0, 0));
                    material.SetVector(_sQuadInfo, new Vector4(0.01f, 0.01f, 0, 0));
                    material.SetVector(_warpInfo,new Vector4(0,0,0,0));
                    return;
                }
            }

            (float x, float y, float r) circleInfo = CalcInfo(_playerCircle.transform);
            (float x, float y, float r) rectInfo = CalcInfo(_playerRect.transform);

            // シェーダに情報を渡す
            material.SetVector(_sCirclePosition, new Vector4(circleInfo.x, circleInfo.y, 0, 0));
            material.SetVector(_sCircleInfo, new Vector4(_circleScale.x, _circleScale.y, circleInfo.r, 0));
            material.SetVector(_sQuadPosition, new Vector4(rectInfo.x, rectInfo.y, 0, 0));
            material.SetVector(_sQuadInfo, new Vector4(_rectScale.x, _rectScale.y, rectInfo.r, 0));
            
            //Goal/Gateの情報を渡す
            if (_warpObjects.IsNullOrEmpty()) return;
            var count = _warpObjects.Count;
            var warpInfos = new Vector4[count];
            var isVanishings = new float[count];
            var vanishTimes = new float[count];
            var warpSeeds = new Vector4[count];

            var deleteList = new List<int>();
            for(int i=0;i<count;i++)
            {
                var tuple = _warpObjects[i];
                if (tuple.obj == null)
                {
                    deleteList.Add(i);
                    Debug.Log("null");
                    continue;
                }
                (float x, float y, float r) goalInfo = CalcInfo(tuple.obj.transform);
                var colorType = tuple.isGoal ? 0 : 1;
               warpInfos[i] = new Vector4(goalInfo.x,goalInfo.y,colorType,0);
               isVanishings[i] = Math.Abs(tuple.obj.transform.localScale.x - 1) < 0.01f ? 0 : 1;
               vanishTimes[i] = 1- tuple.obj.transform.localScale.x;
               warpSeeds[i] = tuple.seed;
            }

            if (!deleteList.IsNullOrEmpty())
            {
                //deleteListを数字が大きい順にソートする
                deleteList.Sort((a, b) => b - a);
                foreach (var i in deleteList)
                {
                    _warpObjects.RemoveAt(i);
                }
                count -= deleteList.Count;
            }
            
            material.SetFloat(_warpNum,count);
            material.SetVectorArray(_warpInfo,warpInfos);
            material.SetFloatArray(_isVanishing,isVanishings);
            material.SetFloatArray(_vanishTime,vanishTimes);
            material.SetVectorArray(_warpSeed,warpSeeds);
        }

        private (float x, float y, float rotate) CalcInfo(Transform target)
        {
            var pos = renderCamera.WorldToViewportPoint(target.position);
            var rotate = -Mathf.Deg2Rad * target.rotation.eulerAngles.z;
            return (pos.x, pos.y, rotate);
        }

        private void SearchEntities()
        {
            var players = GameObject.FindGameObjectsWithTag(TagName.Player);
            foreach (var obj in players)
            {
                if (obj.GetComponent<PlayerEntity>() == null) continue;
                if (obj.GetComponent<PlayerEntity>().IsCircle)
                {
                    _playerCircle = obj;
                }
                else
                {
                    _playerRect = obj;
                }
            }

            _warpObjects = new();
            var goals = GameObject.FindGameObjectsWithTag(TagName.GoalEntity);
            foreach (var obj in goals)
            {
                var seed = new Vector2(Random.value * 100,Random.value * 100);
                _warpObjects.Add((obj,true,seed));
            }

            var gates = GameObject.FindGameObjectsWithTag(TagName.GateEntity);
            foreach (var obj in gates)
            {
                var seed = new Vector2(Random.value * 100,Random.value * 100);
                _warpObjects.Add((obj,false,seed));
            }
        }
    }
}