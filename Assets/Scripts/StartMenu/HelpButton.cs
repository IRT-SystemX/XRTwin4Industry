using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpButton : MonoBehaviour
{
    public void OpenHelpPage()
    {
        string URL = "https://git.irt-systemx.fr/jni1/xrtwin4industry/-/wikis/3.-How-to-Use";
        #if UNITY_WSA
            UnityEngine.WSA.Launcher.LaunchUri(URL, false);
        #else
            Application.OpenURL(URL);
        #endif
    }
}
