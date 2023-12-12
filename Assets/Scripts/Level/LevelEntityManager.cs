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
        [SerializeField]private Camera mainCamera;
        [SerializeField] private TilemapProvider tilemapProviderPrefab;

        private List<BaseEntity> _entities;

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
                }
            }
        }

        private void Update()
        {
            PassPlayerInfoToShader();
        }

        private void PassPlayerInfoToShader()
        {
            var info = new Vector4[4];
            foreach (var entity in _entities)
            {
                if (entity is PlayerEntity playerEntity)
                {
                    var pos = mainCamera.WorldToViewportPoint(entity.transform.position);
                    var scale = playerEntity.GetSize()/2;
                    var cameraScale = mainCamera.WorldToViewportPoint(new Vector3(1,1,0))
                                - mainCamera.WorldToViewportPoint(new Vector3(0,0,0));
                    var width = scale / cameraScale.x;
                    var height = scale / cameraScale.y;
                    var rotate = playerEntity.transform.rotation.eulerAngles.z;
                    
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
                
                //TODO:Shaderに情報を渡す
                //Info[0]と[1]が円の情報、[2]と[3]が四角形の情報
                //[0]と[2]が中心座標、[1]と[3]が幅と高さ、回転角
            }
        }
    }
}