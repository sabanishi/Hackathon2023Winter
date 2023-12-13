using System.Threading;
using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using UnityEngine;

namespace Hackathon2023Winter.StageSelect
{
    public class StageSelectScreen : BaseScreen
    {
        [SerializeField] private StageSelectScreenPresenter presenter;

        private StageSelectData _stageSelectData;

        protected override async UniTask InitializeInternal(IScreenData screenData, CancellationToken token)
        {
            if (screenData is not StageSelectData stageSelectData)
            {
                Debug.LogError("StageSelectDataが渡されていません");
                return;
            }

            _stageSelectData = stageSelectData;
            presenter.Setup(stageSelectData.IsOnline, stageSelectData.IsHost);
        }

        protected override async UniTask<IScreenData> DisposeInternal(CancellationToken token)
        {
            presenter.Cleanup();
            //TODO: レベルIDを渡す
            //TODO: タイトルに戻る場合の処理
            return new MainGameData(_stageSelectData.IsOnline, _stageSelectData.IsHost, 0);
        }
    }
}