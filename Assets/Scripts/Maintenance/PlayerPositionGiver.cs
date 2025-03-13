using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionGiver : MonoBehaviour
{
    public GameObject User;
    public GameObject QRCode;
    public TMPro.TextMeshProUGUI textMeshPro;


    // Update is called once per frame
    void Update()
    {
        if (User)
            Debug.Log("User pos" + User.transform.position);
        if (QRCode)
            Debug.Log("QRcode pos" + QRCode.transform.position);
        if (User && QRCode)
        {
            Vector3 relativePosition = QRCode.transform.InverseTransformPoint(User.transform.position);

            Debug.Log("relative pos" + relativePosition);
            if (textMeshPro)
            {
                textMeshPro.text = "X :" + relativePosition.x.ToString("0.00") + "\nY :" + relativePosition.y.ToString("0.00") + "\nZ :" + relativePosition.z.ToString("0.00");
            }
        }
    }
}
