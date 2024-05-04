
using System;
using Cinemachine;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworkWrapper : NetworkBehaviour
{
    [SerializeField] private ThirdPersonShooterController thirdPersonShooterController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera normalCam, aimCam;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private GameObject playerUI;

    private void Start()
    {
        if (!IsOwner) return;
        
        //thirdPersonController.SetOwner(true);
        thirdPersonController.enabled = true;
        playerInput.enabled = true;
        thirdPersonShooterController.enabled = true;
        normalCam.enabled = true;
        aimCam.enabled = true;
        mainCamera.enabled = true;
        audioListener.enabled = true;
        playerUI.SetActive(true);
    }
}
