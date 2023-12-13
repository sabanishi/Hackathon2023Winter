using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Photon.Pun;

namespace Hackathon2023Winter.MainGame
{
    /// <summary>
    /// Photon下でPUN2に紐づく操作を行うためのMainGameScreen直下のコンポーネント
    /// </summary>
    public class PunMainGameScreen:MonoBehaviourPunCallbacks
    {
        public void GoToStageSelectScreen()
        {
            photonView.RPC(nameof(RPC_GoToStageSelectScreen),RpcTarget.All);
        }
        
        [PunRPC]
        private void RPC_GoToStageSelectScreen()
        {
            ScreenTransition.Instance.Move(ScreenType.StageSelect).Forget();
        }
    }
}