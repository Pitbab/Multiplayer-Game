using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }

        if (LobbyController.Instance != null)
        {
            Destroy(LobbyController.Instance.gameObject);
        }
    }
}
