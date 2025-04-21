using System;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyVFX : ComponentScript<EnemyInfo>
{
    #region Serialized Fields

    [Space, SerializeField] private VisualEffect enemyHitEffect;
    [SerializeField, Min(0)] private float minVFXRangeDamage = 10f;
    [SerializeField, Min(0)] private float maxVFXRangeDamage = 50f;
    [SerializeField, Min(0)] private float minVFXDamage = 5f;

    [SerializeField] private VisualEffect enemyDeathVfxPrefab;

    [SerializeField] private VisualEffect spawnVfxPrefab;

    #endregion

    #region Private Fields

    private bool _hasPlayedHitVFX;

    private CountdownTimer _comicImpactTimer;

    #endregion

    protected override void CustomAwake()
    {
        _comicImpactTimer = new(0.5f, true, true);

        ParentComponent.OnDamaged += PlayVfxOnDamaged;
        ParentComponent.OnDamaged += ComicImpactOnDamaged;

        ParentComponent.OnDeath += DetachVFXOnDeath;
        ParentComponent.OnDeath += CreateDeathVfx;
    }

    private void Start()
    {
        SpawnVfx();
    }

    private void SpawnVfx()
    {
        // Return if there are no spawn VFX
        if (spawnVfxPrefab == null)
            return;

        // Instantiate the spawn VFX at the enemy's position
        var spawnVfx = Instantiate(spawnVfxPrefab, ParentComponent.ParentComponent.CenterTransform.position, Quaternion.identity);

        // Play the spawn VFX
        spawnVfx.Play();

        Destroy(spawnVfx.gameObject, 5);
    }

    private void PlayVfxOnDamaged(object sender, HealthChangedEventArgs args)
    {
        // If there was no damage this frame, return
        // If the damage this frame is less than the minimum damage for the visual effect, return
        if (ParentComponent.DamageThisFrame < minVFXDamage)
            return;

        // If hit vfx has already played, return
        if (_hasPlayedHitVFX)
            return;
        
        PlayVfx(ParentComponent.DamageThisFrame, args.Position);
    }


    private void DetachVFXOnDeath(object sender, HealthChangedEventArgs args)
    {
        // Return if the visual effect is null
        if (enemyHitEffect == null)
            return;

        // Detach the visual effect
        enemyHitEffect.transform.SetParent(null);

        // Set the visual effect to die after 5 seconds
        Destroy(enemyHitEffect.gameObject, 5f);

        // Force the hasPlayedHitVFX flag to false
        _hasPlayedHitVFX = false;

        // // Play the visual effect
        // PlayVfx(true);
    }

    private void CreateDeathVfx(object sender, HealthChangedEventArgs e)
    {
        // Return if there is no death VFX prefab
        if (enemyDeathVfxPrefab == null)
            return;

        // Instantiate the death VFX prefab
        var deathVfx = Instantiate(enemyDeathVfxPrefab, transform.position, Quaternion.identity);
        deathVfx.Play();

        // Destroy the death VFX after 10 seconds
        Destroy(deathVfx.gameObject, 10f);
    }

    private void PlayVfx(float damage, Vector3 damagePosition)
    {
        // Return if the visual effect is null
        if (enemyHitEffect == null)
            return;

        // Set the visual effect's position to the position of the damage
        enemyHitEffect.SetVector3("StartPosition", damagePosition);

        // Calculate the damage percentage based on the amount of damage the enemy took this frame
        var damagePercentage = Mathf.InverseLerp(minVFXRangeDamage, maxVFXRangeDamage, damage);

        // Set the damage amount float
        enemyHitEffect.SetFloat("DamageAmount", damagePercentage);

        // Play the visual effect
        enemyHitEffect.Play();

        // Set the hasPlayedHitVFX flag to true
        _hasPlayedHitVFX = true;
    }

    private void ComicImpactOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // Return if the comic impact manager is null
        if (ComicImpactManager.Instance == null)
            return;

        // Return if the damager is not a power
        if (e.DamagerObject is not IPower && (e.DamagerObject is not Shotgun && !e.IsCriticalHit))
            return;

        // Return if the comic impact timer is not complete
        if (!_comicImpactTimer.IsComplete)
            return;

        // Reset the comic impact timer
        _comicImpactTimer.Reset();

        if (ParentComponent.CurrentHealth > 0)
            ComicImpactManager.Instance.SpawnImpact(e.Position, e.Actor.GameObject.transform);
        else
            ComicImpactManager.Instance.SpawnImpact(e.Position);

        // // Make the UI a child of the enemy
        // ui.transform.SetParent(transform);
    }

    private void Update()
    {
        // // If the enemy took damage this frame, play the visual effect
        // PlayVFXAfterDamage();

        // Update the comic pow timer
        _comicImpactTimer.Update(Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Reset the damage this frame if the visual effect has played
        _hasPlayedHitVFX = false;
    }
}