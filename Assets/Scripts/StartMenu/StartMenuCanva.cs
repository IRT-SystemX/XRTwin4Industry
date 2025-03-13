using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuCanva : MonoBehaviour
{
    [SerializeField] AASClient aasClient;
    [SerializeField] GameObject AssetItemPrefab;
    [SerializeField] GameObject ValidateButton;
    public List<AASClient.DigitalTwinData> DTIDList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAssetsToSelect()
    {
        DTIDList = aasClient.DTIDList;
        int assetnbr = 0;
        foreach (AASClient.DigitalTwinData asset in DTIDList)
        {
            GameObject assetItemPrefab = GameObject.Instantiate(AssetItemPrefab, transform);
            StartMenuAssetItem startMenuAssetItem = assetItemPrefab.GetComponent<StartMenuAssetItem>();
            startMenuAssetItem.SetName(asset.assetIDshort.Replace('_', ' '));
            startMenuAssetItem.startMenuCanva = this;
            startMenuAssetItem.assetNbr = assetnbr;
            assetnbr++;
        }
        ValidateButton.SetActive(true);
    }

    public void ToggleAsset(int assetnbr)
    {
        DTIDList[assetnbr].enabled = !DTIDList[assetnbr].enabled;
    }

    public void Validate()
    {
        for (int i = 0; i < DTIDList.Count; i++)
        {
            if (!DTIDList[i].enabled)
            {
                DTIDList.RemoveAt(i);
                i--;
            }
        }
        StartCoroutine(CallInitDt());
    }

    IEnumerator CallInitDt()
    {
        yield return StartCoroutine(aasClient.InitDTs());
        this.gameObject.SetActive(false);
    }
}
