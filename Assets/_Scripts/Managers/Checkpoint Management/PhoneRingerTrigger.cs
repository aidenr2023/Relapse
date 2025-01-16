using System;
using UnityEngine;

public class PhoneRingerTrigger : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private CheckpointInteractable checkpointInteractable;

    [SerializeField] private Sound ringSound;
    [SerializeField] private float ringInterval;
    
    [SerializeField] private Sound beepSound;
    
    #endregion

    #region Private Fields

    private bool _isInTrigger;

    private CountdownTimer _ringTimer;

    private ManagedAudioSource _audioSource;

    #endregion

    private void Awake()
    {
        // Assert that the checkpointInteractable is not null
        if (checkpointInteractable == null)
            Debug.LogError("CheckpointInteractable is null.");
    }

    private void Start()
    {
        // Set up the ring timer
        _ringTimer = new CountdownTimer(ringInterval);

        _ringTimer.ForcePercent(.9999f);

        // Play the ring SFX from the checkpointInteractable when the timer is done
        _ringTimer.OnTimerEnd += () =>
        {
            // Ring the phone
            RingPhone();

            // Restart the timer
            _ringTimer.Reset();
        };
    }

    private void Update()
    {
        // Set the max time for the ring timer
        _ringTimer.SetMaxTime(ringInterval);

        // Update the ring timer
        _ringTimer.Update(Time.deltaTime);

        // If the current audio source is not null & the audio source is still playing, stop it
        if (_audioSource != null && checkpointInteractable.HasBeenCollected)
        {
            _audioSource.Kill();
            _audioSource = null;
            
            // Play the beep sound at the checkpointInteractable's position
            SoundManager.Instance.PlaySfxAtPoint(beepSound, checkpointInteractable.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Assert that the other collider is the player
        if (!other.CompareTag("Player"))
            return;

        _isInTrigger = true;

        // Start the ring timer
        _ringTimer.Start();
    }

    private void OnTriggerExit(Collider other)
    {
        // Assert that the other collider is the player
        if (!other.CompareTag("Player"))
            return;

        _isInTrigger = false;

        // Stop the ring timer
        _ringTimer.Stop();
    }

    private void RingPhone()
    {
        // Return if the player is not in the collider
        if (!_isInTrigger)
            return;

        // Return if the checkpoint has already been collected
        if (checkpointInteractable.HasBeenCollected)
            return;

        // Play the ring SFX from the checkpointInteractable
        // SoundManager_OLD.Instance.PlaySfxAtPoint(ringSound, checkpointInteractable.transform.position);
        _audioSource = SoundManager.Instance.PlaySfxAtPoint(ringSound, checkpointInteractable.transform.position);
    }
}