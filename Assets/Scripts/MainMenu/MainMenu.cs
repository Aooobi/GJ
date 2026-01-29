using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("DemoScene");
        Debug.Log("Start Game button clicked!");
        // Add logic to start the game, e.g., load the first level
    }

    public void OpenConfig()
    {
        
    }
}
