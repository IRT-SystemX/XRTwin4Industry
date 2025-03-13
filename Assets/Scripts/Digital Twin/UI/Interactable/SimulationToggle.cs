using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationToggle : MonoBehaviour
{
    public string valueName;
    public GameObject associatedGameObject;

    public void ToggleSimulation()
    {
        Interactable interactableScript = associatedGameObject.GetComponent<Interactable>();
        interactableScript.EnableSimulationFor(valueName);
    }

    public void UnToggleSimulation()
    {
        Interactable interactableScript = associatedGameObject.GetComponent<Interactable>();
        interactableScript.DisableSimulationFor(valueName);
    }
}
