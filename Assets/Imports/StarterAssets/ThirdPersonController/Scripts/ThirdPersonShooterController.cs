using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();
    [SerializeField] private Transform debugTransform;
    [SerializeField] private float playerRotationSmoothness;


    private ThirdPersonController thridPersonController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thridPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs.SetCursorState(true);
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero; 
        Vector2 screenCenterPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
        
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thridPersonController.SetSensitivity(aimSensitivity);
            thridPersonController.SetRotationMode(false);
            // rotate player model
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * playerRotationSmoothness);


        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thridPersonController.SetSensitivity(normalSensitivity);
            thridPersonController.SetRotationMode(true);
        }
        



    }
}
