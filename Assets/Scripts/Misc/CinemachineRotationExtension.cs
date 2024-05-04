using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CinemachineRotationExtension : CinemachineExtension
{
    [Range(-180, 180)]
    public float HeadingRotation;
       
 
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if(stage != CinemachineCore.Stage.Aim) return;
        state.RawOrientation *= Quaternion.Euler(HeadingRotation, 0, 0);
        //state.RawPosition.z += HeadingRotation;
    }
}
