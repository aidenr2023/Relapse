using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void StartButton()
    {
        SceneManager.LoadScene("ApartmentBlockout");
    }
    public void ExitButton()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
