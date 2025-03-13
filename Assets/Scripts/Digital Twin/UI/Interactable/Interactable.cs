using MixedReality.Toolkit.UX;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using System.Linq;
using System.Security.Policy;
using UnityEngine.Networking;

public class Value
{
    public int ID;
    public string name;
    public bool simulationMode = false;
    public bool demoSimulation = false;
    public string realValue;
    public string simulationValue = "0";
    public string min;
    public string max;

    #region robotic_arm_simulation
    public SimulatedJoint simulatedJoint;
    public void EnableSimulationMode()
    {
        if (ID != 0)
            return;
        Debug.Log("Simulation enabled");
        simulationMode = true;
        
        simulatedJoint.target =  float.Parse(simulationValue);

    }

    public void DisableSimulationMode()
    {
        if (ID != 0)
            return;
        Debug.Log("Simulation disabled");
        simulationMode = false;
        simulatedJoint.target = float.Parse(realValue);

    }
    #endregion
}

public class Interactable : MonoBehaviour
{
    public bool DEBUG_OpenTab;
    public GameObject UICanvasPrefab;
    public Material defaultMaterial;
    public Material onSelectMaterial;
    public GameObject DigitalTwinRoot;

    public bool canOpenUI = false;
    public bool isUIOpen = false;
    bool hasUIBeenInit = false;
    GameObject UICanvas;
    [SerializeField] string componentName;
    [SerializeField] string componentDesc;
    public int ComponentID = 0;
    public List<Value> valuesList;
    InteractableUIValues UI;
    SimulatedJoint simulatedJoint;
    [SerializeField]
    float simulationTarget = 0;

    private GameObject[] edgeCubes;
    int componentNbr;

    public bool DebugUpdateRealValue = false;
    public string ServerIP;
    public string SetComponentRequest;

    private void Start()
    {
        PressableButton pressableButton = GetComponent<PressableButton>();
        pressableButton.OnClicked.AddListener(ToggleUI);
        pressableButton.IsRayHovered.OnEntered.AddListener(OnHoverEnter);
        pressableButton.IsRayHovered.OnExited.AddListener(OnHoverExit);
        gameObject.tag = "Interactable";
    }

    private void Update()
    {
        if (DEBUG_OpenTab)
        {
            DEBUG_OpenTab = false;
            ToggleUI();
        }
        
        if (DebugUpdateRealValue)
        {
            StartCoroutine(RequestValueUpdateToAAS("-42"));
            DebugUpdateRealValue = false;
        }

    }

    public void ToggleUI()
    {
        if (isUIOpen)
        {
            UICanvas.SetActive(false);
            isUIOpen = false;
            GetComponent<Renderer>().material = defaultMaterial;
            return;
        }
        if (!canOpenUI)
            return;
        DigitalTwinRoot.GetComponent<XRDigitalTwin>().CloseEveryInteractableUI();
        isUIOpen = true;
        Debug.Log("Opening UI");
        GetComponent<Renderer>().material = onSelectMaterial;
        if (!hasUIBeenInit)
        {
            InitUI();
            hasUIBeenInit = true;
        }
        else
        {
            UICanvas.SetActive(true);
        }
        UI.SetName(componentName);
        UI.SetDesc(componentDesc);

        int i = 0;
        foreach (Value value in valuesList)
        {
            UI.SetValue(i, value, value.realValue);
            i++;
        }
    }

