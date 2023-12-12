using System.Threading;
using Cysharp.Threading.Tasks;
using Sabanishin.Common;
using UnityEngine;

namespace Hackathon2023Winter.Screen
{
    public class NowLoadingPanel: SingletonMonoBehaviour<NowLoadingPanel>
    {
        [SerializeField] private GameObject root;
        protected override void OnAwakeInternal()
        {
            root.SetActive(false);
        }

        /// <summary>
        /// time秒かけて暗転を解除する
        /// </summary>
        public async UniTask OpenAnimation(float time, CancellationToken token)
        {
            root.SetActive(false);
            await TmpScreenAnimation.Instance.OpenAnimation(time, token);
        }

        /// <summary>
        /// time秒かけて暗転する
        /// </summary>
        public async UniTask CloseAnimation(float time, CancellationToken token)
        {
            await TmpScreenAnimation.Instance.CloseAnimation(time, token);
            root.SetActive(true);
            await TmpScreenAnimation.Instance.OpenAnimation(0f, token);
        }
    }
}