using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sabanishin.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Screen
{
    /// <summary>
    /// 簡易的な画面遷移アニメーション
    /// </summary>
    public class BlackoutScreenAnimation : SingletonMonoBehaviour<BlackoutScreenAnimation>
    {
        [SerializeField] private Image image;

        protected override void OnAwakeInternal()
        {
            image.color = new Color(0, 0, 0, 0);
        }

        /// <summary>
        /// time秒かけて暗転を解除する
        /// </summary>
        public async UniTask OpenAnimation(float time, CancellationToken token)
        {
            image.enabled = true;
            image.color = new Color(0, 0, 0, 1);
            await image.DOFade(0, time).ToUniTask(cancellationToken: token);
            image.enabled = false;
        }

        /// <summary>
        /// time秒かけて暗転する
        /// </summary>
        public async UniTask CloseAnimation(float time, CancellationToken token)
        {
            image.enabled = true;
            image.color = new Color(0, 0, 0, 0);
            await image.DOFade(1, time).ToUniTask(cancellationToken: token);
        }
    }
}