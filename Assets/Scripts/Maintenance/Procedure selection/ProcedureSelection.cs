using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.UX;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.VirtualTexturing;
using static AASClient;

public class ProcedureSelection : MonoBehaviour
{
    public GameObject procedureItemPrefab;
    public GameObject procedureInstructionsPrefab;
    ARMarker qrCode;
    string assetID;
    string serverURL;
    AASClient aASClient;

    public void ShowProceduresList(string _serverURL, string _assetID, ARMarker _qrCode)
    {
        aASClient = GameObject.FindGameObjectWithTag("AASClient").GetComponent<AASClient>();
        qrCode = _qrCode;
        serverURL = _serverURL;
        assetID = _assetID;
        Debug.Log("GameObject: " + _qrCode.gameObject.name);
        StartCoroutine(ShowProceduresListCoroutine());
        
        
    }
     IEnumerator ShowProceduresListCoroutine()
    {
        JToken procedures;

        object rawAssetList = null;
        yield return Run<string>(GetAsset(), (output) => rawAssetList = output);
        JToken submodels = JObject.Parse((string)rawAssetList).SelectToken("result");
        Debug.Log("submodels :" + submodels.ToString());
        string XRDataID = "";
        JToken XRDataContent = null;
        string Model3DID = "";
        JToken Model3DContent = null;
        string MaintenanceDataID = "";
        JToken MaintenanceDataContent = null;
        DigitalTwinData digitalTwinData = null;
        foreach (JToken submodel in submodels)
        {
            string submodelID = Convert.ToBase64String(Encoding.UTF8.GetBytes((submodel.SelectToken("keys[0].value")).ToString()));
            object submodelRaw = null;
            Debug.Log("Subdmodel ID : " + submodelID);
            yield return Run<string>(aASClient.GetSubmodel("http://192.168.209.236:8081", submodelID), (output) => submodelRaw = output);
            if (submodelRaw == null)
                continue;
            JToken submodelJson = JObject.Parse((string)submodelRaw);
            string idShort = (string)submodelJson.SelectToken("idShort");
            if (idShort == "XRData")
            {
                XRDataID = submodelID;
                XRDataContent = submodelJson;
            }
            else if (idShort == "Model3D")
            {
                Model3DID = submodelID;
                Model3DContent = submodelJson;
            }
            else if (idShort == "ARMaintenanceProcedures")
            {
                MaintenanceDataID = submodelID;
                MaintenanceDataContent = submodelJson;
                Debug.Log("found maintenance");
                Debug.Log(MaintenanceDataContent);
            }

        }
        if (XRDataContent != null && Model3DContent != null)
        {
            if (XRDataID.EndsWith("=="))
                XRDataID = XRDataID.Remove(XRDataID.Length - 2);
            if (Model3DID.EndsWith("=="))
                Model3DID = Model3DID.Remove(Model3DID.Length - 2);
            Debug.Log("asset have everything");
            digitalTwinData = new DigitalTwinData("test", "test", XRDataID, XRDataContent, Model3DID, Model3DContent);
            Debug.Log("Maintenance sm content:" + MaintenanceDataContent);
            //DTIDList.Add(new DigitalTwinData(assetID, assetIDShort, XRDataID, XRDataContent, Model3DID, Model3DContent));
            //aASClient.InstantiateDTFromAssetData(digitalTwinData, qrCode.transform.position);
        }
        procedures = MaintenanceDataContent.SelectToken("submodelElements").First.
            SelectToken("value");
        
        //Display procedures
        Debug.Log("procedures: " + procedures.ToString());
        foreach (var procedure in procedures)
        {
            GameObject procedureItem = Instantiate(procedureItemPrefab, this.transform);
            Debug.Log("instantiating procedure");
            Debug.Log("procedure: " + procedure.ToString());
            procedureItem.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "test";
            Debug.Log("proc name : " + AASParsing.FindTokenInCollection(procedure.SelectToken("value"), "Name").SelectToken("value").ToString());
            procedureItem.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AASParsing.FindTokenInCollection(procedure.SelectToken("value"), "Name").SelectToken("value").ToString();
            procedureItem.transform.GetChild(1).GetComponent<PressableButton>().OnClicked.AddListener(delegate { StartProcedure(
                AASParsing.FindTokenInCollection(procedure.SelectToken("value"), "Instructions").SelectToken("value"), digitalTwinData); });
        }
    }

    public void StartProcedure(JToken procedure, DigitalTwinData digitalTwinData)
    {
        Debug.Log("starting procedure with instructions: " + procedure.ToString());
        GameObject instruction = GameObject.Instantiate(procedureInstructionsPrefab);
        InstructionManager instructionManager = instruction.GetComponent<InstructionManager>();
        if (instructionManager != null)
        {
            Debug.Log("found instruction manager");
        }
        instructionManager.Init(procedure, qrCode, digitalTwinData);
        instructionManager.SetProcedureSelectionObj(this.gameObject);
        this.gameObject.SetActive(false);
    }

    public IEnumerator GetAsset()
    {
        //string url = "http://192.168.209.236:8081/shells/" + assetID + "=/submodel-refs";
        string url = serverURL + "/shells/" + assetID + "=/submodel-refs";
        Debug.Log("URL:" + url);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();


            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Server returned an error" + webRequest.error);
                    yield return null;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Server returned an error"
                        + webRequest.error);
                    yield return null;
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Succesfully connected to server");
                    Debug.Log("asset :" + webRequest.downloadHandler.text);
                    yield return webRequest.downloadHandler.text;
                    break;
            }
        }
    }

    /*To be able to get the coroutine last return value*/
    public IEnumerator Run<T>(IEnumerator target, Action<T> output)
    {
        object result = null;
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        output((T)result);
    }

}
