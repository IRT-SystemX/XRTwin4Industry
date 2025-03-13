using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerInputField : MonoBehaviour
{
    public bool debug = false;
    public ValidateServerButton validateServerButton;

    public void OnValueChange()
    {
        Debug.Log("value updated" + gameObject.GetComponent<TMP_InputField>().text);
        validateServerButton.savedValue = gameObject.GetComponent<TMP_InputField>().text;
    }

    private void Update()
    {
        if (debug == true)
        {
            OnValueChange();
            debug = false;
        }
    }
}
