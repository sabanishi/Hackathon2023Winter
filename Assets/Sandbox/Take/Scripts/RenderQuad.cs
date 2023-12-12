using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderQuad : MonoBehaviour
{
    [SerializeField] private Transform tra;
    [SerializeField] private Material mat;
    
    private Camera _mainCam;
    
    private readonly int _sPosition = Shader.PropertyToID("position");
    
    void Start()
    {
        _mainCam = Camera.main;
        Vector3 center = _mainCam.ViewportToWorldPoint(Vector2.one * 0.5F);
        tra.position = new Vector3(center.x, center.y, 0.0F);
        tra.localScale = _mainCam.ViewportToWorldPoint(Vector2.one)
            - _mainCam.ViewportToWorldPoint(Vector2.zero);
    }
    
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 position = _mainCam.ScreenToViewportPoint(mousePosition);
        mat.SetVector(_sPosition, position);
    }
}
