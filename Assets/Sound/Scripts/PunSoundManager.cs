using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace Sound.Scripts
{
    /// <summary>
    /// Photonを介して相手側でSEを再生させるためのクラス
    /// </summary>
    public class PunSoundManager:MonoBehaviourPun
    {
        private void Awake()
        {
            _instance = this;
        }
        
        private static PunSoundManager _instance;
        public static PunSoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    return null;
                }

                return _instance;
            }
        }

        public void PlaySE(SE_Enum type)
        {
            photonView.RPC(nameof(RPC_PlaySE), RpcTarget.Others, type);
        }
        
        [PunRPC]
        private void RPC_PlaySE(SE_Enum type)
        {
            SoundManager.PlaySE(type);
        }
    }
}