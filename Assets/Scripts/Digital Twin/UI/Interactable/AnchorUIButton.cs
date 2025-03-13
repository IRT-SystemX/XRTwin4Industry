using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorUIButton : MonoBehaviour
{
    public RadialView radialView;

    public void OnClick()
    {
        radialView.enabled = !radialView.enabled;
    }
}
