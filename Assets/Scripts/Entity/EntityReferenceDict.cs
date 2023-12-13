using System;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    [CreateAssetMenu(fileName = "EntityReferenceDict", menuName = "ScriptableObject/EntityReferenceDict")]
    public class EntityReferenceDict : ScriptableObject
    {
        [SerializeField] private EntityReferenceDictionary entityReferenceDictionary;

        /// <summary>
        /// EntityTypeを指定してEntityを生成する
        /// </summary>
        /// <param name="type">指定するEntityType</param>
        /// <param name="isOnline">オンラインモードかどうか</param>
        public BaseEntity Create(EntityType type)
        {
            var dict = entityReferenceDictionary?.GetDict();
            if (dict.IsNullOrEmpty())
            {
                Debug.LogError($"EntityFactory#Create: EntityReferenceDictionaryが設定されていません");
                return null;
            }

            if (dict.TryGetValue(type, out var reference))
            {
                BaseEntity entity = Instantiate(reference.OfflinePrefab);
                entity.SetType(type);
                return entity;
            }

            Debug.LogError($"EntityFactory#Create: {type}が見つかりませんでした");
            return null;
        }

        /// <summary>
        /// EntityTypeを指定してPrefabを取得する
        /// </summary>
        public bool TryGetPrefab(EntityType type, out BaseEntity prefab)
        {
            var dict = entityReferenceDictionary?.GetDict();
            if (dict.IsNullOrEmpty())
            {
                Debug.LogError($"EntityFactory#Create: EntityReferenceDictionaryが設定されていません");
                prefab = null;
                return false;
            }

            if (dict.TryGetValue(type, out var reference))
            {
                prefab = reference.OfflinePrefab;
                return true;
            }

            Debug.LogError($"EntityFactory#Create: {type}が見つかりませんでした");
            prefab = null;
            return false;
        }
    }

    [Serializable]
    public class EntityReferenceDictionary : InspectorDictionary<EntityType, EntityReference, EntityReferencePair>
    {
    }

    [Serializable]
    public class EntityReferencePair : InspectorDictionaryPair<EntityType, EntityReference>
    {
        public EntityReferencePair(EntityType key, EntityReference value) : base(key, value)
        {
        }
    }

    [Serializable]
    public struct EntityReference
    {
        [SerializeField] private BaseEntity offlinePrefab;
        public BaseEntity OfflinePrefab => offlinePrefab;
    }
}