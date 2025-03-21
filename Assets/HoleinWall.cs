using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleinWall : MonoBehaviour
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Glass.SetActive(false);
            brokenGlass.SetActive(true);
            audioSource.PlayOneShot(glassBreak, 1);
            Debug.Log(other.tag);
        }
        else if (other.tag == "Player")

        {
            Debug.Log("I don't know what to do here");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        collider.enabled = false;
    }
}