    public void InitValues(JToken jsonComponentData, int _componentNbr)
    {

        componentName = (string)AASParsing.FindTokenInToken(jsonComponentData, "Name");
        componentDesc = (string)AASParsing.FindTokenInToken(jsonComponentData, "Description");
        JToken valuesToken = AASParsing.FindTokenInToken(jsonComponentData, "Values");
        valuesList = new List<Value>();
        List<JToken> JsonvaluesList = valuesToken.ToList();
        componentNbr = _componentNbr;
        int ID = 0;
        foreach (JToken value in JsonvaluesList)
        {
            string valueName = (string)value.SelectToken("idShort");
            string valueValue = (string)AASParsing.FindTokenInCollection(value.SelectToken("value"), "CurrentValue")
                .SelectToken("value");
            JToken metadataToken = AASParsing.FindTokenInCollection(value.SelectToken("value"), "Metadata")
                .SelectToken("value");
            Value newValue = new Value();
            newValue.ID = ID;
            newValue.name = valueName;
            newValue.realValue = valueValue;

            JToken minValue = AASParsing.FindTokenInCollection(metadataToken, "MinValue");
            if (minValue != null)
                newValue.min = (string) minValue.SelectToken("value");
            JToken maxValue = AASParsing.FindTokenInCollection(metadataToken, "MaxValue");
            if (maxValue != null)
                newValue.max = (string)maxValue.SelectToken("value");

            JToken demoSimulation = AASParsing.FindTokenInCollection(metadataToken, "DemoSimulation");
            if (demoSimulation != null)
                newValue.demoSimulation = (bool)demoSimulation.SelectToken("value");


            if (minValue != null && float.Parse(newValue.realValue.Replace(".", ",")) < float.Parse(newValue.min))
                newValue.realValue = newValue.min;
            if (maxValue != null && float.Parse(newValue.realValue.Replace(".", ",")) > float.Parse(newValue.max))
                newValue.realValue = newValue.max;
            newValue.simulationValue = newValue.realValue;
            valuesList.Add(newValue); 
            ID++;
        }
    }

    public void SetName(string newName)
    {
        componentName = newName;
        if (UI && UICanvas.activeInHierarchy)
            UI.SetName(componentName);
    }

    public void SetDesc(string newDesc)
    {
        componentDesc = newDesc;
        if (UI && UICanvas.activeInHierarchy)
            UI.SetDesc(componentDesc);
    }

    /*Called when value is set from AAS server*/
    public void SetRealValueFor(string valueName, string valueValue)
    {
        int valueID = 0;
        for (; valueID < valuesList.Count; valueID++)
        {
            if (valuesList[valueID].name == valueName)
            {
                valuesList[valueID].realValue = valueValue;
                break;
            }
        }
        if (valuesList[valueID].simulationMode) { return; }
        valuesList[valueID].simulationValue = valuesList[valueID].realValue;
        if (UI && UICanvas.activeInHierarchy)
            UI.SetValue(valueID, valuesList[valueID], valueValue);
        if (valueID != 0 || !valuesList[valueID].demoSimulation) { return; } //Only the first value is used for the simulated joint
        if (simulatedJoint == null)
        {
            try
            {
                simulatedJoint = GetComponent<SimulatedJoint>();
                valuesList[valueID].simulatedJoint = simulatedJoint;
            }
            catch
            {
                return;
            }
        }
        simulatedJoint.target = float.Parse(valueValue);
    }

    #region input_field_value
    /*Called when value is set from input field during simulation mode*/
    public void SetSimulationValueFor(string valueName, string valueValue)
    {
        int valueID = 0;
        for (; valueID < valuesList.Count; valueID++)
        {
            if (valuesList[valueID].name == valueName)
            {
                valuesList[valueID].simulationValue = valueValue;
                break;
            }
        }
        if (!valuesList[valueID].simulationMode) { return; }

        if (valueID != 0) { return; } //Only the first value is used for the simulated joint
        if (simulatedJoint == null)
        {
            try
            {
                simulatedJoint = GetComponent<SimulatedJoint>();
                valuesList[valueID].simulatedJoint = simulatedJoint;
            }
            catch
            {
                return;
            }
        }
        float newValue = float.Parse(valueValue);
        if (simulatedJoint.rotationAngle == SimulatedJoint.Angles.y)
            newValue = -newValue;
        if (newValue > simulatedJoint.maxValue)
            newValue = simulatedJoint.maxValue;
        else if (newValue < simulatedJoint.minValue)
            newValue = simulatedJoint.minValue;
        simulatedJoint.target = newValue;
        if (UI && UICanvas.activeInHierarchy)
            UI.SetValue(valueID, valuesList[valueID], newValue.ToString());
    }

