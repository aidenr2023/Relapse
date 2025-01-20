using System;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour, IUsesInput
{
    private static readonly int CachedScaleProperty = Shader.PropertyToID("_Scale");
    private static readonly int CachedIsOutlinedProperty = Shader.PropertyToID("_IsOutlined");
    private static readonly int CachedIsSelectedProperty = Shader.PropertyToID("_IsSelected");

    #region Serialized Fields

    [SerializeField] private Camera cam;

    [SerializeField] [Min(0)] private float interactDistance = 5;

    [SerializeField] private Material outlineMaterial;
    [SerializeField] [Min(0)] private float outlineScale = 1.1f;

    #endregion

    #region Private Fields

    /// <summary>
    /// A reference to the interactable that the player is currently looking at.
    /// </summary>
    private IInteractable _selectedInteractable;

    private RaycastHit _interactionHitInfo;

    #endregion

    public event Action<IInteractable> OnLookAtInteractable;
    public event Action<IInteractable> OnStopLookingAtInteractable;

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public Player Player { get; private set; }

    public IInteractable SelectedInteractable => _selectedInteractable;

    public Material OutlineMaterial => outlineMaterial;

    public float OutlineScale => outlineScale;

    public RaycastHit InteractionHitInfo => _interactionHitInfo;

    public InteractionIcon CurrentInteractionIcon => _selectedInteractable?.InteractionIcon ?? InteractionIcon.None;

    #endregion

    private void Awake()
    {
        // Get the player component
        Player = GetComponent<Player>();

        // Initialize the controls
        InitializeInput();
    }

    private void Start()
    {
        // Events
        OnLookAtInteractable += SetLookedAtMaterial;
        OnStopLookingAtInteractable += UnsetLookedAtMaterial;
    }

    private void OnEnable()
    {
        // Register the player with the input system
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister the player with the input system
        InputManager.Instance.Unregister(this);
    }

    #region Input Functions

    public void InitializeInput()
    {
        // Subscribe to the interact event
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Interact, InputType.Performed, OnInteractPerformed)
        );
    }

    private void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        // If the player is not currently looking at an interactable, return
        if (_selectedInteractable == null)
            return;

        // Interact with the current interactable
        _selectedInteractable.Interact(this);
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the interactable if it is being looked at
        _selectedInteractable?.LookAtUpdate(this);
    }

    private void FixedUpdate()
    {
        // Detect the currently selected interactable
        UpdateSelectedInteractable();
    }

    private void UpdateSelectedInteractable()
    {
        var previousInteractable = _selectedInteractable;

        // Get the camera pivot
        var cameraPivot = cam.transform;

        // Is there a ray cast hit within the interact distance?
        var hit = Physics.Raycast(
            cameraPivot.position,
            cameraPivot.forward,
            out _interactionHitInfo,
            interactDistance
        );

        // Perform a raycast to see if the player is looking at an interactable
        if (hit)
        {
            // If the player is looking at an interactable,
            // set the current selected interactable to the interactable that the player is looking at
            if (_interactionHitInfo.collider.TryGetComponent(out IInteractable interactable))
            {
                // If the interactable is interactable,
                // set the current selected interactable to the interactable
                if (interactable.IsInteractable)
                    _selectedInteractable = interactable;

                // Otherwise, set the current selected interactable to null
                else _selectedInteractable = null;
            }

            // If there is no interactable, set the current selected interactable to null
            else _selectedInteractable = null;
        }

        // If the player is not looking at an interactable, set the current selected interactable to null
        else _selectedInteractable = null;

        // Events for looking at interactable(s)
        if (previousInteractable != _selectedInteractable)
        {
            if (previousInteractable != null)
                OnStopLookingAtInteractable?.Invoke(previousInteractable);

            if (_selectedInteractable != null)
                OnLookAtInteractable?.Invoke(_selectedInteractable);
        }
    }

    #endregion

    #region Materials Functions

    private void SetLookedAtMaterial(IInteractable interactable)
    {
        // Return if the interactable is null
        if (interactable == null || interactable.GameObject == null)
            return;

        // Return if the interactable has no outline materials
        if (interactable.OutlineMaterials == null)
            return;

        // Check if the interactable has outline materials
        // Get the outline materials
        if (interactable.OutlineMaterials != null && interactable.OutlineMaterials.Count == 0)
            interactable.GetOutlineMaterials(outlineMaterial.shader);

        // Set the outline materials to the looked at color
        foreach (var material in interactable.OutlineMaterials)
            SetOutlineMaterial(material, true, true);
    }

    private void UnsetLookedAtMaterial(IInteractable interactable)
    {
        // Return if the interactable is null
        if (interactable == null || (interactable is MonoBehaviour mb && mb == null))
            return;

        // Return if the interactable has no outline materials
        if (interactable.OutlineMaterials == null)
            return;

        // Check if the interactable has outline materials
        // Get the outline materials
        if (interactable.OutlineMaterials != null && interactable.OutlineMaterials.Count == 0)
            interactable.GetOutlineMaterials(outlineMaterial.shader);

        // Set the outline materials to the looked at color
        foreach (var material in interactable.OutlineMaterials)
            SetOutlineMaterial(material, false, true);
    }

    public void SetOutlineMaterial(Material material, bool isSelected, bool isOutlined)
    {
        if (material == null)
            return;

        // Set the material's color to the looked at material's color
        material.SetFloat(CachedIsSelectedProperty, isSelected ? 1 : 0);

        // // Set the scale of the outline
        // material.SetFloat(CachedScaleProperty, scale);

        // Set the material's outline state
        material.SetInt(CachedIsOutlinedProperty, isOutlined ? 1 : 0);
    }

    #endregion

    private void OnDrawGizmos()
    {
        // Return if the selected interactable is null
        if (_selectedInteractable == null)
            return;

        // return if the selected interactable's game object is null
        if (_selectedInteractable.GameObject == null || _selectedInteractable.GameObject is null)
            return;

        // Draw a sphere at the interactable's position
        Gizmos.DrawSphere(_selectedInteractable.GameObject.transform.position, 0.5f);
    }
}