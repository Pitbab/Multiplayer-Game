using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    private TMP_Text nameText, slotsText;
    private Button button;

    public void InitTextEntry(Lobby lobby)
    {
        nameText = gameObject.transform.Find("Name").GetComponent<TMP_Text>();
        slotsText = gameObject.transform.Find("Slots").GetComponent<TMP_Text>();
        button = GetComponentInChildren<Button>();

        if (nameText != null)
        {
            nameText.text = lobby.Name;
        }

        if (slotsText != null)
        {
            slotsText.text = lobby.AvailableSlots.ToString();
        }
        
    }

    public Button GetButton()
    {
        return button;
    }

}
