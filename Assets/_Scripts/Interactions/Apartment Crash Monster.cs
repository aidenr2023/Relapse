using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApartmentCrashMonster : MonoBehaviour
{
    [SerializeField] public Enemy enemy;
    [SerializeField] public GameObject monsterPrefab;

    [SerializeField] public float delay = 3f;

    // Start is called before the first frame update
    private void Start()
    {
        // Disable the monster at the start
        SetMonster(false);
        monsterPrefab.SetActive(false);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);

        // Enable the monster
        SetMonster(true);
        monsterPrefab.SetActive(true);
    }

    public void SetMonster(bool isOn)
    {
        // enemy.gameObject.SetActive(isOn);
        
        enemy.DetectionBehavior.SetDetectionEnabled(isOn);
    }
}