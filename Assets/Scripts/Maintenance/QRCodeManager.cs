using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.OpenXR;
using Newtonsoft.Json.Linq;
using MixedReality.Toolkit.UX;
using System.Security.Policy;
using UnityEngine.Networking;
using UnityEngine.XR.ARSubsystems;
using System.Text.RegularExpressions;
using GLTFast.Schema;

public class QRCodeManager : MonoBehaviour
{
    ARMarkerManager m_arMarkerManager;
    public GameObject instructionPrefab;
    public GameObject procedureSelectionPrefab;
    public PlayerPositionGiver playerPositionGiver;
    Dictionary<TrackableId, GameObject> procedureSelectionDict = new Dictionary<TrackableId, GameObject>();

    private void Awake()
    {
        m_arMarkerManager = GetComponent<ARMarkerManager>();
        m_arMarkerManager.markersChanged += OnQRCodesChanged;
        foreach(ARMarker previousQR in m_arMarkerManager.trackables)
        {
            //previousQR.enabled = false;
        }
        Debug.Log("QR code manager init");
    }

    public (string BaseUrl, string EncodedPart) ParseURL(string url)
    {
        // Regex to match the base URL and the encoded part
        var regexBaseUrl = new Regex(@"http://[^/]+"); // Matches base URL
        var regexEncodedPart = new Regex(@"(?<=shells/)[A-Za-z0-9]+"); // Matches encoded part after 'shells/'

        // Extract base URL
        var baseUrlMatch = regexBaseUrl.Match(url);
        if (!baseUrlMatch.Success) throw new ArgumentException("Base URL not found.");

        // Extract encoded part
        var encodedPartMatch = regexEncodedPart.Match(url);
        if (!encodedPartMatch.Success) throw new ArgumentException("Encoded part not found.");

        // Return both parts
        return (baseUrlMatch.Value, encodedPartMatch.Value);
    }

    void OnQRCodesChanged(ARMarkersChangedEventArgs args)
    {
        Debug.Log("On QR code change");
        foreach (ARMarker qrCode in args.added)
        {
            Debug.Log($"QR code with the ID {qrCode.trackableId} added.");
            Debug.Log($"Pos:{qrCode.transform.position} Rot:{qrCode.transform.rotation} Size:{qrCode.size}");
            //qrCode.transform.rotation = Quaternion.Euler(0, 0, 0);
            qrCode.destroyOnRemoval = false;
            string decodedString = qrCode.GetDecodedString();
            if (!decodedString.StartsWith("Maintenance procedure : "))
                continue;
            Debug.Log("decoded string : " + decodedString);
            var (baseUrl, encodedPart) = ParseURL(decodedString);
            Debug.Log("base url :" + baseUrl);
            Debug.Log("encoded part :" + encodedPart);
            GameObject procedureSelection = Instantiate(procedureSelectionPrefab);
            //procedureSelection.transform.position = qrCode.transform.position;
            procedureSelection.transform.rotation = qrCode.transform.rotation * Quaternion.Euler(new Vector3(180, 0, 0.0f));
            procedureSelection.transform.position = qrCode.transform.TransformPoint(new Vector3(0, 0.05f, 0.0f));
            if (procedureSelectionDict.ContainsKey(qrCode.trackableId))
                procedureSelectionDict.Add(qrCode.trackableId, procedureSelection);
            
            //URL = "aHR0cHM6Ly9leGFtcGxlLmNvbS9pZHMvYWFzLzIzNzVfMzE4MV8xMTQyXzQ5OTQ"; //AssetID, change QR Later
            procedureSelection.GetComponent<ProcedureSelection>().ShowProceduresList(baseUrl, encodedPart, qrCode);
            playerPositionGiver.QRCode = qrCode.transform.gameObject;
        }

        foreach (ARMarker qrCode in args.removed)
            Debug.Log($"QR code with the ID {qrCode.trackableId} removed.");

        foreach (ARMarker qrCode in args.updated)
        {
            Debug.Log($"QR code with the ID {qrCode.trackableId} updated.");
            //qrCode.transform.rotation = Quaternion.Euler(0, 0, 0);

            Debug.Log($"Pos:{qrCode.transform.position} Rot:{qrCode.transform.rotation} Size:{qrCode.size}");
            if (qrCode != null && procedureSelectionDict.ContainsKey(qrCode.trackableId))
            {
                procedureSelectionDict[qrCode.trackableId].transform.rotation = qrCode.transform.rotation * Quaternion.Euler(new Vector3(180, 0, 0.0f));
                procedureSelectionDict[qrCode.trackableId].transform.position = qrCode.transform.TransformPoint(new Vector3(0, 0.05f, 0.0f));
            } else
            {
                //qrCode.gameObject.SetActive(false);
            }
        }
    }

}
