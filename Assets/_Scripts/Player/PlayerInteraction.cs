using System;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private static readonly int CachedScaleProperty = Shader.PropertyToID("_Scale");
    private static readonly int CachedIsOutlinedProperty = Shader.PropertyToID("_IsOutlined");

    [SerializeField] private Camera cam;

    [SerializeField] [Min(0)] private float interactDistance = 5;

    [SerializeField] private TMP_Text interactText;

    [SerializeField] private Material outlineMaterial;
    [SerializeField] [Min(0)] private float outlineScale = 1.1f;
    [SerializeField] private Color outLineColor = Color.white;
    [SerializeField] private Color lookedAtColor = Color.yellow;

    /// <summary>
    /// A reference to the interactable that the player is currently looking at.
    /// </summary>
    private IInteractable _selectedInteractable;

    public event Action<IInteractable> OnLookAtInteractable;
    public event Action<IInteractable> OnStopLookingAtInteractable;

    #region Getters

    public Player Player { get; private set; }

    public IInteractable SelectedInteractable => _selectedInteractable;

    public float OutlineScale => outlineScale;

    #endregion

    private void Awake()
    {
        // Get the player component
        Player = GetComponent<Player>();
    }

    private void Start()
    {
        // Initialize the controls
        InitializeControls();

        // Events
        OnLookAtInteractable += SetLookedAtMaterial;
        OnStopLookingAtInteractable += UnsetLookedAtMaterial;
    }

    #region Controls

    private void InitializeControls()
    {
        // Subscribe to the interact event
        InputManager.Instance.PlayerControls.Player.Interact.performed += OnInteractPerformed;
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

        // Update the interact text
        UpdateInteractText();
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
            out var hitInfo,
            interactDistance
        );

        // Perform a raycast to see if the player is looking at an interactable
        if (hit)
        {
            // If the player is looking at an interactable,
            // set the current selected interactable to the interactable that the player is looking at
            if (hitInfo.collider.TryGetComponent(out IInteractable interactable))
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

    private void UpdateInteractText()
    {
        // If the player is not looking at an interactable, hide the interact text
        if (_selectedInteractable == null)
        {
            interactText.gameObject.SetActive(false);
            return;
        }

        // Show the interact text
        interactText.gameObject.SetActive(true);

        // Get the object's interact text
        var interactTextString = _selectedInteractable.InteractText(this);

        // If the interact text is empty,
        // set the interact text to the default interact text
        if (interactTextString == string.Empty)
            interactText.text = "Press E to interact";

        // Set the interact text to the interactable's interact text
        else
            interactText.text = $"Press E to\n{interactTextString}";
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
            SetOutlineMaterial(material, lookedAtColor, true);
    }

    private void UnsetLookedAtMaterial(IInteractable interactable)
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
            SetOutlineMaterial(material, outLineColor, true);
    }

    public void SetOutlineMaterial(Material material, Color color, bool isOutlined)
    {
        if (material == null)
            return;

        // Set the material's color to the looked at material's color
        material.color = color;

        // // Set the scale of the outline
        // material.SetFloat(CachedScaleProperty, scale);

        // Set the material's outline state
        material.SetFloat(CachedIsOutlinedProperty, isOutlined ? 1 : 0);
    }
}