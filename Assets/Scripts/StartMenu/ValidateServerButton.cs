using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidateServerButton : MonoBehaviour
{
    public string savedValue;
    [SerializeField] AASClient aasClient;
    [SerializeField] StartMenuCanva startMenuCanva;

    bool hasAlreadyBeenClicked = false;
    public void OnClick()
    {
        if (!hasAlreadyBeenClicked)
        {
            StartCoroutine(OnClickCoroutine());
            hasAlreadyBeenClicked = true;
        }
    }

    IEnumerator OnClickCoroutine()
    {
        yield return StartCoroutine(aasClient.ConnectToServer(savedValue));
        startMenuCanva.SetAssetsToSelect();
        this.gameObject.SetActive(false);
    }
}
