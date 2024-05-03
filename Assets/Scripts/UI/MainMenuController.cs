using System.Collections;
using System.Collections.Generic;
using Misc;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void Play()
    {
        Loader.Load(Loader.Scene.GameMenuScene);
    }
}
