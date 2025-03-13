using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTransformations : MonoBehaviour
{
    public void ResetTransforms()
    {
        transform.parent.parent.parent.GetComponent<XRDigitalTwin>().ResetTransforms();
    }
}
