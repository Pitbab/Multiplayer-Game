using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton, joinCodeButton;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private Transform lobbyTemplate;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    private void Awake()
    {
        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });
        
        quickJoinButton.onClick.AddListener(() =>
        {
            LobbyController.Instance.QuickJoin();
        });
        
        joinCodeButton.onClick.AddListener(() =>
        {
            LobbyController.Instance.JoinWithCode(lobbyCodeInputField.text);
        });
        
        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        LobbyController.Instance.OnLobbyListChanged += LobbyController_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void LobbyController_OnLobbyListChanged(object sender, LobbyController.OnLobbyListChangedArgs e)
    {
        UpdateLobbyList(e.LobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyListContainer)
        {
            if(child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (var lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyListContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
            
        }
    }

    private void OnDestroy()
    {
        LobbyController.Instance.OnLobbyListChanged -= LobbyController_OnLobbyListChanged;
    }
}
