using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class AASParsing : MonoBehaviour
{

    /*Find in submodel*/
    public static JToken FindTokenInSubmodel(JObject json, string propertyPath)
    {
        string[] propertyPathArray = propertyPath.Split('.');
        List<string> propertyPathList = new List<string>();
        for (int i = 0; i < propertyPathArray.Length; i++)
        {
            propertyPathList.Add(propertyPathArray[i]);
        }
        return (FindTokenRecursive(json.SelectToken("submodelElements"), propertyPathList));
    }

    public static JToken FindTokenInToken(JToken token, string propertyPath)
    {
        string[] propertyPathArray = propertyPath.Split('.');
        List<string> propertyPathList = new List<string>();
        for (int i = 0; i < propertyPathArray.Length; i++)
        {
            propertyPathList.Add(propertyPathArray[i]);
        }
        return (FindTokenRecursive(token, propertyPathList));
    }

    static JToken FindTokenRecursive(JToken jsonToken, List<string> jsonPath)
    {
        if (jsonPath.Count== 0)
            return jsonToken;

       
        List<JToken> valuesList = jsonToken.ToList();

        for (int i = 0; i < valuesList.Count; i++) {
            string idShort = (string)valuesList[i].SelectToken("idShort");
            if (idShort == jsonPath[0]) {
                JToken newToken = valuesList[i].SelectToken("value");
                jsonPath.RemoveAt(0);
                return FindTokenRecursive(newToken, jsonPath);
            }
        }
        Debug.LogWarning("couldn't find property " + jsonPath);
        return null;
    }

    public static JToken FindTokenInCollection(JToken jsonToken, string tokenToFind)
    {
        if (jsonToken == null || jsonToken.HasValues == false)
            return null;
        foreach (JToken subtoken in jsonToken)
        {
            if ((string)subtoken.SelectToken("idShort") == tokenToFind)
            {
                return subtoken;
            }
        }
        Debug.LogWarning("couldn't find property " + tokenToFind);
        //Debug.LogError("item " + tokenToFind + " not found in " + jsonToken);
        return null;
    }
}
