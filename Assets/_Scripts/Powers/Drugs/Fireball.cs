using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour, IPower
{
    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    #region IPower Methods

    public void StartCharge(TestPlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
    }

    public void Charge(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(TestPlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
    }

    public void Use(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        // Create a vector that points forward from the camera pivot
        var fireForward = powerManager.Player.PlayerController.CameraPivot.transform.forward;

        // Create the position of the projectile
        var firePosition = powerManager.Player.PlayerController.CameraPivot.transform.position + fireForward * 1;

        // Create the projectile
        var projectileScript = CreateProjectile(firePosition, fireForward);

        // Set up the projectile
        SetUpProjectile(powerManager, projectileScript);
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

    private void SetUpProjectile(TestPlayerPowerManager powerManager, ScriptExtender scriptExtender)
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
            
            // Destroy the projectile when it hits something
            Debug.Log($"BOOM! {obj.name} hit {other.name}");
            Destroy(obj.gameObject);
        }
    }

    #region Active Effects

    public void StartActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion


    #region Passive Effects

    public void StartPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion

    #endregion
}