    /*Called when value is updated from inputField and simulation mode is off*/
    IEnumerator RequestValueUpdateToAAS(string valueValue)
    {
        string URL = (ServerIP + SetComponentRequest).Replace("{componentID}", componentNbr.ToString()).Replace("{newValue}", valueValue);
        Debug.Log("Sending update request: " + URL);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(URL))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = URL.Split('/');
            int page = pages.Length - 1;


            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
            yield return webRequest.downloadHandler.text;
        }
        yield return null;
    }

    /*Called by the interactable value when the input field value change*/
    public void HandleInputValue(string valueName, string valueValue)
    {
        int valueID = 0;
        for (; valueID < valuesList.Count; valueID++)
        {
            if (valuesList[valueID].name == valueName)
            {
                break;
            }
        }

        if (valuesList[valueID].simulationMode)
        {
            SetSimulationValueFor(valueName, valueValue);
        }
        else
        {
            StartCoroutine(RequestValueUpdateToAAS(valueValue));
        }
    }

    #endregion
    /*Enable simulation mode for the specified value*/
    public void EnableSimulationFor(string valueName)
    {
        for (int i = 0; i < valuesList.Count; i++)
        {
            if (valuesList[i].name == valueName)
            {

                #region robotic_arm_simulation
                if (simulatedJoint == null)
                {
                    try
                    {
                        simulatedJoint = GetComponent<SimulatedJoint>();
                        valuesList[i].simulatedJoint = simulatedJoint;
                    }
                    catch
                    {
                        Debug.LogError("could not set joint");
                        return;
                    }
                }
                #endregion
                valuesList[i].EnableSimulationMode();
                if (UI && UICanvas.activeInHierarchy)
                    UI.SetValue(i, valuesList[i], valuesList[i].simulationValue);
                break;
            }
        }
    }

    public void DisableSimulationFor(string valueName)
    {
        for (int i = 0; i < valuesList.Count; i++)
        {
            if (valuesList[i].name == valueName)
            {
                #region robotic_arm_simulation
                if (simulatedJoint == null)
                {
                    try
                    {
                        simulatedJoint = GetComponent<SimulatedJoint>();
                        valuesList[i].simulatedJoint = simulatedJoint;
                    }
                    catch
                    {
                        return;
                    }
                }
                #endregion
                valuesList[i].DisableSimulationMode();
                valuesList[i].simulationValue = valuesList[i].realValue;
                if (UI && UICanvas.activeInHierarchy)
                    UI.SetValue(i, valuesList[i], valuesList[i].realValue);
                break;
            }
        }
    }


    public void InitUI()
    {
        UICanvas = GameObject.Instantiate(UICanvasPrefab);
        UICanvas.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.2f);
        UI = UICanvas.GetComponent<InteractableUIValues>();
        UI.associatedGameObject = this.GameObject();
    }

    public List<Value> GetValues()
    {
        return valuesList;
    }
    
    public void SwitchMaterial(Material bydefault, Material selected)
    {
        defaultMaterial = bydefault;
        onSelectMaterial = selected;
        if (isUIOpen)
            GetComponent<Renderer>().material = onSelectMaterial;
        else
            GetComponent<Renderer>().material = defaultMaterial;

    }

    void OnHoverEnter(float value)
    {
        Debug.Log("hovered");
        if (canOpenUI && !isUIOpen)
            GetComponent<Renderer>().material = onSelectMaterial;
    }

    void OnHoverExit(float value)
    {
        Debug.Log("hovered");
        if (canOpenUI && !isUIOpen)
            GetComponent<Renderer>().material = defaultMaterial;
    }
}