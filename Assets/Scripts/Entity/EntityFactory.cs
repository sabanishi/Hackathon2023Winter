using System;
using Photon.Pun;
using Sabanishi.Common;
using Sabanishin.Common;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class EntityFactory:SingletonMonoBehaviour<EntityFactory>
    {
        [SerializeField]private EntityReferenceDictionary entityReferenceDictionary;

        /// <summary>
        /// EntityTypeを指定してEntityを生成する
        /// </summary>
        /// <param name="type">指定するEntityType</param>
        /// <param name="isOnline">オンラインモードかどうか</param>
        public BaseEntity Create(EntityType type, bool isOnline)
        {
            var dict = entityReferenceDictionary?.GetDict();
            if (dict.IsNullOrEmpty())
            {
                Debug.LogError($"EntityFactory#Create: EntityReferenceDictionaryが設定されていません");
                return null;
            }
            
            if(dict.TryGetValue(type,out var reference))
            {
                BaseEntity entity;
                if (isOnline && reference.OnlinePrefab!=null)
                {
                    var prefabName = reference.OnlinePrefab.name;
                    var obj =PhotonNetwork.Instantiate(prefabName,Vector3.zero, Quaternion.identity);
                    var position = Vector3.zero;
                    obj.transform.localPosition = position;
                    entity = obj.GetComponent<BaseEntity>();
                    if (entity == null)
                    {
                        Debug.LogError($"EntityFactory#Create: {type}にBaseEntityがアタッチされていません");
                        return null;
                    }
                }
                else
                {
                    entity = Instantiate(reference.OfflinePrefab);
                }
                entity.SetType(type);
                return entity;
            }
            
            Debug.LogError($"EntityFactory#Create: {type}が見つかりませんでした");
            return null;
        }
    }

    [Serializable]
    public class EntityReferenceDictionary : InspectorDictionary<EntityType,EntityReference,EntityReferencePair>
    {
    }
    
    [Serializable]
    public class EntityReferencePair : InspectorDictionaryPair<EntityType,EntityReference>
    {
        public EntityReferencePair(EntityType key, EntityReference value) : base(key, value)
        {
        }
    }

    [Serializable]
    public struct EntityReference
    {
        [SerializeField] private BaseEntity offlinePrefab;
        [SerializeField] private BaseEntity onlinePrefab;
        public BaseEntity OfflinePrefab => offlinePrefab;
        public BaseEntity OnlinePrefab => onlinePrefab;
    }
}