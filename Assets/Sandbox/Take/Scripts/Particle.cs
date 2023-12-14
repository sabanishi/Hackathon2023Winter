using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private Rigidbody2D myRigidbody;
    [SerializeField] private float maxForce = 32.0F;
    [SerializeField] private float distThresh = 4.0F;
    [SerializeField] private float mouseRadius = 1.0F;
    // x: min, y: max
    [SerializeField] private Vector2 particleScale;

    private Camera _mainCamera;
    private Vector2 _mouseOffset;
    private Vector2 _destination;

    public Camera MainCamera
    {
        get => _mainCamera;
        set => _mainCamera = value;
    }
    public Vector2 MouseOffset
    {
        get => _mouseOffset;
        set => _mouseOffset = value;
    }
    public Vector2 Destination
    {
        get => _destination;
        set => _destination = value;
    }
    
    void Start()
    {
        // 大きさを計算
        float rand = Random.value;
        rand *= rand;
        float scale = (1.0F - rand) * particleScale.x + rand * particleScale.y;
        myTransform.localScale = new Vector3(scale, scale, 0.0F);
        
        // 物理演算用パラメータ設定
        myRigidbody.mass = Random.Range(0.3F, 1.0F);
        myRigidbody.gravityScale = 0.0F;
        myRigidbody.drag = 1.0F;
        myRigidbody.angularDrag = 0.1F;
    }
    
    void FixedUpdate()
    {
        GoToDestination();
        AvoidMouse();
    }

    void GoToDestination()
    {
        Vector2 pos = myRigidbody.position;
        float dist = Vector2.Distance(pos, _destination);

        float distVal = Mathf.Min(dist / distThresh, 1.0F);
        if (distVal < 0.01F) { return; }
        distVal = 1.0F - (1.0F - distVal) * (1.0F - distVal);
        
        Vector2 force = _destination - pos;
        force.Normalize();
        force *= distVal * maxForce;
        myRigidbody.AddForce(force, ForceMode2D.Force);
    }

    void AvoidMouse()
    {
        Vector2 pos = myRigidbody.position;
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos += _mouseOffset;
        
        float dist = Vector2.Distance(pos, mousePos);
        float distVal = dist / mouseRadius;
        if (distVal > 1.0F) { return; }
        distVal = 1.0F - distVal * distVal;//(1.0F - distVal) * (1.0F - distVal);

        Vector2 force = pos - mousePos;
        force.Normalize();
        force *= distVal * maxForce * 4.0F;
        myRigidbody.AddForce(force, ForceMode2D.Force);
    }

    public Vector2 GetPosition()
    {
        return myRigidbody.position;
    }

    public Vector2 GetScale()
    {
        return myTransform.localScale;
    }

    public float GetRotation()
    {
        return myTransform.eulerAngles.z * Mathf.Deg2Rad;
    }
}
