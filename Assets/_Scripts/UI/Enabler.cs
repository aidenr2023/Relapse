using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enabler : MonoBehaviour
{

    [SerializeField] private GameObject[] itemsToEnable;
    [SerializeField] private GameObject[] itemsToHide;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideItems()
    {
        for (int i = 0; i < itemsToHide.Length; i++)
        {
            itemsToHide[i].SetActive(false);
        }
    }
    public void ShowItems()
    {
        for (int i = 0; i < itemsToEnable.Length; i++)
        {
            itemsToEnable[i].SetActive(true);
        }
    }
}
