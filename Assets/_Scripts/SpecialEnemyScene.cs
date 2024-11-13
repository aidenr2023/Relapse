using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEnemyScene : MonoBehaviour
{
    public Rigidbody enemyRigidbody;
    public Rigidbody doorRigidbody;
    public float walkSpeed = 2f;
    public Transform targetPoint;
    public float disappearDelay = 3f;
    public GameObject enemy;

    public bool doorknocked = false;
    
    public void Start()
    {
        enemy.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !doorknocked)
        {
            KnockOverDoor();
            enemy.SetActive(true);
        }
    }

    private void KnockOverDoor()
    {
        doorknocked = true;

        if(doorRigidbody != null)
        {
            doorRigidbody.constraints = RigidbodyConstraints.None;
            doorRigidbody.useGravity = true;
        }
       

        StartCoroutine(MoveThroughDoor());
    }

    private IEnumerator MoveThroughDoor()
    {
        yield return new WaitForSeconds(0f);

        while (Vector3.Distance(enemyRigidbody.position, targetPoint.position) > 0.1f)
        {
            enemyRigidbody.MovePosition(Vector3.MoveTowards(enemyRigidbody.position, targetPoint.position, walkSpeed * Time.deltaTime));
            yield return null;
        }

        yield return new WaitForSeconds(disappearDelay);
        enemyRigidbody.gameObject.SetActive(false);
        Debug.Log("Enemy left.");
    }
}
