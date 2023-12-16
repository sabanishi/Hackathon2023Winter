using System.Threading;
using Cysharp.Threading.Tasks;
using Sabanishin.Common;
using UnityEngine;

namespace Hackathon2023Winter.Screen
{
    /// <summary>
    /// Now Loading... と表示するパネルを見せるアニメーション
    /// </summary>
    public class NowLoadingAnimation : SingletonMonoBehaviour<NowLoadingAnimation>
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject　text;

        protected override void OnAwakeInternal()
        {
            root.SetActive(false);
            text.SetActive(false);
        }

        public async UniTask OpenAnimation(float time, CancellationToken token)
        {
            root.SetActive(false);
            text.SetActive(false);
            await BlackoutScreenAnimation.Instance.OpenAnimation(time, token);
        }

        public async UniTask CloseAnimation(float time, CancellationToken token)
        {
            await BlackoutScreenAnimation.Instance.CloseAnimation(time, token);
            root.SetActive(true);
            text.SetActive(true);
            await BlackoutScreenAnimation.Instance.OpenAnimation(0f, token);
        }
    }
}