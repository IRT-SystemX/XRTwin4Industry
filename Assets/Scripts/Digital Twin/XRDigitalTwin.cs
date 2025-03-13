using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using GLTFast;
using Unity.VisualScripting.FullSerializer;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor;
using MixedReality.Toolkit.SpatialManipulation;

public class XRDigitalTwin : MonoBehaviour
{
    [SerializeField] GameObject _3DModel;
    [SerializeField] Dictionary<string, GameObject> interactableComponents;
    [SerializeField] PressableButton pressableButtonComponent;
    [SerializeField] List<Material> TransparentMaterials;
    [SerializeField] List<Material> HighVisibilityMaterials;
    public GameObject UICanvasPrefab;
    public GameObject UICNearMenuPrefab;
    public GameObject BoundsVisualsPrefab;
    public string ID;
    public bool isSimulating;
    int interactableComponentsCount = 0;
    bool hasInit = false;
    string ServerIP;
    string SetJointRequest;


    public IEnumerator UpdateCompleteDT(JObject model3D, JObject XRData)
    {
        if (!hasInit)
        {
            yield return StartCoroutine(Initialize(model3D, XRData));
            hasInit = true;
        } else
        {
            //Left open if a use case require completely updating an already initialized DT
        }
    }

    IEnumerator Initialize(JObject jsonModel3Data, JObject jsonXRData)
    {

        interactableComponents = new Dictionary<string, GameObject>();
        yield return LoadModel(jsonModel3Data, jsonXRData);
        Debug.Log("DT initialized " + ID);
    }

    IEnumerator LoadModel(JObject jsonModel3Data, JObject jsonXRData)
    {
        var gltf = new GLTFast.GltfImport();
        string modelURL = (string)AASParsing.FindTokenInSubmodel(jsonModel3Data, "File.FileVersion.DigitalFileURL");
        Task LoadModelTask = gltf.Load(modelURL);
        yield return new WaitUntil(()=> LoadModelTask.IsCompleted);
        Task InstiantiateTask = gltf.InstantiateMainSceneAsync(_3DModel.transform);
        yield return new WaitUntil(() => InstiantiateTask.IsCompleted);
        GetConnectorData(jsonXRData);
        InitComponents(_3DModel.transform.GetChild(0).gameObject, jsonXRData);
        AddBoxColliderToEncompassChildren();
        AddObjectManipulationScripts();
        GetComponent<UIFollowPlayer>().Init();
    }

    /*Init the different components of the asset */
    void InitComponents(GameObject obj, JObject AASdata)
    {
        /*Set mesh materials*/
        if (obj.name.Contains("[Interactable]") && obj.GetComponent<Renderer>())
        {
            InitInteractableSubcomponent(obj, AASdata);
        } else if (obj.GetComponent<Renderer>())
        {
            obj.GetComponent<Renderer>().material = TransparentMaterials[0];
        }

        for (int i = 0; i < obj.transform.childCount; i++) /*recursively call the method for each child*/
        {
            GameObject child = obj.transform.GetChild(i).gameObject;
            InitComponents(child, AASdata);
        }
    }

    void InitInteractableSubcomponent(GameObject obj, JObject AASdata)
    {
        interactableComponentsCount++;
        obj.GetComponent<Renderer>().material = TransparentMaterials[1];
        obj.AddComponent<BoxCollider>();
        PressableButton pressableButton = obj.AddComponent<PressableButton>();   
        pressableButton.StartPushPlane = 0.5f;
        pressableButton.EndPushPlane = 0;
        Interactable interactable = obj.AddComponent<Interactable>();
        interactable.UICanvasPrefab = UICanvasPrefab;
        string interactableName = ClearObjName(obj.name);
        interactableComponents.Add(interactableName, obj);
        JToken interactableToken = AASParsing.FindTokenInSubmodel(AASdata, "InteractableComponents." + interactableName);
        interactable.InitValues(interactableToken, interactableComponentsCount);
        interactable.defaultMaterial = TransparentMaterials[1];
        interactable.onSelectMaterial = TransparentMaterials[2];
        interactable.DigitalTwinRoot = this.gameObject;
        interactable.ServerIP = ServerIP;
        interactable.SetComponentRequest = SetJointRequest;
        JToken valuesToken = AASParsing.FindTokenInToken(interactableToken, "Values");
        JToken metadataToken = AASParsing.FindTokenInCollection(valuesToken[0].SelectToken("value"),
                "Metadata").SelectToken("value");
        if (metadataToken != null && metadataToken.HasValues && (bool)AASParsing.FindTokenInCollection(metadataToken, "DemoSimulation").SelectToken("value"))
            AddSimulatedJoint(obj, valuesToken, metadataToken);

    }

