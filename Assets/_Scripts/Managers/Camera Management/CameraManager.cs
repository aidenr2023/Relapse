﻿using System;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    [SerializeField] private CameraManagerVariable cameraManagerSo;
    [SerializeField] private TransformReference cameraPivot;
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera handsCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public Camera MainCamera => mainCamera;
    
    public Camera HandsCamera => handsCamera;
    
    public CinemachineVirtualCamera VirtualCamera => virtualCamera;
    
    private void Awake()
    {
        // If there is already an instance, destroy this object
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set the instance
        Instance = this;
        
        // Set the camera manager in the scriptable object
        cameraManagerSo.value = this;
    }

    private void OnDestroy()
    {
        // If the instance is this, set it to null
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        // If the virtual camera's follow is ever null,
        // set it to the instance of the player
        if (virtualCamera != null && virtualCamera.Follow == null && cameraPivot.Value != null)
            virtualCamera.Follow = cameraPivot.Value;
    }
}