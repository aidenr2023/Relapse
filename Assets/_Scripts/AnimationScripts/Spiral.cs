using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiral : MonoBehaviour
{
     // Parameters for controlling the spiral movement
        public float height = 5f;       // Maximum height the object will hover at
        public float speed = 2f;        // Speed of the spiral rotation
        public float radius = 2f;       // Radius of the spiral
    
        private Vector3 startPosition;  // The object's starting position
    
        void Start()
        {
            // Save the object's starting position
            startPosition = transform.position;
        }
    
        void Update()
        {
            // Time-based factor for spiral movement
            float time = Time.time * speed;
    
            // Calculate the x, z coordinates based on the radius and time for the spiral
            float x = Mathf.Cos(time) * radius;
            float z = Mathf.Sin(time) * radius;
    
            // Calculate the y coordinate for hovering effect based on sine wave
            float y = Mathf.PingPong(time * height, height);
    
            // Update the object's position to follow the spiral
            transform.position = startPosition + new Vector3(x, y, z);
        }
}
