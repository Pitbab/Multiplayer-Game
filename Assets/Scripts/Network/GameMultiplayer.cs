using System;
using System.Collections;
using System.Collections.Generic;
using Misc;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class GameMultiplayer : NetworkBehaviour
    {
        public static GameMultiplayer Instance { get; private set; }
        public event EventHandler OnTryingToJoinGame;
        public event EventHandler OnFailedToJoinGame;

        public List<GameObject> networkSpawnableList = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }

        public void StartHost()
        {
            //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
            NetworkManager.Singleton.StartHost();
        }

        private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
        {
            //if can join a game
            if (SceneManager.GetActiveScene().name != Loader.Scene.LobbyScene.ToString())
            {
                connectionApprovalResponse.Approved = false;
                connectionApprovalResponse.Reason = "Game has already started";
                return;
            }

            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= LobbyController.Instance.MAX_PLAYER_LOBBY)
            {
                connectionApprovalResponse.Approved = false;
                connectionApprovalResponse.Reason = "Game is full";
            }
            connectionApprovalResponse.Approved = true;
            connectionApprovalResponse.CreatePlayerObject = true;
            //else Approved = false
        }

        public void StartClient()
        {
            OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
            
            //NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
            //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.StartClient();
        }

        private void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
        {
            
        }

        private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
        {
            OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        }
        
        
        private void OnBulletHit(Vector3 position, Vector3 normal, int prefabIndex)
        {
            GameObject go = Instantiate(networkSpawnableList[prefabIndex].gameObject, position, Quaternion.LookRotation(normal));
            NetworkObject vfxNetwork = go.GetComponent<NetworkObject>();
            vfxNetwork.Spawn(true);
            Invoke("DestroyNetworkObjectWithDelay", 0.5f);
        }

        private void DestroyNetworkObjectWithDelay(NetworkObject obj)
        {
            obj.Despawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnBulletHitServerRpc(Vector3 position, Vector3 normal, int prefabIndex)
        {
            OnBulletHit(position, normal, prefabIndex);
        }
        
    }
    
    
}

