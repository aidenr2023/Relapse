using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletePlatform : MonoBehaviour
{
    [SerializeField] GameObject Platform;
    [SerializeField] GameObject Platform2;
    [SerializeField] private float delay = 10f;

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            StartCoroutine(Delete());
    }

    private IEnumerator Delete()
    {
        yield return new WaitForSeconds(delay);

        if (Platform != null)
            Platform.SetActive(false);

        if (Platform2 != null)
            Platform2.SetActive(false);
    }
}