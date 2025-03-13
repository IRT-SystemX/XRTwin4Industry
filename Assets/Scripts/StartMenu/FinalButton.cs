using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalButton : MonoBehaviour
{
    [SerializeField] StartMenuCanva startMenuCanva;

    private void Start()
    {
        /*Transform parent = transform.parent;
        transform.SetParent(null);
        transform.SetParent(parent);*/
        transform.parent.SetAsLastSibling();
    }

    private void Update()
    {
    }


    public void OnClick()
    {
        startMenuCanva.Validate();
    }
}
