using System;
using System.Collections;
using System.Collections.Generic;
using Misc;
using Network;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class InLobbyUI : MonoBehaviour
{
    [SerializeField] private Button backButton, readyButton;
    [SerializeField] private TMP_Text lobbyCode;

    private void Awake()
    {
        backButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            LobbyController.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        lobbyCode.text = LobbyController.Instance.GetLobby().LobbyCode;
    }
}
