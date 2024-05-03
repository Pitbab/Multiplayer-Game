using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    private Lobby lobby;

    [SerializeField] private TMP_Text lobbyNameText, lobbySlots;

    private void Awake()
    {
        GetComponentInChildren<Button>().onClick.AddListener(() => LobbyController.Instance.JoinWithId(lobby.Id));
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        lobbySlots.text = lobby.AvailableSlots.ToString();
    }
    
    
}
