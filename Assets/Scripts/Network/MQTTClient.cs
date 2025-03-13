using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Extensions.WebSocket4Net;
using MQTTnet.Formatter;
using System.Threading.Tasks;
using MQTTnet.Packets;
using Newtonsoft.Json.Linq;

public class MQTTClient : MonoBehaviour
{
    static MqttFactory mqttFactory;
    static IMqttClient mqttClient;
    static Dictionary<string, GameObject> staticDTs;
    static List<MqttApplicationMessage> messagesToManage;
    public static string serverIP;

    static bool clientInit = false;

    void Update()
    {
        //Debug.Log("update here");
        if (messagesToManage != null && messagesToManage.Count > 0)
        {
            ManageIncomingMessage(messagesToManage[0], staticDTs);
            messagesToManage.RemoveAt(0);
        }
    }

    static async Task InitMQTTClient()
    {
        messagesToManage = new List<MqttApplicationMessage>();
        mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            Debug.Log("[MQTT] Received application message.");
            Debug.Log("[MQTT]" + e.ApplicationMessage.ConvertPayloadToString() + " " + e.ApplicationMessage.Topic);
            messagesToManage.Add(e.ApplicationMessage);
            return Task.CompletedTask;
        };
        int indexOfSlash = serverIP.LastIndexOf('/');
        int indexOfPoints = serverIP.LastIndexOf(':');
        
        Debug.Log("Connecting to mqtt server " + serverIP.Substring(indexOfSlash + 1, indexOfPoints - indexOfSlash - 1));
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(serverIP.Substring(indexOfSlash + 1, indexOfPoints - indexOfSlash - 1), 1884).Build();
        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        Debug.Log("[MQTT] The MQTT client is connected.");

        Debug.Log("[MQTT]" + response);
        if (staticDTs == null)
            staticDTs = new Dictionary<string, GameObject>();
    }

    public static async Task Connect_Client_Using_WebSockets(Dictionary<string, GameObject> DTs)
    {
        /*
         * This sample creates a simple MQTT client and connects to a public broker using a WebSocket connection.
         * 
         * This is a modified version of the sample _Connect_Client_! See other sample for more details.
         */

        if (!clientInit)
        {
            await InitMQTTClient();
            clientInit = true;
        }

        foreach (var kvp in DTs)
        {
            staticDTs.Add(kvp.Key, kvp.Value);
        }


        foreach (KeyValuePair<string, GameObject> XRDT in staticDTs)
        {
            Debug.Log("[MQTT] adding DT to subscription");
            XRDigitalTwin XRDTScript = XRDT.Value.GetComponent<XRDigitalTwin>();
            
            foreach (KeyValuePair<string, GameObject> interactableComponent in XRDTScript.GetInteractableComponents())
            {
                MqttClientSubscribeOptions nameSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("sm-repository/sm-repo/submodels/"+ XRDT.Key + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Name/updated");
                    })
                .Build();
                var subscribeResponse = await mqttClient.SubscribeAsync(nameSubscribeOptions, CancellationToken.None);
                MqttClientSubscribeOptions DescSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("sm-repository/sm-repo/submodels/"+ XRDT.Key + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Description/updated");
                    })
                .Build();
                subscribeResponse = await mqttClient.SubscribeAsync(DescSubscribeOptions, CancellationToken.None);
                int i = 0;
                foreach (Value value in 
                        interactableComponent.Value.GetComponent<Interactable>().GetValues())
                {
                    MqttClientSubscribeOptions ValueSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                    f =>
                    {
                        Debug.Log("subscribing to " + "sm-repository/sm-repo/submodels/" + XRDT.Key + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Values." + value.name + ".CurrentValue/updated");
                        f.WithTopic("sm-repository/sm-repo/submodels/" + XRDT.Key + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Values." + value.name + ".CurrentValue/updated");
                    })
                    .Build();
                    subscribeResponse = await mqttClient.SubscribeAsync(ValueSubscribeOptions, CancellationToken.None);
                    i++;
                }
            }
        }
    }

   public static async Task SubscribeDT(string assetID, GameObject DT)
    {
        Debug.Log("in subscribe DT");
        if (!clientInit)
        {
            await InitMQTTClient();
            clientInit = true;
        }
        
        staticDTs.Add(assetID, DT);
        foreach (KeyValuePair<string, GameObject> interactableComponent in DT.GetComponent<XRDigitalTwin>().GetInteractableComponents())
        {
            MqttClientSubscribeOptions nameSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic("sm-repository/sm-repo/submodels/" + assetID+ "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Name/updated");
                })
            .Build();
            var subscribeResponse = await mqttClient.SubscribeAsync(nameSubscribeOptions, CancellationToken.None);
            MqttClientSubscribeOptions DescSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic("sm-repository/sm-repo/submodels/" + assetID + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Description/updated");
                })
            .Build();
            subscribeResponse = await mqttClient.SubscribeAsync(DescSubscribeOptions, CancellationToken.None);
            int i = 0;
            foreach (Value value in
                    interactableComponent.Value.GetComponent<Interactable>().GetValues())
            {
                MqttClientSubscribeOptions ValueSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                f =>
                {
                    Debug.Log("subscribing to " + "sm-repository/sm-repo/submodels/" + assetID + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Values." + value.name + ".CurrentValue/updated");
                    f.WithTopic("sm-repository/sm-repo/submodels/" + assetID + "/submodelElements/InteractableComponents." + interactableComponent.Key + ".Values." + value.name + ".CurrentValue/updated");
                })
                .Build();
                subscribeResponse = await mqttClient.SubscribeAsync(ValueSubscribeOptions, CancellationToken.None);
                i++;
            }
        }
    }

    static void ManageIncomingMessage(MqttApplicationMessage message, Dictionary<string, GameObject> DTs)
    {
        string[] URL =  message.Topic.Split('/');
        string DTID = URL[3];
        string[] relativePath = URL[5].Split('.');
        string componentNbr = relativePath[1];
        JObject messagejson = JObject.Parse(message.ConvertPayloadToString());
        Debug.Log("receive mqqt message " + message.Topic);

        try
        {
            switch (relativePath[2])
            {
                case "Name":
                    string newName = (string)messagejson.SelectToken("value");
                    DTs[DTID].GetComponent<XRDigitalTwin>().GetInteractableComponents()[componentNbr].
                        GetComponent<Interactable>().SetName(newName);
                    break;
                case "Description":
                    string newDesc = (string)messagejson.SelectToken("value");
                    DTs[DTID].GetComponent<XRDigitalTwin>().GetInteractableComponents()[componentNbr].
                        GetComponent<Interactable>().SetDesc(newDesc);
                    break;
                case "Values":
                    if (relativePath[4] != "CurrentValue") { break; } //We only manage Current_value updates
                    string valueName = relativePath[3]; 
                    string newValue = (string)messagejson.SelectToken("value");
                    DTs[DTID].GetComponent<XRDigitalTwin>().GetInteractableComponents()[componentNbr].
                        GetComponent<Interactable>().SetRealValueFor(valueName,newValue);
                    Debug.Log("Calling set value");
                    break;
                default:
                    throw new Exception("Invalid property");
            }
        }
        catch (Exception)
        {
            Debug.LogWarning("Received invalid MQTT message. Message will be ignored.\n"+  message.Topic + "\n" + message.ConvertPayloadToString() );
            throw;
        }
    }
}
