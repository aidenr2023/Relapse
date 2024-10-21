using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowText2 : MonoBehaviour
{
    [SerializeField] private GameObject textmeshproObject;
    [SerializeField] private float displayDuration = 3f;
    // Start is called before the first frame update
    void Start()
    {
       if (textmeshproObject != null)
       {
            textmeshproObject.SetActive(false);
       }
       
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (textmeshproObject != null)
            {
                textmeshproObject.SetActive(true);
                StartCoroutine(HideTextAfterDelay(displayDuration));
            }

        }
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (textmeshproObject != null)
        {
            textmeshproObject.SetActive(false);
             Destroy(gameObject);
        }
    }
}