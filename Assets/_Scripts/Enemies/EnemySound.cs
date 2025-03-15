using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo))]
public class EnemySound : ComponentScript<EnemyInfo>
{
    #region Serialized Fields

    [SerializeField] private Sound enemyDeathSound;

    [SerializeField] private ManagedAudioSource enemyMoanSource;
    [SerializeField] private Sound[] moanSounds;
    [SerializeField, Range(0, 1)] private float moanSoundChance = 1f;
    [SerializeField, Min(0)] private float moanSoundMinCooldown = 5f;
    [SerializeField, Min(0)] private float moanSoundMaxCooldown = 10f;

    #endregion

    #region Private Fields

    private bool _hasPlayedHitSoundThisFrame;

    private CountdownTimer _moanSoundTimer;

    #endregion

    protected override void CustomAwake()
    {
        // Set up the cooldown timer for the moan sound
        _moanSoundTimer = new CountdownTimer(UnityEngine.Random.Range(moanSoundMinCooldown, moanSoundMaxCooldown));

        _moanSoundTimer.OnTimerEnd += () =>
        {
            PlayMoanSound();
            _moanSoundTimer.SetMaxTimeAndReset(UnityEngine.Random.Range(moanSoundMinCooldown, moanSoundMaxCooldown));
        };
        _moanSoundTimer.Start();

        ParentComponent.OnDamaged += PlaySoundOnDamaged;

        ParentComponent.OnDeath += PlaySoundOnDeath;
    }

    private void Start()
    {
        // Set the moan sound source to be permanent
        enemyMoanSource?.SetPermanent(true);
    }

    private void Update()
    {
        // Update the moan sound timer
        _moanSoundTimer.Update(Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Reset the hasPlayedHitSoundThisFrame flag
        _hasPlayedHitSoundThisFrame = false;
    }

    private void PlaySoundOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Return if the sound is null
        if (enemyDeathSound == null)
            return;

        // Return if the hit sound has already played this frame
        if (_hasPlayedHitSoundThisFrame)
            return;

        // Play the sound at the enemy's position
        SoundManager.Instance.PlaySfxAtPoint(enemyDeathSound, transform.position);

        // Set the hasPlayedHitSoundThisFrame flag to true
        _hasPlayedHitSoundThisFrame = true;
    }

    private void PlaySoundOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // Determine which sound should be played
        var sfx = e.IsCriticalHit && e.DamagerObject.CriticalHitSfx != null
            ? e.DamagerObject.CriticalHitSfx
            : e.DamagerObject.NormalHitSfx;

        // Return if the sound is null
        if (sfx == null)
            return;

        // Return if the hit sound has already played this frame
        if (_hasPlayedHitSoundThisFrame)
            return;

        // Play the sound at the enemy's position
        SoundManager.Instance.PlaySfxAtPoint(sfx, transform.position);

        // Set the hasPlayedHitSoundThisFrame flag to true
        _hasPlayedHitSoundThisFrame = true;
    }

    private void PlayMoanSound()
    {
        // Random chance to play the moan sound
        if (UnityEngine.Random.value > moanSoundChance)
            return;

        // Return if the moan sounds array is null or empty
        if (moanSounds == null || moanSounds.Length == 0)
            return;

        // Play a random moan sound
        var randomSound = moanSounds[UnityEngine.Random.Range(0, moanSounds.Length)];
        enemyMoanSource?.Play(randomSound);
    }
}