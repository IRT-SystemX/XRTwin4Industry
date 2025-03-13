using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSwitchButton : MonoBehaviour
{
    public void OnClick()
    {
        transform.parent.parent.parent.GetComponent<XRDigitalTwin>().SwitchObjectMode();
    }
}
