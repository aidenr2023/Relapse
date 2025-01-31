using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabSystem : MonoBehaviour
{
    [SerializeField] private GameObject[] tabs;

    GameObject currentTab;
    GameObject nextTab;
    GameObject previousTab;

    int currentTabIndex;
    int nextTabIndex;
    int previousTabIndex;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HideTabs()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
    }
    public void GetCurrentTab()
    {
        
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].activeSelf)
            {
                
                //Set Current Tab
                GameObject currentTab = tabs[i];
                Debug.Log("Current Tab: " + currentTab);
                currentTabIndex = i;
                GetNextTab();
                GetPreviousTab();

                
                //Debug.Log("Current Tab Index: " + currentTabIndex);
                Debug.Log("Current Tab: " + currentTab);

                //Debug.Log("Next Tab Index: " + nextTabIndex);
                Debug.Log("Next Tab: " + nextTab);

                //Debug.Log("Previous Tab Index: " + previousTabIndex);
                Debug.Log("Previous Tab: " + previousTab);
                
            }
        }
    }
    public void MoveTabs()
    {
        
    }

    public void GetNextTab()
    {
        
        if (currentTabIndex == tabs.Length-1)
        {
            
            nextTabIndex = 0;
        }
        else
        {
            nextTabIndex = currentTabIndex + 1;
        }
        nextTab = tabs[nextTabIndex];

        

    }
    public void GetPreviousTab()
    {
        if(currentTabIndex == 0)
        {
            previousTabIndex = tabs.Length - 1;
        }
        else
        {
            previousTabIndex = currentTabIndex - 1;
        }
        previousTab = tabs[previousTabIndex];
    }

}