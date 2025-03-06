using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApartmentCrashMonster : MonoBehaviour
{
    [SerializeField] public GameObject enemy;

    [SerializeField] public float delay = 3f;
    // Start is called before the first frame update
    void Start()
    {
        enemy.SetActive(false);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);
        enemy.SetActive(true);
    }
}
