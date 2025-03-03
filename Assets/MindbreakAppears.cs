using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindbreakAppears : MonoBehaviour
{
    [SerializeField] public GameObject mindbreak;
    // Start is called before the first frame update
    void Start()
    {
        mindbreak.SetActive(false);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            mindbreak.SetActive(true);
    }
}
