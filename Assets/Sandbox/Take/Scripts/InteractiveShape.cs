using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractiveShape : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private Material renderMaterial;
    [SerializeField] private GameObject particle;
    [SerializeField] private Vector2 edgeLength = new Vector2(8.0F, 4.0F);
    [SerializeField] private int maxParticle = 256;

    private List<Particle> _particles;
    private Vector2 _mouseOffset;

    private readonly int _sPositions = Shader.PropertyToID("particlePositions");
    private readonly int _sInfo = Shader.PropertyToID("particleInfo");
    private readonly int _sNum = Shader.PropertyToID("particleNum");
    
    void Start()
    {
        _particles = new List<Particle>();
        _mouseOffset = renderCamera.transform.position - mainCamera.transform.position;
        
        CreateParticles();
    }
    
    void Update()
    {
        Vector2 renderSize = renderCamera.ViewportToWorldPoint(Vector2.one)
                            - renderCamera.ViewportToWorldPoint(Vector2.zero);
        
        Vector4[] pos = _particles.Select(p =>
        {
            Vector2 pos = p.GetPosition();
            pos = renderCamera.WorldToViewportPoint(pos);
            return new Vector4(pos.x, pos.y, 0.0F, 0.0F);
        }).ToArray();
        
        Vector4[] info = _particles.Select(p =>
        {
            Vector2 scale = p.GetScale() / Mathf.Sqrt(2.0F);
            scale.x /= renderSize.x;
            scale.y /= renderSize.y;
            float rot = -p.GetRotation();
            return new Vector4(scale.x, scale.y, rot, 0.0F);
        }).ToArray();
        
        renderMaterial.SetVectorArray(_sPositions, pos);
        renderMaterial.SetVectorArray(_sInfo, info);
        renderMaterial.SetInt(_sNum, _particles.Count);
    }

    void CreateParticles()
    {
        _particles.Clear();
        
        for (int i = 0; i < maxParticle; i++)
        {
            GameObject obj = Instantiate(particle, myTransform.position, Quaternion.identity, myTransform);
            Particle part = obj.GetComponent<Particle>();
            if (part == null) { continue; }
            
            _particles.Add(part);

            part.MainCamera = mainCamera;
            part.MouseOffset = _mouseOffset;
            
            // 目的地を与える
            // float r = Mathf.Sqrt(Random.value) * rangeRadius;
            // float t = Random.value * Mathf.PI * 2.0F;
            // part.Destination = (Vector2) myTransform.position + new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * r;
            float x = (Random.value - 0.5F) * edgeLength.x;
            float y = (Random.value - 0.5F) * edgeLength.y;
            part.Destination = (Vector2) myTransform.position + new Vector2(x, y);
        }
    }
}
