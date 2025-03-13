using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuButton : MonoBehaviour
{
    public bool isMaterialTransparent = true;

    public void SetHighVisibilityMaterial()
    {
        GameObject[] DigitalTwins =  GameObject.FindGameObjectsWithTag("DigitalTwin");
        foreach (GameObject _DigitalTwin in DigitalTwins)
        {
            _DigitalTwin.GetComponent<XRDigitalTwin>().SetHighVisibilityMaterial();
        }
        isMaterialTransparent = false;
    }

    public void SetTransparentMaterial()
    {
        GameObject[] DigitalTwins = GameObject.FindGameObjectsWithTag("DigitalTwin");
        foreach (GameObject _DigitalTwin in DigitalTwins)
        {
            _DigitalTwin.GetComponent<XRDigitalTwin>().SetTransparentMaterial();
        }
        isMaterialTransparent = true;
    }
    public void OnClick()
    {
        if (isMaterialTransparent)
            SetHighVisibilityMaterial();
        else
            SetTransparentMaterial();
    }
    public void ChangeMaterialByVoice()
    {
        if (isMaterialTransparent)
            SetHighVisibilityMaterial();
        else
            SetTransparentMaterial();
    }
}
