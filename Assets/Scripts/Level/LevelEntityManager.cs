using System.Collections.Generic;
using Hackathon2023Winter.Entity;
using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// TilemapProviderに含まれるEntityを管理するクラス
    /// </summary>
    public class LevelEntityManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera renderCamera;
        [SerializeField] private TilemapProvider tilemapProviderPrefab;
        [SerializeField] private Material material;

        private List<BaseEntity> _entities;
        
        private readonly int _sCirclePosition = Shader.PropertyToID("circlePosition");
        private readonly int _sCircleInfo = Shader.PropertyToID("circleInfo");
        private readonly int _sQuadPosition = Shader.PropertyToID("quadPosition");
        private readonly int _sQuadInfo = Shader.PropertyToID("quadInfo");

        public void Setup()
        {
            _entities = new List<BaseEntity>();
        }

        public void Cleanup()
        {
            _entities.Clear();
        }

        public void CreateLevel(bool isOnline)
        {
            var tilemap = tilemapProviderPrefab.TerrainTilemap;
            tilemap.CompressBounds();
            float sX = (float)tilemap.size.x / 2;
            float sY = (float)tilemap.size.y / 2;

            TilemapProvider provider;
            //Tileを生成する
            if (isOnline)
            {
                provider = PhotonNetwork.Instantiate(tilemapProviderPrefab.name, Vector3.zero, Quaternion.identity)
                    .GetComponent<TilemapProvider>();
            }
            else
            {
                provider = Instantiate(tilemapProviderPrefab);
                provider.SwitchToOffline();
            }

            provider.transform.parent = transform;
            provider.transform.localPosition = new Vector3(sX, sY, 0) * (-1.0f);
            provider.transform.localScale = Vector3.one;

            //TilemapProvider内のEntityを全て取得する
            _entities = provider.GetEntities();

            //初期化処理
            foreach (var entity in _entities)
            {
                entity.SetIsOwner(true);
                switch (entity)
                {
                    case PlayerEntity playerEntity:
                        bool isCircle = playerEntity.IsCircle;
                        if (isOnline)
                        {
                            playerEntity.Setup(true, isCircle);
                        }
                        else
                        {
                            playerEntity.Setup(isCircle, true);
                        }
                        break;
                    case JumpRampEntity jumpRamp:
                        jumpRamp.Setup();
                        break;
                    case ShiftBlock shiftBlock:
                        shiftBlock.Setup();
                        break;
                }
            }
        }

        private void Update()
        {
            //PassPlayerInfoToShader();
        }

        private void PassPlayerInfoToShader()
        {
            var info = new Vector4[4];
            foreach (var entity in _entities)
            {
                if (entity is PlayerEntity playerEntity)
                {
                    var pos = renderCamera.WorldToViewportPoint(entity.transform.position);
                    var scale = playerEntity.GetSize()/2;
                    var cameraScale = renderCamera.ViewportToWorldPoint(Vector2.one)
                                - renderCamera.ViewportToWorldPoint(Vector2.zero);
                    var width = scale / cameraScale.x;
                    var height = scale / cameraScale.y;
                    var rotate = -Mathf.Deg2Rad * playerEntity.transform.rotation.eulerAngles.z;
                    
                    if (playerEntity.IsCircle)
                    {
                        info[0] = new Vector4(pos.x, pos.y, 0, 0);
                        info[1] = new Vector4(width, height, rotate, 0);
                    }
                    else
                    {
                        info[2] = new Vector4(pos.x, pos.y, 0, 0);
                        info[3] = new Vector4(width, height, rotate, 0);
                    }
                }
                
                // シェーダに情報を渡す
                material.SetVector(_sCirclePosition, info[0]);
                material.SetVector(_sCircleInfo, info[1]);
                material.SetVector(_sQuadPosition, info[2]);
                material.SetVector(_sQuadInfo, info[3]);
                
                //Debug.Log("circle pos: " + info[0]);
                // Debug.Log("circle info: " + info[1].x);
                // Debug.Log("quad pos: " + info[2]);
                // Debug.Log("quad info: " + info[3]);
            }
        }
    }
}