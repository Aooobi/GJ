using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public RectTransform ConfigPanel;

    private bool isConfigOpen = false;
    public void StartGame()
    {
        SceneManager.LoadScene("DemoScene");
        Debug.Log("Start Game button clicked!");
        // Add logic to start the game, e.g., load the first level

        //AudioManager.Instance.PlayBGM("ª®ª’ÚBGM");
    }

    public void OpenOrCloseConfig()
    {
        if (!isConfigOpen)
        {
            ConfigPanel.DOAnchorPosX(-444, 0.5f);
            isConfigOpen = true;
        }
        else
        {
            ConfigPanel.DOAnchorPosX(444, 0.5f);
            isConfigOpen = false;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game button clicked!");
        Application.Quit();
    }
}
