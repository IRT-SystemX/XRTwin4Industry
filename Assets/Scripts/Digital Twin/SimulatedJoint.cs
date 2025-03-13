using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimulatedJoint : MonoBehaviour
{
    public float rotationSpeed;
    public float target;
    public enum Angles { x, y, z };
    public Angles rotationAngle;

    bool isGoingClockwise = true;

    
    public float xRot = 0;
    public float yRot = 0;
    public float zRot = 0;

    public float xOffset = 0;
    public float yOffset = 0;
    public float zOffset = 0;

    public float maxValue;
    public float minValue;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (rotationSpeed == 0)
        {
            return;
        }
        target = target > maxValue ? maxValue : target;
        target = target < minValue ? minValue : target;

        RotatePartialAmplitudeQuat();
        return;
    }

    void RotatePartialAmplitudeQuat()
    {
        isGoingClockwise = transform.localEulerAngles.y < target ? true : false;

        switch (rotationAngle)
        {
            case Angles.x:
                if (xRot + xOffset == target)
                    return;
                if (xRot < target + xOffset)
                {
                    if (xRot + rotationSpeed * Time.deltaTime > target + xOffset)
                    {
                        xRot = target + xOffset;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                    else
                    {
                        xRot += rotationSpeed * Time.deltaTime;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }

                }
                else
                {
                    if (xRot - rotationSpeed * Time.deltaTime < target + xOffset)
                    {
                        xRot = target + xOffset;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                    else
                    {
                        xRot -= rotationSpeed * Time.deltaTime;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                }
                break;
            case Angles.y:

                if (yRot == target + yOffset)
                    return;
                if (yRot < target + yOffset)
                {
                    if (yRot + rotationSpeed * Time.deltaTime > target + yOffset)
                    {
                        yRot = target + yOffset;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                    else
                    {
                        yRot += rotationSpeed * Time.deltaTime;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);

                    }

                }
                else
                {
                    if (yRot - rotationSpeed * Time.deltaTime < target + yOffset)
                    {
                        yRot = target + yOffset;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                    else
                    {
                        yRot -= rotationSpeed * Time.deltaTime;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                }
                break;
            case Angles.z:
                if (zRot == target + zOffset)
                    return;
                if (zRot < target + zOffset)
                {
                    if (zRot + rotationSpeed * Time.deltaTime > target + zOffset)
                    {
                        zRot = target + zOffset;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                    else
                    {
                        zRot += rotationSpeed * Time.deltaTime;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                }
                else
                {
                    if (zRot - rotationSpeed * Time.deltaTime < target + zOffset)
                    {
                        zRot = target + zOffset;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                    else
                    {
                        zRot -= rotationSpeed * Time.deltaTime;
                        transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
                    }
                }
                break;

        }
    }
}