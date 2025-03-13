using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*forces bound box pose to 0,0,0*/
public class BoundBoxManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.zero;
    }

    public void SetSize(Vector2 parentSize)
    {
        Debug.Log("parent size" +  parentSize.ToString());
        if (parentSize.x > 0 && parentSize.y > 0) 
            transform.localScale = new Vector3(parentSize.x + parentSize.x * 0.05f, parentSize.y + parentSize.x * 0.05f, 0.00001f);
    }
}
