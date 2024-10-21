using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowText : MonoBehaviour
{
    [SerializeField] private GameObject textmeshproObject;
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
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (textmeshproObject != null)
            {
                textmeshproObject.SetActive(false);
            }
        }
    }
}