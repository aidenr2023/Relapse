using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class voidphase2 : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public GameObject Platforms;
    [SerializeField] public GameObject Platform;

    private void Start()
    {
        SetPlatforms(false);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            SetPlatforms(true);
    }

    private void SetPlatforms(bool value)
    {
        if (Platforms != null)
            Platforms.SetActive(value);
        if (Platform != null)
            Platform.SetActive(value);
    }
}