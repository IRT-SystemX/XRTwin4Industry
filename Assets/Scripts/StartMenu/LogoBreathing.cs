using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoBreathing : MonoBehaviour
{
    public float minPos;
    public float maxPos;
    public float speed;

    bool goingUp = true;

    // Update is called once per frame
    void Update()
    {
        if (goingUp)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y , transform.localPosition.z + speed * Time.deltaTime);
            if (transform.localPosition.z > maxPos)
                goingUp = false;
        } else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y , transform.localPosition.z - (speed * Time.deltaTime));
            if (transform.localPosition.z < minPos)
                goingUp = true;
        }
    }
}