    void AddSimulatedJoint(GameObject obj, JToken valuesToken, JToken metadataToken)
    {
        SimulatedJoint joint = obj.AddComponent<SimulatedJoint>();
        joint.xOffset = float.Parse((string)AASParsing.FindTokenInCollection(metadataToken, "X_Offset").SelectToken("value"));
        joint.xRot = joint.xOffset;
        joint.yOffset = float.Parse((string)AASParsing.FindTokenInCollection(metadataToken, "Y_Offset").SelectToken("value"));
        joint.yRot = joint.yOffset;
        joint.zOffset = float.Parse((string)AASParsing.FindTokenInCollection(metadataToken, "Z_Offset").SelectToken("value"));
        joint.zRot = joint.zOffset;
        obj.transform.localRotation = Quaternion.Euler(joint.xRot, joint.yRot, joint.zRot);
        joint.maxValue = float.Parse((string)AASParsing.FindTokenInCollection(metadataToken, "MaxValue").SelectToken("value"));
        joint.minValue = float.Parse((string)AASParsing.FindTokenInCollection(metadataToken, "MinValue").SelectToken("value"));
       
        switch ((string)AASParsing.FindTokenInCollection(metadataToken, "Axis").SelectToken("value"))
        {
            case "X":
                joint.rotationAngle = SimulatedJoint.Angles.x;
                joint.xRot = joint.target + joint.xOffset;
                break;
            case "Y":
                joint.rotationAngle = SimulatedJoint.Angles.y;
                joint.yRot = -joint.target + joint.yOffset;
                break;
            case "Z":
                joint.rotationAngle = SimulatedJoint.Angles.z;
                joint.zRot = joint.target + joint.zOffset;
                break;
        }
        obj.transform.localRotation = Quaternion.Euler(joint.xRot, joint.yRot, joint.zRot);
        joint.target = float.Parse(((string)(AASParsing.FindTokenInCollection(valuesToken[0].SelectToken("value"),
               "CurrentValue").SelectToken("value"))).Replace(".", ","));
        if (joint.rotationAngle == SimulatedJoint.Angles.y)
        {
            float tmp = joint.minValue;
            joint.minValue = -joint.maxValue;
            joint.maxValue = -tmp;
            joint.target = -joint.target;
        }

        if (joint.target < joint.minValue)
            joint.target = joint.minValue;
        if (joint.target > joint.maxValue)
            joint.target = joint.maxValue;
            
        joint.rotationSpeed = float.Parse((string)AASParsing.FindTokenInCollection(metadataToken, "Speed").SelectToken("value"));
        
        /*set value when it doesn't start at 0*/

    }

    string ClearObjName(string name)
    {
        string newName = name.Split(' ')[1];
        return newName;
    }

    public Dictionary<string, GameObject> GetInteractableComponents()
    {
        return interactableComponents;
    }

    void AddBoxColliderToEncompassChildren()
    {
        // Create a new Bounds object starting from the position of the parent object
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        // Get all the MeshRenderer components in the children
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length > 0)
        {
            // Encapsulate each child's bounds
            foreach (MeshRenderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // Create or get the BoxCollider on the parent object
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            // Set the BoxCollider's center and size to match the encapsulated bounds
            boxCollider.center = bounds.center - transform.position;
            boxCollider.size = bounds.size;
            /*Adapter la bounding box?*/
        }
        else
        {
            Debug.LogWarning("No MeshRenderers found in children.");
        }
    }

