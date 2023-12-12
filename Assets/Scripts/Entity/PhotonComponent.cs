using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.Entity
{
    public class PhotonComponent:MonoBehaviourPun
    {
        private void Awake()
        {
            if (!photonView.IsMine)
            {
                if (GetComponent<Rigidbody2D>() != null)
                {
                    GetComponent<Rigidbody2D>().isKinematic = true;
                }
            }
        }
    }
}