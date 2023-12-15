#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    [CustomEditor(typeof(JumpRampEntity))]
    public class JumpRampEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var jumpRamp = target as JumpRampEntity;
            //Tilemapの情報を元にEntityを生成する
            if (GUILayout.Button("Setup"))
            {
                jumpRamp.SetBarHeight();
            }
        }
    }
}
#endif