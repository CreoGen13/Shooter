using System;
using System.Reflection;
using MAIN.Scripts.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MAIN.Scripts
{
    public class Main : MonoBehaviour
    {
        private MainModel _model;
        
        public Player Player { get; private set; }
        public Scrollbar Scrollbar => scrollbar;
        public GameObject Overlay => overlay;

        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private HUD hud;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private GameObject overlay;

        private void Start()
        {
            _model = new MainModel(this);
            StartNetworkManager();
        }
        
        private void Update()
        {
            _model.Update();
        }

        private void StartNetworkManager()
        {
            
            networkManager.OnClientConnectedCallback += (clientId) =>
            {
                Debug.Log("Player " + clientId + " has connected");
            };
            networkManager.OnClientDisconnectCallback += (clientId) =>
            {
                Debug.Log("Player " + clientId + " has disconnected");
            };
        }

        public void SetPlayer(Player player)
        {
            Player = player;
        }
    }
}
