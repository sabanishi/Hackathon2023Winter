using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGetTest : MonoBehaviour
{
    private Texture2D _tex;
    [SerializeField] private RenderTexture rTex;
    
    void Start()
    {
        _tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
    }
    
    void OnPostRender()
    {
        GetColorOnRenderTexture();
    }

    void GetColorOnScreen()
    {
        Vector2 pos = Input.mousePosition;
        _tex.ReadPixels(new Rect(pos.x, pos.y, 1, 1), 0, 0);
        _tex.Apply();
        Color color = _tex.GetPixel(0, 0);
        Vector3 col = new Vector3(color.r, color.g, color.b);
        Debug.Log(color);
        Debug.Log("pos: " + pos);
    }

    void GetColorOnRenderTexture()
    {
        RenderTexture cacheTex = RenderTexture.active;
        RenderTexture.active = rTex;
        
        Vector2 pos = Input.mousePosition;
        pos = Camera.main.ScreenToViewportPoint(pos);
        pos *= new Vector2(rTex.width, rTex.height);
        
        _tex.ReadPixels(new Rect(pos.x, pos.y, 1, 1), 0, 0);
        _tex.Apply();

        RenderTexture.active = cacheTex;
        
        Color color = _tex.GetPixel(0, 0);
        Vector3 col = new Vector3(color.r, color.g, color.b);
        Debug.Log(color);
        Debug.Log("pos: " + pos);
    }
}
