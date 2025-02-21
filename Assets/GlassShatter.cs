using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassShatter : MonoBehaviour
{
    [SerializeField] public GameObject brokenGlass;
    [SerializeField] public GameObject Glass;
    [SerializeField] public AudioClip glassBreak;
     [SerializeField] public AudioSource audioSource;
     [SerializeField] public Collider collider;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void OnTriggerEnter (Collider other)
    {
        if(other.tag == "Player")
    {
        Glass.SetActive(false);
        brokenGlass.SetActive(true);
        audioSource.PlayOneShot(glassBreak, 1);
    }
    }
    private void OnTriggerExit(Collider other)
    {
        collider.enabled = false;
    }
}
