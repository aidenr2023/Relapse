using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour, IPower
{
    // a field for a projectile prefab
    [SerializeField] private GameObject projectilePrefab;

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken) => string.Empty;

    #region IPower Methods

    public void StartCharge(PlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
    }

    public void Charge(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(PlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
    }

    public void Use(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Create a vector that points forward from the camera pivot
        var fireForward = powerManager.Player.PlayerController.CameraPivot.transform.forward;

        // Create the position of the projectile
        var firePosition = powerManager.Player.PlayerController.CameraPivot.transform.position + fireForward * 1;

        // Instantiate the projectile prefab
        var projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);

        // Get the IPowerProjectile component from the projectile
        var powerProjectile = projectile.GetComponent<IPowerProjectile>();

        // Shoot the projectile
        powerProjectile.Shoot(this, powerManager, pToken, firePosition, fireForward);

        // // Create the projectile
        // var projectileScript = CreateProjectile(firePosition, fireForward);
        //
        // // Set up the projectile
        // SetUpProjectile(powerManager, projectileScript);
    }

    private ScriptExtender CreateProjectile(Vector3 pos, Vector3 forward)
    {
        // Create a cube primitive
        var newProjectile = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Set the position of the cube to the fire position
        newProjectile.transform.position = pos;

        // Set the forward of the cube to the forward vector
        newProjectile.transform.forward = forward;

        // Add a script extender to the projectile
        var scriptExtender = newProjectile.AddComponent<ScriptExtender>();

        // Add a rigidbody to the projectile through the script extender
        scriptExtender.ExtenderAddComponent<Rigidbody>();

        // Make the current collider a trigger
        var projectileCollider = scriptExtender.GetComponent<Collider>();
        projectileCollider.isTrigger = true;

        // Return the script extender connected to the projectile
        return scriptExtender;
    }

    private void SetUpProjectile(PlayerPowerManager powerManager, ScriptExtender scriptExtender)
    {
        // Add a function to the script extender that runs when the projectile is updated
        scriptExtender.OnObjectFixedUpdate += FireballMovement;

        // Add a function to the script extender that runs when the projectile hits something
        scriptExtender.TriggerEnter += FireballTriggerEnter;

        // Destroy the projectile after 5 seconds
        Destroy(scriptExtender.gameObject, 5);

        return;

        // Create a function that runs when the projectile is updated
        void FireballMovement(ScriptExtender obj)
        {
            var rb = obj.ExtenderGetComponent<Rigidbody>();

            // Add force in the forward direction
            rb.AddForce(obj.transform.forward * 10, ForceMode.Impulse);
        }

        void FireballTriggerEnter(ScriptExtender obj, Collider other)
        {
            // Return if the projectile hits sender of the projectile
            if (other.gameObject == powerManager.gameObject)
                return;

            // Return if the other object is a trigger
            if (other.isTrigger)
                return;

            // If the projectile hits something with an IActor component, deal damage
            if (other.TryGetComponent(out IActor actor))
                actor.ChangeHealth(-100, powerManager.Player.PlayerInfo, this, obj.transform.position);

            // Destroy the projectile when it hits something
            Debug.Log($"BOOM! {obj.name} hit {other.name}");
            Destroy(obj.gameObject);

            // TODO: Make an explosion of some sort
        }
    }

    #region Active Effects

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion


    #region Passive Effects

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion

    #endregion
}