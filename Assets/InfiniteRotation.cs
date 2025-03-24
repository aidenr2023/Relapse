using UnityEngine;

public class InfiniteRotation : MonoBehaviour
{
    [Header("Rotation Options")]
    public bool rotateX = false;
    public bool rotateY = true;
    public bool rotateZ = false;

    [Header("Rotation Speed (degrees per second)")]
    public float rotationSpeed = 90f;

    //void Update()
    {
        // Calculate rotation values based on selected axes
        float xRotation = rotateX ? rotationSpeed * Time.deltaTime : 0f;
        float yRotation = rotateY ? rotationSpeed * Time.deltaTime : 0f;
        float zRotation = rotateZ ? rotationSpeed * Time.deltaTime : 0f;

        // Apply the rotation to the object
        transform.Rotate(xRotation, yRotation, zRotation);
    }
}
