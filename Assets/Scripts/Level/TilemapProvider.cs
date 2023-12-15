using System.Collections.Generic;
using System.Linq;
using Hackathon2023Winter.Entity;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hackathon2023Winter.Level
{
    public class TilemapProvider : MonoBehaviour
    {
        [SerializeField] private Tilemap entityTilemap;
        [SerializeField] private Tilemap terrainTilemap;
        [SerializeField] private Transform entityParent;
        [SerializeField] private Tile signTile;
        [SerializeField] private GameObject terrainShader;
        [SerializeField] private GameObject BackGround;
        public Tilemap TerrainTilemap => terrainTilemap;
#if UNITY_EDITOR
        /// <summary>
        /// TilemapからEntityを生成する<br />
        /// エディタ上でのみ動作する
        /// </summary>
        public void CreateEntities()
        {
            var levelChipDict = Resources.Load<LevelChipDict>("LevelChipDict");
            var entityReferenceDict = Resources.Load<EntityReferenceDict>("EntityReferenceDict");

            var bounds = entityTilemap.cellBounds;
            entityTilemap.CompressBounds();
            var allTiles = entityTilemap.GetTilesBlock(bounds);
            float sX = (float)entityTilemap.size.x / 2;
            float sY = (float)entityTilemap.size.y / 2;

            //TilemapProviderのタイル以外の部分を作成する
            for (int x = 0; x < entityTilemap.size.x; x++)
            {
                for (int y = 0; y < entityTilemap.size.y; y++)
                {
                    TileBase tileBase = allTiles[x + y * entityTilemap.size.x];
                    if (tileBase == null) continue;
                    if (tileBase.Equals(signTile)) continue;
                    var entityType = levelChipDict.GetChipType(tileBase);
                    if (entityType.Equals(EntityType.TerrainChip)) continue;
                    //Entityを生成する
                    CreateEntity(entityType, x + 0.5f, y + 0.5f, entityReferenceDict);
                }
            }

            Undo.RecordObject(this, "Create Entities");

            //セーブする
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 全てのEntityを削除する<br />
        /// エディタ上でのみ動作する
        /// </summary>
        public void DeleteEntities()
        {
            foreach (var child in entityParent.OfType<Transform>().ToArray())
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            Undo.RecordObject(this, "Delete All Entities");

            //セーブする
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 指定した位置にEntityを生成する
        /// </summary>
        private void CreateEntity(EntityType type, float x, float y, EntityReferenceDict dict)
        {
            if (!dict.TryGetPrefab(type, out var prefab))
            {
                Debug.LogError($"TilemapProvider#CreateEntity: {type}が見つかりませんでした");
                return;
            }

            //同じ位置にオブジェクトが存在しない事を確認する
            foreach (Transform child in entityParent)
            {
                if (child.position.x == x && child.position.y == y) return;
            }


            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create Entity");
            obj.transform.parent = entityParent;
            obj.transform.localPosition = new Vector3(x, y, 0);
            var entity = obj.GetComponent<BaseEntity>();
            entity.SetType(type);
        }
#endif
        /// <summary>
        /// オフラインモードのプレハブに変更する
        /// </summary>
        public void SwitchToOffline()
        {
            if (gameObject.GetComponent<PhotonView>() != null)
            {
                Destroy(gameObject.GetComponent<PhotonView>());
            }
            if (gameObject.GetComponent<PhotonTransformView>() != null)
            {
                Destroy(gameObject.GetComponent<PhotonTransformView>());
            }

            if (terrainTilemap.gameObject.GetComponent<PhotonView>() != null)
            {
                Destroy(terrainTilemap.gameObject.GetComponent<PhotonView>());
            }
            if (terrainTilemap.gameObject.GetComponent<PhotonTransformView>() != null)
            {
                Destroy(terrainTilemap.gameObject.GetComponent<PhotonTransformView>());
            }
            if(terrainShader.GetComponent<PhotonView>() != null)
            {
                Destroy(terrainShader.GetComponent<PhotonView>());
            }
            if(terrainShader.GetComponent<PhotonTransformView>() != null)
            {
                Destroy(terrainShader.GetComponent<PhotonTransformView>());
            }
            if(BackGround.GetComponent<PhotonView>() != null)
            {
                Destroy(BackGround.GetComponent<PhotonView>());
            }
            if(BackGround.GetComponent<PhotonTransformView>() != null)
            {
                Destroy(BackGround.GetComponent<PhotonTransformView>());
            }

            foreach (Transform child in entityParent)
            {
                var entity = child.gameObject.GetComponent<BaseEntity>();
                if (entity != null)
                {
                    entity.SwitchToOffline();
                }
            }
        }

        /// <summary>
        /// TilemapProviderに含まれる全てのEntityを返す
        public List<BaseEntity> GetEntities()
        {
            return entityParent.GetComponentsInChildren<BaseEntity>().ToList();
        }
    }
}