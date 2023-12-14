using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderQuad : MonoBehaviour
{
    [SerializeField] private Transform tra;
    [SerializeField] private Camera cam;
    
    void Start()
    {
        Vector3 center = cam.ViewportToWorldPoint(Vector2.one * 0.5F);
        tra.position = new Vector3(center.x, center.y, 0.0F);
        tra.localScale = cam.ViewportToWorldPoint(Vector2.one)
            - cam.ViewportToWorldPoint(Vector2.zero);
    }
}
