using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletePlatform : MonoBehaviour
{
    [SerializeField]  GameObject Platform;
[SerializeField]  GameObject Platform2;
[SerializeField] private float delay = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(Delete());
        }
    }

    private IEnumerator Delete ()
    {
        yield return new WaitForSeconds(delay);
        Platform.SetActive(false);
        Platform2.SetActive(false);
    }
}
