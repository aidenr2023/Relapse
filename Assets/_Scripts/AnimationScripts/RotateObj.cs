using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObj : MonoBehaviour
{
    public float rotationSpeed = 45f;  // Rotation speed in degrees per second
    public Vector3 rotationAxis = Vector3.up;  // Axis of rotation (default is around the Y-axis)

    void Update()
    {
        // Rotate the object around the specified axis by the rotation speed
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
