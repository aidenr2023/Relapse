using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TolereanceMeter : MonoBehaviour
{
    public RectTransform needle;
    public float minAngle = 0f;
    public float maxAngle = 270f;
    


    public void UpdateToleranceUI(float percentage)
    {
        // Clamp the percentage between 0 and 1
        percentage = Mathf.Clamp01(percentage);

        // Calculate the angle based on the percentage
        float angle = Mathf.Lerp(minAngle, maxAngle, percentage);

        Debug.Log($"Updating dial: Percentage={percentage}, Angle={angle}");

        // Set the needle's rotation
        needle.localRotation = Quaternion.Euler(0f, 0f, -angle); // Negative to rotate in the correct direction
    }
}
