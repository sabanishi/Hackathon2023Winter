using System;
using Sabanishi.Common;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    [CreateAssetMenu(fileName = "TilemapProviderDict", menuName = "ScriptableObject/TilemapProviderDict")]
    public class TilemapProviderDict: ScriptableObject
    {
        [SerializeField] private TilemapProviderDictionary tilemapProviderDict;
        
        /// <summary>
        /// intを指定してPrefabを取得する
        /// </summary>
        public bool TryGetPrefab(int key, out TilemapProvider prefab)
        {
            var dict = tilemapProviderDict?.GetDict();
            if (dict.IsNullOrEmpty())
            {
                Debug.LogError($"EntityFactory#Create: EntityReferenceDictionaryが設定されていません");
                prefab = null;
                return false;
            }

            if (dict.TryGetValue(key, out var value))
            {
                prefab = value;
                return true;
            }

            Debug.LogError($"EntityFactory#TryGetValue: {key}が見つかりませんでした");
            prefab = null;
            return false;
        }
    }

    [Serializable]
    public class TilemapProviderDictionary : InspectorDictionary<int, TilemapProvider,TilemapProviderPair>
    {
    }

    [Serializable]
    public class TilemapProviderPair : InspectorDictionaryPair<int,TilemapProvider>
    {
        public TilemapProviderPair(int key, TilemapProvider value) : base(key, value)
        {
        }
    }
}