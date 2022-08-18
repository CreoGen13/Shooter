using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace MAIN.Scripts.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button startServerButton;
        [SerializeField] private Button startHostButton;
        [SerializeField] private Button startClientButton;


        private void Awake()
        {
            Cursor.visible = true;
        }

        private void Start()
        {
            startServerButton.onClick.AddListener(() =>
            {
                if(NetworkManager.Singleton.StartServer())
                {
                    Debug.Log("Server started");
                }
                else
                {
                    Debug.Log("Server started ERROR");
                }
            });
            startHostButton.onClick.AddListener(() =>
            {
                if(NetworkManager.Singleton.StartHost())
                {
                    Debug.Log("Host started");
                }
                else
                {
                    Debug.Log("Host started ERROR");
                }
            });
            startClientButton.onClick.AddListener(() =>
            {
                if(NetworkManager.Singleton.StartClient())
                {
                    Debug.Log("Client started");
                }
                else
                {
                    Debug.Log("Client started ERROR");
                }
            });
        }
    }
}
