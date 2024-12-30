using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAtCam : MonoBehaviour
{
    // Update is called once per frame
    private void LateUpdate()
    {
        var mainCamera = Camera.main;

        // Return if the main camera is null
        if (mainCamera == null)
            return;

        transform.LookAt(
            transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up
        );
    }
}