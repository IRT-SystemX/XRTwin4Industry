using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*Set values from Digital Twin to UI*/
public class InteractableUIValues : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI componentNameText;
    [SerializeField] TextMeshProUGUI componentDescText;
    [SerializeField] GameObject ValueTextPrefab;
    public GameObject associatedGameObject;

    List<GameObject> valuetexts = new List<GameObject>();

    private void Start()
    {
    }

    public void SetName(string newName)
    {
        componentNameText.text = newName;
    }

    public void SetDesc(string newDesc)
    {
        componentDescText.text = newDesc;
    }

    public void SetValue(int valueID, Value valueObj, string valueToDisplay)
    {
        if (valuetexts.Count <= valueID)
        {
            GameObject valuetext = GameObject.Instantiate(ValueTextPrefab, this.transform.GetChild(0));
            valuetext.GetComponent<InitInteractableValueText>().Init(valueObj.name, associatedGameObject);
            valuetexts.Add(valuetext);
            valuetext.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = valueObj.name + " : " + valueToDisplay +
                "\nMin value: " + valueObj.min + "\nMax value :" + valueObj.max;
        } else
        {
            GameObject valuetext = valuetexts[valueID];
            valuetext.GetComponent<InitInteractableValueText>().Init(valueObj.name, associatedGameObject);
            valuetexts.Add(valuetext);
            valuetext.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = valueObj.name + " : " + valueToDisplay +
                "\nMin value: " + valueObj.min + "\nMax value :" + valueObj.max;
        }
    }

}
