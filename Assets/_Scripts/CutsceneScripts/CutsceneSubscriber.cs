using UnityEngine;

/// <summary>
/// Listens to cutscene events from the CutsceneHandler and disables or enables player movement and UI accordingly.
/// </summary>
public class CutsceneSubscriber : MonoBehaviour
{
    public CutsceneHandler cutsceneHandler;
    public BasicPlayerMovement playerMovement;
    [SerializeField] PlayerMovementV2 playerMovementV2;

    private Rigidbody playerRigidbody;
    private PlayerActions playerActions;
    // Reference to the player's UI.
    public GameObject playerUI;
    private PlayerLook playerCameraMovement;
    private WeaponManager weaponManager;

    private void Start()
    {
        // Get the player's Rigidbody component.
        playerRigidbody = GetComponent<Rigidbody>();

        // Get the player's PlayerActions component.
        //playerActions = GetComponent<PlayerActions>();

        // Get the player's PlayerLook component.
        playerCameraMovement = GetComponent<PlayerLook>();

        // Get the player's WeaponManager component.
        weaponManager = GetComponent<WeaponManager>();

        // Subscribe to cutscene start/end events.
        if (cutsceneHandler != null)
        {
            cutsceneHandler.OnCutsceneStart.AddListener(DisableMovement);
            cutsceneHandler.OnCutsceneEnd.AddListener(EnableMovement);
            cutsceneHandler.OnCutsceneStart.AddListener(DisableUI);
            cutsceneHandler.OnCutsceneEnd.AddListener(EnableUI);
        }
        else
        {
            Debug.LogError("CutsceneHandler not assigned in CutsceneSubscriber.");
        }
    }

    /// <summary>
    /// Disables player movement when the cutscene starts.
    /// </summary>
    public void DisableMovement()
    {
        playerMovementV2.DisablePlayerControls();

        // playerRigidbody.velocity = Vector3.zero;

        playerMovement.enabled = false;
        
        
        playerMovementV2.enabled = false;
        
        playerCameraMovement.enabled = false;


        //turn off ridigbody physics

        playerRigidbody.isKinematic = true;
        
        weaponManager.enabled = false;

        //set the player to Kinematic to prevent physics from moving the player
        //playerRigidbody.isKinematic = true;
        playerRigidbody.velocity = Vector3.zero;
    }

    /// <summary>
    /// Re-enables player movement when the cutscene ends.
    /// </summary>
    public void EnableMovement()
    {
        playerMovementV2.enabled = true;
        playerMovement.enabled = true;
        //reset the player's velocity
        playerRigidbody.isKinematic = false;
        playerMovementV2.EnablePlayerControls();
        playerRigidbody.velocity = Vector3.zero;

        weaponManager.enabled = false;

        playerCameraMovement.enabled = true;
        //clear the player's input buffer


    }

    /// <summary>
    /// Disables the player's UI during the cutscene.
    /// </summary>
    public void DisableUI()
    {
        playerUI.SetActive(false);
    }
    
    /// <summary>
    /// Re-enables the player's UI after the cutscene.
    /// </summary>
    public void EnableUI()
    {
        playerUI.SetActive(true);
    }
    
    private void OnDestroy()
    {
        // Clean up subscriptions to prevent memory leaks.
        if (cutsceneHandler != null)
        {
            cutsceneHandler.OnCutsceneStart.RemoveListener(DisableMovement);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(EnableMovement);
            cutsceneHandler.OnCutsceneStart.RemoveListener(DisableUI);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(EnableUI);
        }
    }


    private void Update()
    {
        //debug log the velocity of the player
        Debug.Log(playerRigidbody.velocity);

        //log when the cutscene ends    

    }
}
