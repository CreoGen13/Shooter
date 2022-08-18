using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MAIN.Scripts.UI
{
    public class PlayerHUD : NetworkBehaviour
    {
        private readonly NetworkVariable<NetworkString> _playerName = new NetworkVariable<NetworkString>();
        private bool _overlaySet = false;


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _playerName.Value = new NetworkString(new ForceNetworkSerializeByMemcpy<FixedString32Bytes>(OwnerClientId.ToString()));
            }
        }

        // public override void OnNetworkDespawn()
        // {
        //     Debug.Log("WDaw");
        //     if (IsServer)
        //     {
        //         _playersName.Value = new NetworkString(new ForceNetworkSerializeByMemcpy<FixedString32Bytes>(OwnerClientId.ToString()));
        //     }
        // }

        private void SetOverlay()
        {
            var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            localPlayerOverlay.text = "Player " + _playerName.Value;
        }

        private void Update()
        {
            if (!_overlaySet && !string.IsNullOrEmpty(_playerName.Value.ToString()))
            {
                SetOverlay();
                _overlaySet = true;
            }
        }
    }

    public struct NetworkString : INetworkSerializable
    {
        public NetworkString(ForceNetworkSerializeByMemcpy<FixedString32Bytes> info)
        {
            _info = info;
        }
        private ForceNetworkSerializeByMemcpy<FixedString32Bytes> _info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _info);
        }

        public override string ToString()
        {
            return _info.Value.ToString();
        }
    }
}