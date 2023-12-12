using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor( typeof( Tilemap ) )]
public sealed class TilemapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var tilemap = target as Tilemap;

        if ( GUILayout.Button( "Compress Bounds" ) )
        {
            Undo.RecordObject( tilemap, "Compress Bounds" );
            tilemap.CompressBounds();
            EditorSceneManager.MarkSceneDirty( tilemap.gameObject.scene );
            
            Debug.Log(tilemap.size.x+" "+tilemap.size.y);
        }
    }
}