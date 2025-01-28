using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneScript2 : MonoBehaviour
{

    //Script for the drone to play hover animation and sound while following paths through moving transform thorugh waypoints
    
    //get transform of drone 
    private Transform droneTransform;
    
    //get the rigidbody of the drone
    private Rigidbody droneRigidbody;
    
    //get the original position of the drone
    private Vector3 originalPosition;
    
    [SerializeField] private Transform[] waypoints;
    
    private int currentWaypointIndex = 0;
    
    private void Start()
    {
        //get the transform of the drone
        droneTransform = transform;
        
        //get the rigidbody of the drone
        droneRigidbody = GetComponent<Rigidbody>();
        
        //set the original position of the drone
        originalPosition = droneTransform.position;
        
        //set the drone to be kinematic
        droneRigidbody.isKinematic = true;
    }
    
    private void Update()
    {
        //call the hover function
       // Hover();
        
        //call the follow path function
        FollowPath();
    }

    private void FollowPath()
    {
        //check if the waypoints are null or empty
        if (waypoints == null || waypoints.Length == 0)
        {
            //log a warning message
            Debug.LogWarning("No waypoints assigned to the drone.");
            return;
            
            //get the target waypoint
            Transform targetWaypoint = waypoints[currentWaypointIndex];
            
            
            
        }
    }
    
    //start a coroutine to make the drone explode after player lands on it
    private IEnumerator ExplodeDrone()
    {
        //wait for 5 seconds
        yield return new WaitForSeconds(2f);
        
        //destroy the drone
        Destroy(gameObject);
    }
    
    
}
