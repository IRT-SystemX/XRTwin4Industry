using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowPlayer : MonoBehaviour
{
    public GameObject UIPrefab; // The UI object
    public Transform player; // The player object
    public BoxCollider boxCollider;
    public float sizeMultiplier;

    GameObject UI;
    bool init = false;

    public void Init()
    {
        boxCollider = GetComponent<BoxCollider>();
        player = GameObject.FindGameObjectWithTag("MainCamera").transform;
        UI = GameObject.Instantiate(UIPrefab, transform);
        init = true;
        float boxBiggestDimension = GetBiggestDimensionInVector(boxCollider.size);
        UI.transform.localScale = new Vector3(boxBiggestDimension * sizeMultiplier, boxBiggestDimension * sizeMultiplier, 1);
    }

    float GetBiggestDimensionInVector(Vector3 size)
    {
        float biggest = size.x;
        if (size.y > biggest)
            biggest = size.y;
        if (size.z > biggest)
            biggest = size.z;
        return biggest;
    }

    void Update()
    {
        if (!init)
            return;
        Vector3 directionToPlayer = player.position - transform.position;

        // Get the cube's local forward, right, and up vectors
        Vector3 cubeForward = transform.forward;
        Vector3 cubeRight = transform.right;

        // Determine which side of the cube is facing the player
        float forwardDot = Vector3.Dot(directionToPlayer, cubeForward);
        float rightDot = Vector3.Dot(directionToPlayer, cubeRight);

        Vector3 UIPosition = Vector3.zero;
        Quaternion UIRotation = Quaternion.identity;

        if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
        {
            if (forwardDot > 0)
            {
                // Front side
                UI.transform.localPosition = new Vector3(0 + boxCollider.center.x, -boxCollider.size.y / 2 + boxCollider.center.y,
                (boxCollider.size.z / 2 + 0.05f +boxCollider.center.z));
                UIRotation = Quaternion.LookRotation(-cubeForward);
            }
            else
            {
                // Back side
                UI.transform.localPosition = new Vector3(0 +boxCollider.center.x, -boxCollider.size.y / 2 + boxCollider.center.y,
                (-(boxCollider.size.z / 2 + 0.05f) + boxCollider.center.z));
                UIRotation = Quaternion.LookRotation(cubeForward);
            }
        }
        else
        {
            if (rightDot > 0)
            {
                // Right side
                UI.transform.localPosition = new Vector3(boxCollider.size.x / 2 + 0.05f + +boxCollider.center.x, -boxCollider.size.y / 2 + boxCollider.center.y,
                    +boxCollider.center.z);
                UIRotation = Quaternion.LookRotation(-cubeRight);
            }
            else
            {
                // Left side
                UI.transform.localPosition = new Vector3(-(boxCollider.size.x / 2 + 0.05f) + boxCollider.center.x, 
                    -boxCollider.size.y / 2 + boxCollider.center.y, 0 +boxCollider.center.z);
                UIRotation = Quaternion.LookRotation(cubeRight);
            }
        }
        UI.transform.rotation = UIRotation;
    }
}
