using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicMaterialManager : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private Renderer[] renderersToExclude;

    private readonly Dictionary<Renderer, Material> _materials = new();

    private void Awake()
    {
        // Get all the renderers on this object
        var renderers = GetComponentsInChildren<Renderer>();

        // Loop through all the renderers
        foreach (var cRenderer in renderers)
        {
            // If the renderer is in the renderersToExclude array, continue
            if (Array.Exists(renderersToExclude, r => r == cRenderer))
                continue;

            // Get all the materials on the object
            // Get the single material that is either the same as the material or the parent of the material
            var singleMaterial = Array.Find(cRenderer.sharedMaterials, m => m == material);

            // If the renderer does not have the material, add it to the list
            if (singleMaterial == null)
                singleMaterial = AddSingleMaterial(cRenderer, material);

            // Add the renderer and the material to the dictionary
            _materials.Add(cRenderer, singleMaterial);
            
            // Debug.Log($"Material added to {cRenderer.name}. Material: {singleMaterial.name}", cRenderer);
        }
    }

    private static Material AddSingleMaterial(Renderer r, Material material)
    {
        // // Create the new material
        // var newMaterial = new Material(material);
        var newMaterial = material;

        var rendererMaterials = r.sharedMaterials;
        
        // Create a new array based on the renderer's materials
        var newMaterials = new Material[rendererMaterials.Length + 1];

        for (var i = 0; i < rendererMaterials.Length; i++)
            newMaterials[i] = rendererMaterials[i];
        
        // Add the new material to the array
        newMaterials[^1] = newMaterial;

        // Set the renderer's materials to the new array
        r.sharedMaterials = newMaterials;
        
        // Return the new material
        return newMaterial;
    }

    public void AddMaterial()
    {
        var renderers = _materials.Keys.ToArray();
        
        foreach (var cRenderer in renderers)
        {
            // If the cRenderer is null, continue
            if (cRenderer == null)
                continue;

            // If the cRenderer's material is NOT null, continue
            if (_materials[cRenderer] != null)
                continue;

            // Update the dictionary
            _materials[cRenderer] = AddSingleMaterial(cRenderer, material);
        }
    }

    public void RemoveMaterial()
    {
        var keys = new List<Renderer>(_materials.Keys);

        foreach (var cRenderer in keys)
        {
            // If the cRenderer is null, continue
            if (cRenderer == null)
                continue;

            // If the cRenderer's material is null, continue
            if (_materials[cRenderer] == null)
                continue;

            var rendererMaterials = cRenderer.sharedMaterials;
            
            var newArr = new Material[rendererMaterials.Length - 1];
            
            var addIndex = 0;
            for (var i = 0; i < rendererMaterials.Length; i++)
            {
                var cMat = rendererMaterials[i];

                if (cMat == _materials[cRenderer])
                    continue;

                newArr[addIndex] = cMat;
                addIndex++;
            }
            
            // Set the renderer's materials to the arraylist
            cRenderer.sharedMaterials = newArr;

            // Update the dictionary
            _materials[cRenderer] = null;
        }
    }

    public void ChangeMaterial(Material newMaterial)
    {
        // Remove the material from all the renderers
        RemoveMaterial();

        // Set the material to the new material
        material = newMaterial;

        // Add the material to all the renderers
        AddMaterial();
    }
}