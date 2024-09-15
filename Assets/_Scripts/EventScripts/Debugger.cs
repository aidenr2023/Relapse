using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the cube
        Gizmos.color = Color.blue;
        Vector3 directionFW = transform.TransformDirection(Vector3.forward) * 5;
        Gizmos.DrawRay(transform.position, directionFW);

        // draws a 5 unit long green line left of cube 
        Gizmos.color = Color.green;
        Vector3 directionLeft = transform.TransformDirection(Vector3.left) * 5;
        Gizmos.DrawRay(transform.position, directionLeft);

        //draws a 5 unit long green line right of the cube
        Gizmos.color = Color.red;
        Vector3 directionRight = transform.TransformDirection(Vector3.right) * 5;
        Gizmos.DrawRay(transform.position, directionRight);
    }
}
