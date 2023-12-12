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
    }
}