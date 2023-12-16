using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Hackathon2023Winter.Entity
{
    [CustomEditor(typeof(SwitchEntity))]
    public class SwitchEntityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var switchEntity = target as SwitchEntity;
            //Tilemapの情報を元にEntityを生成する
            if (GUILayout.Button("Setup"))
            {
                switchEntity?.SetSwitchTarget();
            }
        }
    }
}

#endif