using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingOrb : MonoBehaviour
{
    [SerializeField] public GameObject section;
    [SerializeField] public GameObject orb;
    [SerializeField] public GameObject autoOrb;
    [SerializeField] public Animator animator;
    [SerializeField] public AudioClip clip;

    [SerializeField] public float delay= 10f;
    // Start is called before the first frame update
    void Start()
    {
        section.SetActive(false);
        orb.SetActive(true);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            section.SetActive(true);
            orb.SetActive(false);
            animator.Play("Section 1");
            StartCoroutine(Remove());
        }
    }

    IEnumerator Remove()
    {
        yield return new WaitForSeconds(delay);
        autoOrb.SetActive(false);
    }
}
