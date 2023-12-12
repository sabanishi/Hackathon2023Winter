using Hackathon2023Winter.Level;
using Photon.Pun;
using UnityEngine;

namespace Hackathon2023Winter.MainGame
{
    public class MainGameManager:MonoBehaviour
    {
        [SerializeField]private LevelEntityManager levelEntityManager;
        [SerializeField] private TilemapProvider tilemapProvider;
        public void Setup()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                levelEntityManager.Setup();
                levelEntityManager.CreateLevel(tilemapProvider,true);
            }
            else
            {
                Debug.Log("not master client");
            }

            //var prefabName = "LevelEntityManager";
            //var obj =PhotonNetwork.Instantiate(prefabName,Vector3.zero, Quaternion.identity);
        }

        public void Cleanup()
        {
            
        }
    }
}