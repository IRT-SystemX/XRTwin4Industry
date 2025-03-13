using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit;
using UnityEngine.XR.ARFoundation;
using Unity.VisualScripting;
using Microsoft.MixedReality.OpenXR.ARFoundation;
using Microsoft.MixedReality.OpenXR.ARSubsystems;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using UnityEngine.XR.WSA;

public class anchorManipulator : MonoBehaviour
{
    ARAnchor anchor;
    public ARAnchorManager aRManager;
    XRAnchorStore xRAnchorStore;
    

    // Start is called before the first frame update
    async void Start()
    {
        await Task.Delay(1000);
        Debug.Log("starting start");
        xRAnchorStore = await XRAnchorStore.LoadAnchorStoreAsync(aRManager.subsystem);
        Debug.Log("store loaded");
        foreach (string anchorName in xRAnchorStore.PersistedAnchorNames)
        {
            Debug.Log("found " + anchorName + " in store");
            if (anchorName == "cube")
            {
                TrackableId trackableId = xRAnchorStore.LoadAnchor("cube");
                anchor = this.AddComponent<ARAnchor>();
                anchor.transform.position = new Vector3( 
                    PlayerPrefs.GetFloat("cube" + "_posX"),
                    PlayerPrefs.GetFloat("cube" + "_posY"),
                    PlayerPrefs.GetFloat("cube" + "_posZ"));
                anchor.transform.rotation = new Quaternion(
                     PlayerPrefs.GetFloat("cube" + "_rotX"),
                     PlayerPrefs.GetFloat("cube" + "_rotY"),
                     PlayerPrefs.GetFloat("cube" + "_rotZ"),
                     PlayerPrefs.GetFloat("cube" + "_rotW"));
                Debug.Log("anchor found with this name");
                Debug.Log("anchor pos3 = " + anchor.transform.position.ToString());
                /*transform.position = anchor.transform.position;
                transform.rotation = anchor.transform.rotation;*/
                transform.SetParent(anchor.transform, true);
                Debug.Log("obj pos: " + transform.position.ToString());
                return;
            }
        }
        Debug.Log("no cube anchor found");
        anchor = this.AddComponent<ARAnchor>();
        Debug.Log("anchor pos = " + anchor.transform.position.ToString());
        RemoveAnchor();

    }

    public void RemoveAnchor()
    {
        Debug.Log("anchor pos = " + anchor.transform.position.ToString());
        xRAnchorStore.UnpersistAnchor("cube");
        Destroy(anchor);
        anchor = null;
        StartCoroutine(RecreateAnchor());
    }

    IEnumerator RecreateAnchor()
    {
        yield return null;
        anchor = this.AddComponent<ARAnchor>();

        xRAnchorStore.TryPersistAnchor(anchor.trackableId, "cube");
        PlayerPrefs.SetFloat("cube" + "_posX", anchor.transform.position.x);
        PlayerPrefs.SetFloat("cube" + "_posY", anchor.transform.position.y);
        PlayerPrefs.SetFloat("cube" + "_posZ", anchor.transform.position.z);
        PlayerPrefs.SetFloat("cube" + "_rotX", anchor.transform.rotation.x);
        PlayerPrefs.SetFloat("cube" + "_rotY", anchor.transform.rotation.y);
        PlayerPrefs.SetFloat("cube" + "_rotZ", anchor.transform.rotation.z);
        PlayerPrefs.SetFloat("cube" + "_rotW", anchor.transform.rotation.w);
        PlayerPrefs.Save();
        foreach (string anchorName in xRAnchorStore.PersistedAnchorNames)
        {

            if (anchorName == "cube")
            {
                Debug.Log("Anchor stored sucessfully");
            }
        }
    }
}
