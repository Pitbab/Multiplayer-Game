using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button backButton, createButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle privateToggle;

    private void Awake()
    {
        lobbyNameInputField.text = "My lobby";
        createButton.onClick.AddListener(() =>
        {
            LobbyController.Instance.CreateLobby(lobbyNameInputField.text, privateToggle.isOn);
        });
        
        backButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);    
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    
    
}
