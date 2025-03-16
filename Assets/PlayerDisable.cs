using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDisable : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Camera _cameraPlayer_hands;
    //private Camera _cameraPlayer;

    private void Start()
    {
        // If there is no cutscene manager instance, return
        if (CutsceneManager.Instance == null)
        {
            Debug.LogError("CutsceneManager not found!!!");
            return;
        }
        
        CutsceneManager.Instance.CutsceneHandler.OnCutsceneStart.AddListener(DisablePlayer);
        CutsceneManager.Instance.CutsceneHandler.OnCutsceneEnd.AddListener(EnablePlayer);

        if (CameraManager.Instance != null)
        {
            _cameraPlayer_hands = CameraManager.Instance.HandsCamera;
            //_cameraPlayer = CameraManager.Instance.MainCamera;
        }
        else
        {
            Debug.LogError("CameraManager not found");
        }
    }

    private void DisablePlayer()
    {
        int playerLayer = LayerMask.NameToLayer("GunHandHolder");
        if (playerLayer == -1)
        {
            Debug.LogError("Player layer does not exist!");
            return;
        }

        Debug.Log("Player Layer Index: " + playerLayer);
        Debug.Log("Culling mask before: " + _cameraPlayer_hands.cullingMask);
        _cameraPlayer_hands.cullingMask &= ~(1 << playerLayer);
        //_cameraPlayer.cullingMask &= ~(1 << playerLayer);
        Debug.Log("Culling mask after: " + _cameraPlayer_hands.cullingMask);
        Debug.Log("Player Disabled");
    }

    private void EnablePlayer()
    {
        int playerLayer = LayerMask.NameToLayer("GunHandHolder");
        if (playerLayer == -1)
        {
            Debug.LogError("Player layer does not exist!");
            return;
        }

        Debug.Log("Player Layer Index: " + playerLayer);
        Debug.Log("Culling mask before: " + _cameraPlayer_hands.cullingMask);
        _cameraPlayer_hands.cullingMask |= (1 << playerLayer); // Corrected line
        //_cameraPlayer.cullingMask |= (1 << playerLayer); // Corrected line
        Debug.Log("Culling mask after: " + _cameraPlayer_hands.cullingMask);
        Debug.Log("Player Enabled"); // Corrected log message
    }
}