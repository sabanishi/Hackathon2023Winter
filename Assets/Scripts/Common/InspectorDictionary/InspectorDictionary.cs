using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sabanishi.Common
{
    /// <summary>
    /// インスペクター上でDictionaryを扱うための基底クラス
    /// </summary>
    [Serializable]
    public class InspectorDictionary<TKey, TValue, TType> where TType : InspectorDictionaryPair<TKey, TValue>
    {
        [SerializeField] private List<TType> list;

        private Dictionary<TKey, TValue> _dict;

        public Dictionary<TKey, TValue> GetDict()
        {
            if (_dict == null)
            {
                _dict = ConvertListToDictionary(list);
            }

            return _dict;
        }

        /// <summary>
        /// インスペクター上のリストをDictionaryに変換する
        /// </summary>
        private static Dictionary<TKey, TValue> ConvertListToDictionary(List<TType> list)
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var pair in list)
            {
                dict.Add(pair.Key, pair.Value);
            }

            return dict;
        }
    }
}