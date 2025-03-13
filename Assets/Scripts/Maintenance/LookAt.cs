using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        LookAtTarget();
    }

    void LookAtTarget()
    {
        //transform.LookAt(new Vector3(target.transform.position.x, -target.transform.position.y, target.transform.position.z));
        transform.rotation = Quaternion.LookRotation(transform.position - target.transform.position);
    }
}
