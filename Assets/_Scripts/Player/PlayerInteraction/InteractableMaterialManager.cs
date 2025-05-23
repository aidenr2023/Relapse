﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableMaterialManager : MonoBehaviour
{
    private static readonly int CachedIsOutlinedProperty = Shader.PropertyToID("_IsOutlined");
    private static readonly int CachedIsSelectedProperty = Shader.PropertyToID("_IsSelected");

    #region Serialized Fields

    [SerializeField] private bool showOutline = true;

    #endregion

    #region Private Fields

    private SelectedState _selectedState = SelectedState.Uninitialized;
    private OutlineState _outlineState = OutlineState.Uninitialized;

    private readonly Dictionary<Renderer, Material> _interactableMaterials = new();

    #endregion

    #region Getters

    public IInteractable Interactable { get; private set; }

    public bool ShowOutline
    {
        get => showOutline;
        set => showOutline = value;
    }

    public bool IsForceSelected { get; set; }

    #endregion

    private void Awake()
    {
        // Get the interactable component
        Interactable = GetComponent<IInteractable>();

        // Set the interactable material manager of the interactable
        if (Interactable != null)
            Interactable.InteractableMaterialManager = this;
    }

    private void Start()
    {
        // Set up the interactable materials
        StartCoroutine(SetUpMaterialCoroutine());
    }

    private IEnumerator SetUpMaterialCoroutine()
    {
        // Wait until the interactable manager's instance is not null
        yield return new WaitUntil(() => InteractableManager.Instance != null);

        SetUpInteractableMaterials();
    }

    private void Update()
    {
        var playerInteraction = Player.Instance?.PlayerInteraction;

        // Return if the player interaction instance is null
        if (playerInteraction == null)
            return;

        var isSelected = playerInteraction.SelectedInteractable == Interactable;

        // if (isSelected)
        //     Debug.Log($"Selected {Interactable.GameObject.name}");

        // Set the selected property of the interactable
        SetSelected(isSelected || IsForceSelected);

        // Set the outlined property of the interactable
        SetOutlined(showOutline);

        // Reset the force selected property
        IsForceSelected = false;
    }

    private void SetUpInteractableMaterials()
    {
        var outlineMaterial = InteractableManager.Instance.OutlineMaterial;

        // Return if player interaction's outline material is null
        if (outlineMaterial == null)
            return;

        // Get all the renderers in this object
        var renderers = GetComponentsInChildren<Renderer>();

        // Loop through the renderers
        foreach (var cRenderer in renderers)
        {
            // Check if the renderer has ANY material with the same shader as the outline material
            var materials = cRenderer.materials;

            Material outlineMaterialInstance = null;

            foreach (var material in materials)
            {
                if (material.shader != outlineMaterial.shader)
                    continue;

                outlineMaterialInstance = material;
                break;
            }

            // If the outline material instance is still null,
            // create a new material and add it to the renderer
            if (outlineMaterialInstance == null)
            {
                outlineMaterialInstance = new Material(outlineMaterial);

                var oldMaterials = new List<Material>(materials) { outlineMaterialInstance };
                cRenderer.materials = oldMaterials.ToArray();
            }

            // Add the renderer and the material to the interactable materials dictionary
            _interactableMaterials.Add(cRenderer, outlineMaterialInstance);
        }
    }

    private void SetOutlined(bool isOutlined)
    {
        if (isOutlined)
        {
            if (_outlineState == OutlineState.Outlined)
                return;
        
            _outlineState = OutlineState.Outlined;
        }
        
        // This causes a bug with spawning a gun in the player's hands
        else
        {
            if (_outlineState == OutlineState.NotOutlined)
                return;
        
            _outlineState = OutlineState.NotOutlined;
        }

        foreach (var interactableMaterial in _interactableMaterials)
        {
            var material = interactableMaterial.Value;
            material.SetInt(CachedIsOutlinedProperty, isOutlined ? 1 : 0);
        }
    }

    private void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            if (_selectedState == SelectedState.Selected)
                return;

            _selectedState = SelectedState.Selected;
        }

        else
        {
            if (_selectedState == SelectedState.NotSelected)
                return;

            _selectedState = SelectedState.NotSelected;
        }

        foreach (var interactableMaterial in _interactableMaterials)
        {
            var material = interactableMaterial.Value;
            material.SetFloat(CachedIsSelectedProperty, isSelected ? 1 : 0);
        }
    }

    private enum SelectedState
    {
        Uninitialized,
        Selected,
        NotSelected
    }

    private enum OutlineState
    {
        Uninitialized,
        Outlined,
        NotOutlined
    }
}