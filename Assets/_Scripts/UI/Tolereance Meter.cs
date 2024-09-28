using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TolereanceMeter : MonoBehaviour
{
    public RectTransform needle;
    public float minAngle = 180f;
    public float maxAngle = 0f;
    


    public void UpdateToleranceUI(float percentage)
    {
        // Clamp the percentage between 0 and 1
        percentage = Mathf.Clamp01(percentage);

        // Calculate the angle based on the percentage
        float angle = Mathf.Lerp(minAngle, maxAngle, percentage);

        Debug.Log($"Updating dial: Percentage={percentage}, Angle={angle}");

        // Set the needle's rotation, account for initial 180° offset by adding 180° to angle
        needle.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