    void AddObjectManipulationScripts()
    {
        ConstraintManager constraintManager = gameObject.AddComponent<ConstraintManager>();
        ObjectManipulator objectManipulator = gameObject.AddComponent<ObjectManipulator>();
        objectManipulator.HostTransform = transform;
        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.Move;
        BoundsControl boundsControl = gameObject.AddComponent<BoundsControl>();
        boundsControl.BoundsVisualsPrefab = BoundsVisualsPrefab;
        boundsControl.ConstraintsManager = constraintManager;
        boundsControl.EnabledHandles = (HandleType)7; //Rotation + translation + scale
        UGUIInputAdapterDraggable uGUIInputAdapterDraggable = gameObject.AddComponent<UGUIInputAdapterDraggable>();
        MinMaxScaleConstraint minMaxScaleConstraint = gameObject.AddComponent<MinMaxScaleConstraint>();
        minMaxScaleConstraint.MinimumScale = new Vector3(0.01f, 0.01f, 0.01f);
        minMaxScaleConstraint.MaximumScale = new Vector3(20f, 20f, 20f);
    }

    /*Switch to manipulation mode or to component selection mode*/
    public void SwitchObjectMode()
    {
        BoundsControl boundsControl = gameObject.GetComponent<BoundsControl>();
        boundsControl.enabled = !boundsControl.enabled;
        ObjectManipulator objectManipulator = gameObject.GetComponent<ObjectManipulator>();
        objectManipulator.enabled = !objectManipulator.enabled;
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.enabled = !boxCollider.enabled;
        foreach (KeyValuePair<string, GameObject> kvp in interactableComponents)
        {
            Interactable interactableScript = kvp.Value.GetComponent<Interactable>();
            interactableScript.canOpenUI = !interactableScript.canOpenUI;
        }

    }

    public void ResetTransforms()
    {
        transform.localScale = Vector3.one; 
        transform.eulerAngles = Vector3.zero;
    }

    public void CloseEveryInteractableUI()
    {
        foreach (KeyValuePair<string, GameObject> kvp in interactableComponents)
        {
            Interactable interactableScript = kvp.Value.GetComponent<Interactable>();
            if (interactableScript.isUIOpen) {
                interactableScript.ToggleUI();
            }
        }
    }


    void GetConnectorData(JToken XRDataToken)
    {
        JToken XRMetadataToken = AASParsing.FindTokenInSubmodel((JObject)XRDataToken, "XRMetadata");
        JToken ConnectorToken = AASParsing.FindTokenInCollection(XRMetadataToken, "Connector").SelectToken("value");
        ServerIP = (string)AASParsing.FindTokenInCollection(ConnectorToken, "ServerIP").SelectToken("value");
        SetJointRequest = (string)AASParsing.FindTokenInCollection(ConnectorToken, "SetJointRequest").SelectToken("value");
    }


    public void SetHighVisibilityMaterial(GameObject obj = null)
    {
        if (obj == null)
            obj = this.gameObject;
        Renderer renderer = obj.GetComponent<Renderer>();
        /*Set mesh materials*/
        if (obj.name.Contains("[Interactable]") && renderer != null)
        {
            obj.GetComponent<Interactable>().SwitchMaterial(HighVisibilityMaterials[1], HighVisibilityMaterials[2]);
        }
        else if (renderer != null && renderer.material.name.Contains("Non interactable-transparent"))
        {
            obj.GetComponent<Renderer>().material = HighVisibilityMaterials[0];
        }

        for (int i = 0; i < obj.transform.childCount; i++) /*recursively call the method for each child*/
        {
            GameObject child = obj.transform.GetChild(i).gameObject;
            SetHighVisibilityMaterial(child);
        }
    }

    public void SetTransparentMaterial(GameObject obj = null)
    {
        if (obj == null)
            obj = this.gameObject;
        Renderer renderer = obj.GetComponent<Renderer>();
        /*Set mesh materials*/
        if (obj.name.Contains("[Interactable]") && renderer != null)
        {
            obj.GetComponent<Interactable>().SwitchMaterial(TransparentMaterials[1], TransparentMaterials[2]);
        }
        else if (renderer != null && renderer.material.name.Contains("Non interactable"))
        {
            obj.GetComponent<Renderer>().material = TransparentMaterials[0];
        }

        for (int i = 0; i < obj.transform.childCount; i++) /*recursively call the method for each child*/
        {
            GameObject child = obj.transform.GetChild(i).gameObject;
            SetTransparentMaterial(child);
        }
    }
}

