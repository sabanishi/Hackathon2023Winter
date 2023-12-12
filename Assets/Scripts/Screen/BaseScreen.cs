using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hackathon2023Winter.Screen
{
    /// <summary>
    /// Screenのトップモジュールの基底クラス
    /// </summary>
    public abstract class BaseScreen : MonoBehaviour
    {
        /// <summary>
        /// Screen生成直後に呼ばれるメソッド
        /// </summary>
        public async UniTask Initialize(IScreenData screenData, CancellationToken token)
        {
            await InitializeInternal(screenData, token);
        }

        /// <summary>
        /// Screen破壊直前に呼ばれるメソッド
        /// </summary>
        public async UniTask<IScreenData> Dispose(CancellationToken token)
        {
            return await DisposeInternal(token);
        }

        /// <summary>
        /// ScreenがOpenされた直後に呼ばれるメソッド
        /// </summary>
        public async UniTask OpenDeal(CancellationToken token)
        {
            await OpenDealInternal(token);
        }

        /// <summary>
        /// ScreenがCloseされる直前に呼ばれるメソッド
        /// </summary>
        public async UniTask CloseDeal(CancellationToken token)
        {
            await CloseDealInternal(token);
        }

        /// <summary>
        /// ScreenをOpenする際のアニメーション
        /// </summary>
        public async UniTask OpenAnimation(CancellationToken token)
        {
            await OpenAnimationInternal(token);
        }

        /// <summary>
        /// ScreenをCloseする際のアニメーション
        /// </summary>
        public async UniTask CloseAnimation(CancellationToken token)
        {
            await CloseAnimationInternal(token);
        }

        protected virtual UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            return UniTask.FromResult<IScreenData>(null);
        }

        protected virtual UniTask OpenDealInternal(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask CloseDealInternal(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual async UniTask OpenAnimationInternal(CancellationToken token)
        {
            await BlackoutScreenAnimation.Instance.OpenAnimation(0.5f, token);
        }

        protected virtual async UniTask CloseAnimationInternal(CancellationToken token)
        {
            await BlackoutScreenAnimation.Instance.CloseAnimation(0.5f, token);
        }
    }
}