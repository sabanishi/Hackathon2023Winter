using System;
using Hackathon2023Winter.Entity;
using Sabanishi.Common;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// TileBaseとEntityTypeの対応関係を保持するクラス
    /// </summary>
    [CreateAssetMenu(fileName = "LevelChipDict", menuName = "ScriptableObject/LevelChipDict")]
    public class LevelChipDict : ScriptableObject
    {
        [SerializeField] private ChipDictionary chipDictionary;

        /// <summary>
        /// keyに対応するEntityTypeを取得する
        /// </summary>
        public EntityType GetChipType(TileBase key)
        {
            var dict = chipDictionary?.GetDict();
            if (dict.IsNullOrEmpty())
            {
                Debug.LogError($"LevelChipDict#GetChipType: ChipDictionaryが設定されていません");
                return EntityType.None;
            }

            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }

            Debug.LogError($"LevelChipDict#GetChipType: {key}が見つかりませんでした");
            return EntityType.None;
        }
    }

    [Serializable]
    public class ChipDictionary : InspectorDictionary<TileBase, EntityType, ChipPair>
    {
    }

    [Serializable]
    public class ChipPair : InspectorDictionaryPair<TileBase, EntityType>
    {
        public ChipPair(Tile tile, EntityType value) : base(tile, value)
        {
        }
    }
}