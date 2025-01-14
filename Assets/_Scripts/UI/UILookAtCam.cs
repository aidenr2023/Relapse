using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAtCam : MonoBehaviour
{
    [SerializeField] private bool useFixedUpdate = false;

    private Camera _mainCamera;

    private void Update()
    {
        // Get the main camera
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        // Return if the main camera is null
        if (_mainCamera == null)
            return;

        // Return if useFixedUpdate is true
        if (useFixedUpdate)
            return;
        
        // Look at the camera
        transform.LookAt(
            transform.position + _mainCamera.transform.rotation * Vector3.forward,
            _mainCamera.transform.rotation * Vector3.up
        );
    }
    
    private void FixedUpdate()
    {
        // Return if the main camera is null
        if (_mainCamera == null)
            return;

        // Return if useFixedUpdate is false
        if (!useFixedUpdate)
            return;
        
        // Look at the camera
        transform.LookAt(
            transform.position + _mainCamera.transform.rotation * Vector3.forward,
            _mainCamera.transform.rotation * Vector3.up
        );
    }
}