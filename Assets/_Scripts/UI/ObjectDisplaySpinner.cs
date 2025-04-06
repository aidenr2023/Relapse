using System;
using UnityEngine;

public class ObjectDisplaySpinner : MonoBehaviour
{
    private const float DEFAULT_ROTATION_SPEED = .25f;
    private const float DEFAULT_X_ROTATION = 45f;

    [SerializeField] private float rotationSpeed = DEFAULT_ROTATION_SPEED;
    [SerializeField] private float xRotation = DEFAULT_X_ROTATION;

    private Transform _childObject;

    private void Update()
    {
        // Rotate this transform around the y-axis
        var currentRotation = transform.localRotation.eulerAngles;
        var newRotation = Quaternion.Euler(
            currentRotation.x,
            currentRotation.y + (rotationSpeed * 360 * Time.deltaTime),
            currentRotation.z
        );
        transform.localRotation = newRotation;
    }

    public static ObjectDisplaySpinner DisplayObject(
        Transform transform,
        float rotationSpeed = DEFAULT_ROTATION_SPEED,
        float xRotation = DEFAULT_X_ROTATION
    )
    {
        // var spinner = Instantiate<ObjectDisplaySpinner>(transform.transform, Quaternion.identity);

        // Create an empty game object
        var spinnerObject = new GameObject($"Display: {transform.name}")
        {
            transform =
            {
                position = transform.position,
                rotation = Quaternion.identity
            }
        };

        var spinner = spinnerObject.AddComponent<ObjectDisplaySpinner>();

        // Set the parent of the transform to the spinner
        spinner._childObject = transform;
        spinner.xRotation = xRotation;
        spinner.rotationSpeed = rotationSpeed;
        transform.SetParent(spinnerObject.transform);
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        return spinner;
    }
}