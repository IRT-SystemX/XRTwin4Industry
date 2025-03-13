using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartMenuAssetItem : MonoBehaviour
{
    public int assetNbr;
    public StartMenuCanva startMenuCanva;
    [SerializeField] TextMeshProUGUI assetNameText;
    bool isActive = true;

    public void SetName(string name)
    {
        assetNameText.text = name;
    }

    public void Toggle()
    {
        isActive = !isActive;
        startMenuCanva.ToggleAsset(assetNbr);
    }
}
