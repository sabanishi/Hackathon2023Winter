using Cysharp.Threading.Tasks;
using Hackathon2023Winter.Screen;
using Photon.Pun;

namespace Hackathon2023Winter.MainGame
{
    /// <summary>
    /// Photon下でPUN2に紐づく操作を行うためのMainGameScreen直下のコンポーネント
    /// </summary>
    public class PunMainGameScreen : MonoBehaviourPunCallbacks
    {
        public void GoToStage()
        {
            photonView.RPC(nameof(RPC_GoToStage), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_GoToStage()
        {
            ScreenTransition.Instance.Move(ScreenType.MainGame).Forget();
        }
    }
}