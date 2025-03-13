using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetToggle : MonoBehaviour
{
    [SerializeField] StartMenuAssetItem startMenuAssetItem;

    public void OnClick()
    {
        startMenuAssetItem.Toggle();
    }
   
}
