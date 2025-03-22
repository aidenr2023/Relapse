using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HoleinWall : MonoBehaviour
{
    [FormerlySerializedAs("brokenGlass")] [SerializeField] public GameObject AnimatedWall;
    [FormerlySerializedAs("Glass")] [SerializeField] public GameObject StaticWall;
    [FormerlySerializedAs("glassBreak")] [SerializeField] public AudioClip BreakSound;
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
            StaticWall.SetActive(false);
            AnimatedWall.SetActive(true);
            audioSource.PlayOneShot(BreakSound, 1);
            Debug.Log(other.CompareTag("Enemy"));
        }
        else
        {
            Debug.Log("No Tag read");
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     collider.enabled = false;
    // }
}
