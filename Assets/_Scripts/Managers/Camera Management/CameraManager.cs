using System;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
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
    }

    private void OnDestroy()
    {
        // If the instance is this, set it to null
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        var playerInstance = Player.Instance;
        
        // If the virtual camera's follow is ever null,
        // set it to the instance of the player
        if (virtualCamera != null && virtualCamera.Follow == null && playerInstance != null)
            virtualCamera.Follow = playerInstance.PlayerController.CameraPivot.transform;
    }
}