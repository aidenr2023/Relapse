// https://discussions.unity.com/t/automatically-assigning-gameobjects-a-unique-and-consistent-id-any-ideas/75104/3

using UnityEngine;
using System.Collections;

// Placeholder for UniqueIdDrawer script
public class UniqueIdentifierAttribute : PropertyAttribute
{
}

public class UniqueId : MonoBehaviour
{
    [SerializeField, UniqueIdentifier] private string uniqueId;

    public string UniqueIdValue => uniqueId;
}