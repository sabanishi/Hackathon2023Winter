using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class InteractiveShape : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private Material renderMaterial;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject particle;
    [SerializeField] private Vector2 edgeLength = new Vector2(8.0F, 4.0F);
    [SerializeField] private Vector2 rangeCenter;
    [SerializeField] private int maxParticle = 256;
    [SerializeField] private GameObject positionObjA;
    [SerializeField] private GameObject positionObjR;
    [SerializeField] private GameObject positionObjP;
    [Header("1フレームあたりに生成する数")][SerializeField] private int instantiateNum;

    private List<Particle> _particles;
    private Vector2 _mouseOffset;
    private bool _isCreateParticles;
    private Vector2 _positionA, _positionR, _positionP;

    private readonly int _sPositions = Shader.PropertyToID("particlePositions");
    private readonly int _sInfo = Shader.PropertyToID("particleInfo");
    private readonly int _sColors = Shader.PropertyToID("particleColors");
    private readonly int _sNum = Shader.PropertyToID("particleNum");
    
    public async UniTask Initialize()
    {
        _particles = new List<Particle>();
        _mouseOffset = myTransform.position - mainCamera.transform.position;
        
        _positionA = positionObjA.transform.position;
        _positionR = positionObjR.transform.position;
        _positionP = positionObjP.transform.position;
        await CreateParticles();
    }
    
    void Update()
    {
        if (!_isCreateParticles)
        {
            renderMaterial.SetInt(_sNum, 0);
            return;
        }

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

        // if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        //     _particles.ForEach(p => Destroy(p.gameObject));
        //     _isCreateParticles = false;
        // }
    }
    
    private int _startPos;
    private Texture2D _tex;
    private List<Vector4> _colors;
    private bool _canCreateParticle;

    private async UniTask CreateParticles()
    {
        _particles.Clear();
        
        _colors = new List<Vector4>();
        
        // 画面上の色を保存するためのテクスチャ
        _tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
        
        // アクティブなレンダーテクスチャの切り替え
        RenderTexture cacheTex = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        //パーティクルの生成を行う
        _startPos = 0;
        while (_startPos < maxParticle)
        {
            _canCreateParticle = true;
            await UniTask.WaitUntil(() => !_canCreateParticle);
            this.GetCancellationTokenOnDestroy().ThrowIfCancellationRequested();
        }
        
        // アクティブなレンダーテクスチャを戻す
        RenderTexture.active = cacheTex;
        
        renderMaterial.SetVectorArray(_sColors, _colors);
        _isCreateParticles = true;
            
        //全てのパーティクルをアクティブにする
        _particles.ForEach(p => p.gameObject.SetActive(true));
    }

    private int CreatePartParticles()
    {
        int counter = 0;
        int i;
        for (i = _startPos; i < maxParticle; i++)
        {
            if(counter>= instantiateNum)
            {
                return i;
            }
            counter++;
            
            // 目的地を計算
            // float r = Mathf.Sqrt(Random.value) * rangeRadius;
            // float t = Random.value * Mathf.PI * 2.0F;
            // part.Destination = (Vector2) myTransform.position + new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * r;
            float x = (Random.value - 0.5F) * edgeLength.x;
            float y = (Random.value - 0.5F) * edgeLength.y;
            Vector2 dest = rangeCenter + new Vector2(x, y);
            
            // 目的地のレンダーテクスチャ上の位置
            Vector2 positionOnTex = renderCamera.WorldToViewportPoint(dest);
#if UNITY_STANDALONE_WIN
            positionOnTex.y = 1.0F - positionOnTex.y;
#endif
            positionOnTex *= new Vector2(renderTexture.width, renderTexture.height);
            
            // レンダーテクスチャ上の色をテクスチャ(Texture2D)に書き込む
            _tex.ReadPixels(new Rect(positionOnTex.x, positionOnTex.y, 1, 1), 0, 0);
            _tex.Apply();
            
            // Texture2Dから色を取得
            Color col = _tex.GetPixel(0, 0);
            Vector4 colVec = new Vector4(col.r, col.g, col.b, 1.0F);

            Vector2 createPosition;

            if (colVec is { x: < 0.1F, y: > 0.9F, z: < 0.1F })
            { // green: A
                createPosition = _positionA;
            }
            else if (colVec is { x: > 0.9F, y: > 0.7F, z: < 0.1F })
            { // yellow: R
                createPosition = _positionR;
            }
            else if (colVec is { x: > 0.9F, y: < 0.1F, z: < 0.1F })
            { // red: P
                createPosition = _positionP;
            }
            else
            { // otherwise
                i--;
                continue;
            }
            _colors.Add(colVec);
            
            // パーティクルの生成
            GameObject obj = Instantiate(particle, createPosition, Quaternion.identity, myTransform);
            Particle part = obj.GetComponent<Particle>();
            if (part == null) { continue; }
            
            _particles.Add(part);

            part.MainCamera = mainCamera;
            part.MouseOffset = _mouseOffset;
            part.Destination = dest;
            //全てのパーティクルが生成されるまで非アクティブにする
            part.gameObject.SetActive(false);
        }
        return i;
    }

    void OnPostRender()
    {
        if (!_canCreateParticle) { return; }
        _startPos = CreatePartParticles();
        _canCreateParticle = false;
    }
}
