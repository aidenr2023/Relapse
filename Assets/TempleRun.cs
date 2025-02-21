using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleRun : MonoBehaviour
{
      public Animator anim;
      public GameObject debris;
    [SerializeField] public AudioClip debrisBreak;
     [SerializeField] public AudioSource audioSource;
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
               debris.SetActive(true); 
               audioSource.PlayOneShot(debrisBreak, 1);
        }
    }
}
