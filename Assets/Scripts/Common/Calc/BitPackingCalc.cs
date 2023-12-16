using UnityEngine;

namespace Hackathon2023Winter.Calc
{
    public static class BitPackingCalc
    {
        public static long SerializePack_1(Vector2Int pos,Vector2Int velocity)
        {
            return ((long)pos.x << 48) | ((long)pos.y << 32) | ((long)velocity.x << 17) | ((long)velocity.y << 2);
        }

        public static long SerializePack_2(int rotate,int angularVelocity, Vector2Int scale)
        {
            return ((long)rotate << 48) | (long)angularVelocity << 31 | (long)scale.x << 19 | (long)scale.y << 7;
        }
        
        public static (Vector2Int,Vector2Int) DeserializePack_1(long pack)
        {
            var posX = (int)(pack >> 48) & 0xFFFF;
            var posY = (int)(pack >> 32) & 0xFFFF;
            var velocityX = (int)(pack >> 17) & 0x7FFF;
            var velocityY = (int)(pack >> 2) & 0x7FFF;
            return (new Vector2Int(posX,posY),new Vector2Int(velocityX,velocityY));
        }

        public static (int, int, Vector2Int) DeserializePack_2(long pack)
        {
            var rotate = (int)(pack >> 48) & 0xFFFF;
            var angularVelocity = (int)(pack >> 31) & 0x1FFFF;
            var scaleX = (int)(pack >> 19) & 0xFFF;
            var scaleY = (int)(pack >> 7) & 0xFFF;
            return (rotate,angularVelocity,new Vector2Int(scaleX,scaleY));
        }
    }
}