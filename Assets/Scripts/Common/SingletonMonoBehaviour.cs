using UnityEngine;

namespace Sabanishin.Common
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = GetComponent<T>();
                OnAwakeInternal();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                OnDestroyInternal();
                Instance = null;
            }
        }

        /// <summary>
        /// Awake時に実行されるメソッド
        /// Override用
        /// </summary>
        protected virtual void OnAwakeInternal()
        {
        }

        /// <summary>
        /// Destroy時に実行されるメソッド
        /// Override用
        /// </summary>
        protected virtual void OnDestroyInternal()
        {
        }
    }
}