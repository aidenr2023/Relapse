using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private bool isSpinning = true;

    [SerializeField] private float timeToSpin = 1f;

    [SerializeField] private bool isMovingUpAndDown = true;

    [SerializeField] private float timeToMove = 1f;

    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Spin the object around the y-axis
        if (isSpinning)
            transform.Rotate(0, 360 * Time.deltaTime / timeToSpin, 0);

        if (isMovingUpAndDown)
        {
            var newY = Mathf.PingPong(Time.time / timeToMove, 1) * (maxY - minY) + minY;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }


    }
}
