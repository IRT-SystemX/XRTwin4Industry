using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Globalization;
using Microsoft.MixedReality.OpenXR;
using UnityEngine.Rendering.VirtualTexturing;
using System.Linq;
using UnityEngine.Video;
using System.Security.Policy;
using MixedReality.Toolkit.SpatialManipulation;
using static AASClient;

public class InstructionManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI textMeshPro;
    public UnityEngine.UI.Image image;
    public VideoPlayer videoPlayer;
    public GameObject obj;
    public ARMarker qrCode;
    public DigitalTwinData digitalTwinData;

    public GameObject prevButton;
    public GameObject nextButton;
    

    GameObject procedureSelectionObj;

    StatusIcon statusIcon;
    enum DownloadingState
    {
        completed,
        downloading,
        failed
    }
    DownloadingState textState = DownloadingState.downloading;
    DownloadingState illustrationState = DownloadingState.downloading;
    DownloadingState indicatorState = DownloadingState.downloading;
    DownloadingState globalState = DownloadingState.downloading;

    JToken currentProcedure = null;
    int currentStep = 0;
    List<GameObject> indicators = new List<GameObject>();
    RectTransform rectTransform;
    BoundBoxManager boundBoxManager = null;

    private void Update()
    {
        if (statusIcon != null)
        {
            if (textState == DownloadingState.failed || illustrationState == DownloadingState.failed
                || indicatorState == DownloadingState.failed)
            {
                if (globalState != DownloadingState.failed)
                {
                    statusIcon.SetState(StatusIcon.State.connection_lost);
                    globalState = DownloadingState.failed;
                }
            }
            else if (textState == DownloadingState.downloading || illustrationState == DownloadingState.downloading
                || indicatorState == DownloadingState.downloading)
            {
                if (globalState != DownloadingState.downloading)
                {
                    statusIcon.SetState(StatusIcon.State.downloading);
                    globalState = DownloadingState.downloading;
                }
            }
            else
            {
                if (globalState != DownloadingState.completed)
                {
                    statusIcon.SetState(StatusIcon.State.normal);
                    globalState = DownloadingState.completed;
                }
            }
        } else
        {
            Debug.Log("null status icon");
        }
    }

    public void Init(JToken procedure, ARMarker _qrCode, DigitalTwinData _digitalTwinData)
    {
        qrCode = _qrCode;
        statusIcon = _qrCode.gameObject.GetComponent<StatusIcon>();
        transform.position = qrCode.transform.position;
        DestroyIndicators();
        Debug.Log("Will start procedure");
        rectTransform = GetComponent<RectTransform>();
        digitalTwinData = _digitalTwinData;
        StartProcedure(procedure);
    }

    public IEnumerator GetMaintenanceInstructions(string URL, Action<string> onSuccess)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(URL))
        {
            yield return webRequest.SendWebRequest();


            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Server returned an error!"
                        + webRequest.error);
                    yield return null;
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Server returned an error!"
                        + webRequest.error);
                    yield return null;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Server returned an error!"
                        + webRequest.error);
                    yield return null;
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Got maintenance instructions successfully");
                    onSuccess?.Invoke(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    public void StartProcedure(JToken procedure)
    {
        DestroyIndicators();
        Debug.Log("instructions : " + procedure.ToString());
        Debug.Log(procedure.ToString());

        currentProcedure = procedure;
        LoadStep();
    }

    public void LoadNextStep()
    {
        prevButton.SetActive(true);
        nextButton.GetComponent<NextButton>().SetNormalSprite();
        if (currentStep + 1 < currentProcedure.ToList<JToken>().Count ){
            currentStep += 1;
            LoadStep();
            Debug.Log("current step : " + currentStep);
            Debug.Log("count : " + currentProcedure.ToList<JToken>().Count);
            if (currentStep + 1 >= currentProcedure.ToList<JToken>().Count)
                nextButton.GetComponent<NextButton>().SetValidateSprite();
        } else
        {
            EndProcedure();
        }
    }

    public void LoadPrevStep()
    {
        if (currentStep > 0)
        {
            currentStep -= 1;
            LoadStep();
        }
        if (currentStep == 0)
            prevButton.SetActive(false);
    }

    void LoadStep()
    {
        textState = DownloadingState.downloading;
        DestroyIndicators();
        JToken currentInstruction = currentProcedure[currentStep].SelectToken("value");
        Debug.Log("current instruction : " + currentInstruction.ToString());

        string TextInstruction = AASParsing.FindTokenInCollection(currentInstruction, "TextInstruction").SelectToken("value").ToString();
        string illustrationType = null;
        string illustrationURL = null;
        SetText(TextInstruction);
        textState = DownloadingState.completed;
        Debug.Log("txt instruction : " + TextInstruction);
        if (AASParsing.FindTokenInCollection(currentInstruction, "Illustration") != null) {
            illustrationType = AASParsing.FindTokenInCollection(AASParsing.FindTokenInCollection(currentInstruction, "Illustration").SelectToken("value"), "Type").SelectToken("value").ToString();
            Debug.Log("illustration type : " + illustrationType);
            illustrationURL = AASParsing.FindTokenInCollection(AASParsing.FindTokenInCollection(currentInstruction, "Illustration").SelectToken("value"), "FileURL").SelectToken("value").ToString();
            Debug.Log("illustration type : " + illustrationURL);
            illustrationState = DownloadingState.downloading;
            SetIllustration(illustrationType, illustrationURL);
        }
        JToken indicatorToken = AASParsing.FindTokenInCollection(currentInstruction, "Indicators");
        if (indicatorToken != null) {
            foreach (JToken indicator in indicatorToken.SelectToken("value"))
            {
                indicatorState = DownloadingState.downloading;
                AddIndicator(indicator.SelectToken("value"));
            }
        }
    }

    void SetText(string text)
    {
        textMeshPro.text = text;
        ResizeBoundingBox();
    }

    void SetIllustration(string illustrationType, string URL)
    {
        if (illustrationType == "image")
            StartCoroutine(LoadImageCoroutine(URL));
        else if (illustrationType == "video")
            StartCoroutine(LoadVideoCoroutine(URL));
        else
        {
            image.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadImageCoroutine(string imageUrl)
    {
        image.gameObject.SetActive(true);
        videoPlayer.gameObject.SetActive(false);
        Debug.Log("getting image at " + imageUrl);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Get the downloaded texture
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            // Create a Sprite without altering the aspect ratio
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)  // Center the pivot
            );
            yield return 0;
            image.preserveAspect = true;
            // Apply the sprite to the Image component
            image.sprite = sprite;
            float widthToHeighRatio = sprite.rect.height/sprite.rect.width;
            while (image.rectTransform.sizeDelta.x == 0) //wait until Unity is done intializing the rect transform 
            {
                yield return 0;
            }
            image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, image.rectTransform.sizeDelta.x * widthToHeighRatio);
            yield return 0;
            ResizeBoundingBox();
            illustrationState = DownloadingState.completed;
        }
        else
        {
            Debug.LogWarning("Failed to load image: " + request.error);
            illustrationState = DownloadingState.failed;
        }
    }

    private IEnumerator LoadVideoCoroutine(string videoUrl)
    {
        image.gameObject.SetActive(false);
        videoPlayer.gameObject.SetActive(true);
        Debug.Log("getting image at " + videoUrl);
        videoPlayer.url = videoUrl;
        videoPlayer.Play();
        RectTransform rectTransform = videoPlayer.gameObject.GetComponent<RectTransform>();
        while (videoPlayer.height == 0 || videoPlayer.width == 0 || rectTransform.sizeDelta.x == 0)
        {
            Debug.Log("waiting in this loop");
            yield return 0;
        }
        float widthToHeighRatio = (float)videoPlayer.height / (float)videoPlayer.width;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x * widthToHeighRatio);
        yield return 0;
        ResizeBoundingBox();
        illustrationState = DownloadingState.completed;
        yield return null;
    }

    private void AddIndicator(JToken indicator)
    {
        Debug.Log("token : " + indicator.ToString());
        string modelURL = AASParsing.FindTokenInCollection(indicator, "ModelURL").SelectToken("value").ToString();
        Debug.Log("model URL: " + modelURL);
        JToken posToken = AASParsing.FindTokenInCollection(indicator, "Position").SelectToken("value");
        JToken rotToken = AASParsing.FindTokenInCollection(indicator, "Rotation").SelectToken("value");
        JToken scaleToken = AASParsing.FindTokenInCollection(indicator, "Scale").SelectToken("value");
       

        Vector3 position = new Vector3(float.Parse(posToken[0].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(posToken[1].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(posToken[2].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat));

        Vector3 rotation = new Vector3(float.Parse(rotToken[0].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(rotToken[1].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(rotToken[2].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat));
        
        Vector3 scale = new Vector3(float.Parse(scaleToken[0].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(scaleToken[1].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(scaleToken[2].SelectToken("value").ToString(), CultureInfo.InvariantCulture.NumberFormat));

        if (modelURL != null)
        {
            StartCoroutine(LoadIndicatorModel(modelURL, position, rotation, scale));
        }

    }

    IEnumerator LoadIndicatorModel(string URL, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        yield return null;
        GameObject modelObj = new GameObject();
        yield return new WaitForSeconds(2);
        var gltf = new GLTFast.GltfImport();
        Task LoadModelTask = gltf.Load(URL);
        yield return new WaitUntil(() => LoadModelTask.IsCompleted);
        Task InstiantiateTask = gltf.InstantiateMainSceneAsync(modelObj.transform);
        yield return new WaitUntil(() => InstiantiateTask.IsCompleted);
        yield return new WaitForSeconds(1);
        Debug.Log("Model loaded");
        Debug.Log("position: " + position.ToString());
        modelObj.transform.parent = qrCode.transform;
        modelObj.transform.localPosition = position;
        modelObj.transform.localRotation = Quaternion.Euler(rotation);
        modelObj.transform.localScale = scale;
        indicators.Add(modelObj);
        indicatorState = DownloadingState.completed;
    }

    public void DestroyIndicators()
    {
        while (indicators != null && indicators.Count > 0)
        {
            Destroy(indicators[0].gameObject);
            indicators.RemoveAt(0);
            Debug.Log("destroyed obj");
            
        }
        indicators.Clear();
    }

    public void ResizeBoundingBox()
    {
        if (boundBoxManager == null) {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).tag == "BoundingBox")
                {
                    boundBoxManager = transform.transform.GetChild(i).GetComponent<BoundBoxManager>();
                }
            }
        }
        boundBoxManager.SetSize(rectTransform.sizeDelta);
    }

    public void SetProcedureSelectionObj(GameObject obj)
    {
        procedureSelectionObj = obj;
    }

    public void EndProcedure()
    {
        DestroyIndicators();
        procedureSelectionObj.SetActive(true);
        Destroy(this.gameObject);
    }
}
