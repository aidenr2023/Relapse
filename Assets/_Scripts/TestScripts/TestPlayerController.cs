using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TestPlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    [SerializeField] private float moveSpeed;

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the inputs
        InitializeInputs();
    }

    private void InitializeComponents()
    {
        // Get the CharacterController component
        _characterController = GetComponent<CharacterController>();
    }

    private void InitializeInputs()
    {
        
    }

    #endregion


    // Update is called once per frame
    void Update()
    {
    }
}