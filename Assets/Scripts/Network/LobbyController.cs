using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Misc;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Network
{
    public class LobbyController : MonoBehaviour
    {

        private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
        private Lobby joinedLobby;
        public readonly int MAX_PLAYER_LOBBY = 2;
        private float heartBeatTimer;
        private float listLobbyTimer;
        public static LobbyController Instance { get; private set; }

        public event EventHandler OnCreateLobbyStarted;
        public event EventHandler OnCreateLobbyFailed;
        public event EventHandler OnJoinStarted;
        public event EventHandler OnQuickJoinFailed;
        public event EventHandler <OnLobbyListChangedArgs> OnLobbyListChanged;

        public class OnLobbyListChangedArgs : EventArgs
        {
            public List<Lobby> LobbyList;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        
            InitializedUnityAuthentication();
            
        }

        private async void InitializedUnityAuthentication()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                InitializationOptions initializationOptions = new InitializationOptions();
                initializationOptions.SetProfile(Random.Range(0, 1000).ToString());
                
                await UnityServices.InitializeAsync(initializationOptions);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            
            Debug.Log(AuthenticationService.Instance.PlayerId);

        }

        private void Update()
        {
            HandleHeartBeat();
            HandlePeriodicListLobbies();
        }

        private void HandlePeriodicListLobbies()
        {
            if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name == Loader.Scene.GameMenuScene.ToString())
            {
                listLobbyTimer -= Time.deltaTime;
                if (listLobbyTimer <= 0f)
                {
                    float listLobbiesTimerMax = 3f;
                    listLobbyTimer = listLobbiesTimerMax;
                    ListLobbies();
                }
            }

        }



        private void HandleHeartBeat()
        {
            if (IsLobbyHost())
            {
                heartBeatTimer -= Time.deltaTime;
                if (heartBeatTimer <= 0f)
                {
                    float heartBeatTimerMax = 15f;
                    heartBeatTimer = heartBeatTimerMax;

                    LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                }
            }
        }

        private bool IsLobbyHost()
        {
            return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        private async void ListLobbies()
        {
            try
            {
                var queryOptions = new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    }
                };

                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            
                OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedArgs()
                {
                    LobbyList = response.Results
                });
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }

        }


        private async Task<Allocation> AllocateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYER_LOBBY -1);

                return allocation;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                return default;
            }

        }

        private async Task<String> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                return relayJoinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                return default;
            }

        }

        private async Task<JoinAllocation> JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                return default;
            }
        }
        
        public async void CreateLobby(String lobbyName, bool isPrivate)
        {
            OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
            try
            {
                joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYER_LOBBY, new CreateLobbyOptions()
                {
                    IsPrivate = isPrivate
                });

                Allocation allocation = await AllocateRelay();

                string relayJoinCode = await GetRelayJoinCode(allocation);
                await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions 
                {
                    Data = new Dictionary<string, DataObject> 
                    {
                        { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                    }
                });

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
                GameMultiplayer.Instance.StartHost();
                
                Loader.LoadNetwork(Loader.Scene.LobbyScene);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            }

        }

        public async void QuickJoin()
        {
            OnJoinStarted?.Invoke(this, EventArgs.Empty);
            try
            {
                joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
                
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

                GameMultiplayer.Instance.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
            }

        }
        
        public async void JoinWithId(string lobbyId)
        {
            OnJoinStarted?.Invoke(this, EventArgs.Empty);
            try
            {
                joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
                
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

                GameMultiplayer.Instance.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
            }

        }
        
        public async void JoinWithCode(string lobbyCode)
        {
            OnJoinStarted?.Invoke(this, EventArgs.Empty);
            try
            {
                joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                
                string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
                
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
                
                GameMultiplayer.Instance.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        public async void DeleteLobby()
        {
            if (joinedLobby != null)
            {
                try
                {
                    await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                    joinedLobby = null;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }

            }
        }

        public async void LeaveLobby()
        {
            if (joinedLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                    joinedLobby = null;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }

            }
        }
        
        public async void KickPlayer(string playerId)
        {
            if (IsLobbyHost())
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                    joinedLobby = null;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }

            }
        }

        public Lobby GetLobby()
        {
            return joinedLobby;
        }
    }
}

