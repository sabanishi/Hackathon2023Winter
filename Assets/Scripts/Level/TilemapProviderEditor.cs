using UnityEditor;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    /// <summary>
    /// Editor上でTilemapProviderに操作を行うためのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(TilemapProvider))]
    public class TilemapProviderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var provider = target as TilemapProvider;
            //Tilemapの情報を元にEntityを生成する
            if (GUILayout.Button("Create Entities"))
            {
                provider.CreateEntities();
            }

            //全てのEntityを削除する
            if (GUILayout.Button("Delete All Entities"))
            {
                provider.DeleteEntities();
            }
        }
    }
}