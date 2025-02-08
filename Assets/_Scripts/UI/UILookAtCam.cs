using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAtCam : MonoBehaviour
{
    [SerializeField] private bool useUpdate = true;
    [SerializeField] private bool useLateUpdate = false;
    [SerializeField] private bool useFixedUpdate = false;
    [SerializeField] private bool useStart = false;
    
    private Camera _mainCamera;

    private void Start()
    {
        // Look at the camera
        if (useStart)
            LookAtCamera();
    }

    private void Update()
    {
        // Get the main camera
        if (_mainCamera == null)
            _mainCamera = Camera.main;
        
        // Return if not use update
        if (!useUpdate)
            return;
        
        // Look at the camera
        LookAtCamera();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        // Return if not use late update
        if (!useLateUpdate)
            return;
        
        // Look at the camera
        LookAtCamera();
    }
    
    private void FixedUpdate()
    {
        // Return if useFixedUpdate is false
        if (!useFixedUpdate)
            return;
        
        // Look at the camera
        LookAtCamera();
    }

    private void LookAtCamera()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
        
        // Look at the camera
        transform.LookAt(
            transform.position + _mainCamera.transform.rotation * Vector3.forward,
            _mainCamera.transform.rotation * Vector3.up
        );
    }
}