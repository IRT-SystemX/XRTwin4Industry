using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;
using GLTFast;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;
using MixedReality.Toolkit.UX;
using System.Security.Policy;
using static AASClient;

public class AASClient : MonoBehaviour
{
    public string serverUrl;
    [SerializeField] GameObject XRDTPrefab;

    public DialogPool DialogPool;

    public class DigitalTwinData
    {
        public string assetID;
        public string assetIDshort;
        public string xRDataSubmodelID;
        public JToken xRDataSubmodelContent;
        public string model3DID;
        public JToken model3DContent;


        public bool enabled = true;

        public DigitalTwinData(string assetID, string assetIdShort, string xRDataSubmodelID, JToken xRDataSubmodelContent,
            string model3DID, JToken model3DContent)
        {
            this.assetID = assetID;
            this.assetIDshort = assetIdShort;
            this.xRDataSubmodelID = xRDataSubmodelID;
            this.xRDataSubmodelContent = xRDataSubmodelContent;
            this.model3DID = model3DID;
            this.model3DContent = model3DContent;
        }
    }

    public List<DigitalTwinData> DTIDList;
    Dictionary<string, GameObject> XRDTs; /*xRDataSubmodelID as key and GameObject as value to allow the use of dictionnary to update components*/

    void Awake()
    {
        // Bypass SSL certificate validation
        ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;
        if (DialogPool == null)
        {
            DialogPool = GetComponent<DialogPool>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DTIDList = new List<DigitalTwinData>();
        XRDTs = new Dictionary<string, GameObject>();
        return;
        
    }

    public IEnumerator ConnectToServer(string _serverUrl)
    {
        serverUrl = _serverUrl;
        object rawAssetList = null;
        yield return Run<string>(GetAssetList(), (output) => rawAssetList = output);
        JToken assetListJson = JObject.Parse((string)rawAssetList).SelectToken("result");
        foreach (JToken asset in assetListJson) /*Look for assets with XRData and Model3D submodels, meaning they can be used in the app*/
        {
            string XRDataID = "";
            JToken XRDataContent = null;
            string Model3DID = "";
            JToken Model3DContent = null;
            JToken submodels = asset.SelectToken("submodels");
            if (submodels == null)
                continue;
            foreach (JToken submodel in asset.SelectToken("submodels"))
            {
                string submodelID = Convert.ToBase64String(Encoding.UTF8.GetBytes((submodel.SelectToken("keys[0].value")).ToString()));
                object submodelRaw = null;
                yield return Run<string>(GetSubmodel(serverUrl, submodelID), (output) => submodelRaw = output);
                if (submodelRaw == null)
                    continue;
                JToken submodelJson = JObject.Parse((string)submodelRaw);
                string idShort = (string)submodelJson.SelectToken("idShort");
                if (idShort == "XRData")
                {
                    XRDataID = submodelID;
                    XRDataContent = submodelJson;
                }
                if (idShort == "Model3D")
                {
                    Model3DID = submodelID;
                    Model3DContent = submodelJson;
                }

            }
            if (XRDataContent != null && Model3DContent != null)
            {
                if (XRDataID.EndsWith("=="))
                    XRDataID = XRDataID.Remove(XRDataID.Length - 2);
                if (Model3DID.EndsWith("=="))
                    Model3DID = Model3DID.Remove(Model3DID.Length - 2);
                string assetID = (string)asset.SelectToken("id");
                string assetIDShort = (string)asset.SelectToken("idShort");
                DTIDList.Add(new DigitalTwinData(assetID, assetIDShort, XRDataID, XRDataContent, Model3DID, Model3DContent));
            }
        }
    }

    IEnumerator GetAssetList()
    {
        string url = serverUrl + "/shells";
        Debug.Log(url);


        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();


            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Server returned an error-" + webRequest.error);
                    IDialog dialog = DialogPool.Get()
                    .SetHeader("")
                    .SetBody("Connection to server failed")
                    .SetNeutral("Ok", (args) => Debug.Log("Code-driven dialog says " + args.ButtonType));
                    dialog.Show();
                    yield return null;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Server returned an error!" 
                        + webRequest.error);
                    IDialog dialogProtocol = DialogPool.Get()
                    .SetHeader("")
                    .SetBody("Connection to server failed")
                    .SetNeutral("Ok", (args) => Debug.Log("Code-driven dialog says " + args.ButtonType));
                    dialogProtocol.Show();
                    yield return null;
                    break;
                case UnityWebRequest.Result.Success:
                    yield return webRequest.downloadHandler.text;
                    break;
            }
        }
    }

    public IEnumerator GetSubmodel(string _serverURL, string submodelURL)
    {
        string url = _serverURL + "/submodels/" + submodelURL;
        Debug.Log(url);


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
                    yield return webRequest.downloadHandler.text;
                    break;
            }
        }
    }

    public void SetDTDataList(List<DigitalTwinData> newDTIDList)
    {
        DTIDList = newDTIDList;
    }

    public IEnumerator InitDTs()
    {
        Vector3 spawnPos = new Vector3(0, 1, 0.5f);
        foreach (DigitalTwinData DTID in DTIDList)
        {
            yield return StartCoroutine(InitDT(DTID, spawnPos));
            spawnPos.x += 1;
        }
        MQTTClient.serverIP = serverUrl;
        yield return MQTTClient.Connect_Client_Using_WebSockets(XRDTs);

    }

    IEnumerator InitDT(DigitalTwinData DTData, Vector3 spawnPos)
    {
        GameObject XRDT = GameObject.Instantiate(XRDTPrefab);
        XRDT.SetActive(true);
        XRDT.transform.position = spawnPos;
        XRDT.GetComponent<XRDigitalTwin>().ID = DTData.assetID;
        XRDTs.Add(DTData.xRDataSubmodelID, XRDT); //XRData submodel ID is used as key
        yield return StartCoroutine(UpdateDT(DTData));
        yield return XRDT;
    }

    IEnumerator InitDTWithCallback(DigitalTwinData DTData, Vector3 spawnPos, System.Action<GameObject> callback)
    {
        if (XRDTs.ContainsKey(DTData.xRDataSubmodelID))
        {
            XRDTs[DTData.xRDataSubmodelID].transform.position = spawnPos;
            callback?.Invoke(null);
        }
        else
        {
            GameObject XRDT = GameObject.Instantiate(XRDTPrefab);
            XRDT.SetActive(true);
            XRDT.transform.position = spawnPos;
            XRDT.GetComponent<XRDigitalTwin>().ID = DTData.assetID;

            XRDTs.Add(DTData.xRDataSubmodelID, XRDT); //XRData submodel ID is used as key
            yield return StartCoroutine(UpdateDT(DTData));
            Debug.Log("Sending callback");
            callback?.Invoke(XRDT);
        }
    }


    IEnumerator UpdateDT(DigitalTwinData DTData)
    {
        yield return StartCoroutine(XRDTs[DTData.xRDataSubmodelID].GetComponent<XRDigitalTwin>().UpdateCompleteDT((JObject)DTData.model3DContent,
            (JObject)DTData.xRDataSubmodelContent));
    }

    /*To be able to get the coroutine last return value*/
    public static IEnumerator Run<T>(IEnumerator target, Action<T> output)
    {
        object result = null;
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        output((T)result);
    }

    public void InstantiateDTFromAssetData(DigitalTwinData assetData, Vector3 position, System.Action<GameObject> callback)
    {
        Debug.Log("coroutine starting");
        StartCoroutine(InitDTWithCallback(assetData, position, callback));
        Debug.Log("Coroutine started");
    }

}
