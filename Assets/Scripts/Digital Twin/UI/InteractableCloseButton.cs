using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCloseButton : MonoBehaviour
{
    public GameObject associatedGameObject;
    public void CloseWindow()
    {
        associatedGameObject = transform.parent.parent.parent.gameObject.GetComponent<InteractableUIValues>().associatedGameObject;
        associatedGameObject.GetComponent<Interactable>().ToggleUI();
    }
}
