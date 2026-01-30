using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public string SceneName = "DemoScene";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// 加载到游戏场景
    /// </summary>
    public void LoadGameScene()
    {
        SceneManager.LoadScene(SceneName);
    }
}
