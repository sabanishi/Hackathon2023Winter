using System.Collections.Generic;
using Hackathon2023Winter.Entity;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// ステージ上のEntityを生成/管理するクラス
    /// </summary>
    public class LevelEntityManager : MonoBehaviour
    {
        [SerializeField] private LevelChipDict levelChipDict;
        [SerializeField] private Transform entityParent;

        private List<BaseEntity> entityList;

        public void Setup()
        {
            entityList = new List<BaseEntity>();
        }

        public void Cleanup()
        {
            entityList.Clear();
        }

        public void CreateLevel(TilemapProvider provider, bool isOnline)
        {
            var tilemap = provider.Tilemap;
            var bounds = tilemap.cellBounds;
            tilemap.CompressBounds();
            var allTiles = tilemap.GetTilesBlock(bounds);
            float sX = (float)tilemap.size.x / 2; //23
            float sY = (float)tilemap.size.y / 2; //13

            for (int x = 0; x < tilemap.size.x; x++)
            {
                for (int y = 0; y < tilemap.size.y; y++)
                {
                    TileBase tileBase = allTiles[x + y * tilemap.size.x];
                    if (tileBase == null) continue;
                    var entityType = levelChipDict.GetChipType(tileBase);
                    if(entityType.Equals(EntityType.NormalBlock))continue;
                    CreateEntity(entityType, isOnline, x - sX + 0.5f, y - sY + 0.5f);
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }

            var  obj = PhotonNetwork.Instantiate(provider.name, Vector3.zero, Quaternion.identity);
            obj.transform.parent = entityParent;
            obj.transform.localPosition = new Vector3(-sX, -sY, 0);
            Debug.Log(sX+" "+sY);
        }

        private void CreateEntity(EntityType type, bool isOnline, float x, float y)
        {
            var entity = EntityFactory.Instance.Create(type, isOnline);
            entityList.Add(entity);
            entity.transform.position = new Vector3(x, y, 0);
            entity.transform.parent = entityParent;
        }
    }
}