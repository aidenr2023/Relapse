using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatingObjects : MonoBehaviour

{
    [SerializeField] public GameObject floatingObject;
    [SerializeField] public float delay = 5f;
    [SerializeField] public Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
    floatingObject.SetActive(false);    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            floatingObject.SetActive(true);
            animator.Play("Floating object");
           
        }
    }
    
   
}

