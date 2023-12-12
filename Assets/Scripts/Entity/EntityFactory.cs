using Sabanishin.Common;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    /// <summary>
    /// EntityTypeからEntityを生成するクラス
    /// </summary>
    public class EntityFactory : SingletonMonoBehaviour<EntityFactory>
    {
        private EntityReferenceDict _dict;

        protected override void OnAwakeInternal()
        {
            _dict = Resources.Load<EntityReferenceDict>("EntityReferenceDictionary");
        }

        public BaseEntity Create(EntityType type)
        {
            return _dict.Create(type);
        }
    }
}