using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform respawnPosition;
    public Transform RespawnPosition => respawnPosition;
    public bool IsInteractable => true;

    public GameObject GameObject => gameObject;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public void Interact(PlayerInteraction playerInteraction)
    {
        Checkpoint.Instance.SaveCheckpoint(this);
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return "Save checkpoint";
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
