using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableInputField : MonoBehaviour
{
    public string valueName;
    public bool debug = false;
    public GameObject associatedGameObject;

    public void OnValueChange()
    {
        Debug.Log("value updated" + gameObject.GetComponent<TMP_InputField>().text);
        Interactable interactableScript = associatedGameObject.GetComponent<Interactable>();
        interactableScript.HandleInputValue(valueName, gameObject.GetComponent<TMP_InputField>().text);
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
