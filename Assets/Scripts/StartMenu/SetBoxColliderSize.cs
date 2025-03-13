using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBoxColliderSize : MonoBehaviour
{
    public Vector3 size;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoxCollider>().size = size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
