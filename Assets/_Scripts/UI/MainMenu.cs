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
