using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hackathon2023Winter.Level
{
    public class TilemapProvider:MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        public Tilemap Tilemap => tilemap;
    }
}