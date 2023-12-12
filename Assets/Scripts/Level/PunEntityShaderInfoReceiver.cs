using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.Level
{
    public class PunEntityShaderInfoReceiver:MonoBehaviourPun
    {
        private ReactiveProperty<Vector2> _circleScale;
        private ReactiveProperty<Vector2> _rectScale;
        
        public IReadOnlyReactiveProperty<Vector2> CircleScale => _circleScale;
        public IReadOnlyReactiveProperty<Vector2> RectScale => _rectScale;

        public void Setup()
        {
            _circleScale = new();
            _rectScale = new ();
        }
        
        public void SetScales(Vector2 circleScale,Vector2 rectScale)
        {
            _circleScale.Value = circleScale;
            _rectScale.Value = rectScale;
        }
        
        public void Cleanup()
        {
            _circleScale.Dispose();
            _rectScale.Dispose();
        }
    }
}