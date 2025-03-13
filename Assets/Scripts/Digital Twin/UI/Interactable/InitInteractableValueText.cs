using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitInteractableValueText : MonoBehaviour
{
    public GameObject inputField;
    public GameObject toggleButton;
    public string valueName;
    public void Init(string _valueName, GameObject associatedGameObject)
    {
        inputField.GetComponent<InteractableInputField>().valueName = _valueName;
        inputField.GetComponent<InteractableInputField>().associatedGameObject = associatedGameObject;
        toggleButton.GetComponent<SimulationToggle>().valueName = _valueName;
        toggleButton.GetComponent<SimulationToggle>().associatedGameObject = associatedGameObject;
        valueName = _valueName;

    }
}
