using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static AASClient;

public class DTButton : MonoBehaviour
{
    AASClient aasClient;
    public InstructionManager instructionManager;

    // Start is called before the first frame update
    void Start()
    {
        aasClient = GameObject.FindGameObjectWithTag("AASClient").GetComponent<AASClient>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadDT()
    {
        GameObject xrDT = null;
        if (instructionManager == null)
            instructionManager = transform.parent.parent.GetComponent<InstructionManager>();
        if (instructionManager == null)
            Debug.Log("null instruction manager");
        if (instructionManager.qrCode == null)
            Debug.Log("null qr code");
        DigitalTwinData digitalTwinData = instructionManager.digitalTwinData;
        aasClient.InstantiateDTFromAssetData(digitalTwinData, transform.position, (_xrDt) => {
            if (_xrDt != null)
            {
                Debug.Log("Instantiated Digital Twin: " + _xrDt.name);
                xrDT = _xrDt;
                StartCoroutine(LoadDTCouroutine(digitalTwinData, xrDT));
            } else
            {
                Debug.Log("DT already loaded");
            }
        });
    }

    IEnumerator LoadDTCouroutine(DigitalTwinData digitalTwinData, GameObject xrDT)
    {
        Debug.Log("calling subscribe DT");
        MQTTClient.serverIP = "http://192.168.209.236:8081";
        yield return MQTTClient.SubscribeDT(digitalTwinData.xRDataSubmodelID, xrDT);
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
