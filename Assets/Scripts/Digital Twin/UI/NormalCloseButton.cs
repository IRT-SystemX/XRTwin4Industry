using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCloseButton : MonoBehaviour
{
    public void Close()
    {
        this.gameObject.transform.parent.parent.gameObject.SetActive(false);
    }
}
