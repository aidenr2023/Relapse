using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleRun : MonoBehaviour
{
      public Animator anim;
      public GameObject debris;
    [SerializeField] public AudioClip debrisBreak;
     [SerializeField] public AudioSource audioSource;
     [SerializeField] public Collider collider;
     [SerializeField] public GameObject wall;
      [SerializeField] public GameObject shatteredwall;
      [SerializeField] public GameObject doorFrame;
       [SerializeField] public GameObject brokenFrame;

       [SerializeField] public GameObject particle;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
               anim.Play("BreakableWall");
               shatteredwall.SetActive(true);
               wall.SetActive(false);
               debris.SetActive(true);
               doorFrame.SetActive(false);
               brokenFrame.SetActive(true); 
               audioSource.PlayOneShot(debrisBreak, 1);
               particle.SetActive(true);
               StartCoroutine(End());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        collider.enabled = false;
    }

    IEnumerator End ()
    {
        yield return new WaitForSeconds(5);
        particle.SetActive(false);
    }
}
