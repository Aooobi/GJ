using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CGConversation : MonoBehaviour
{
    public Image CGImage;
    public Image PureBlack;

    public GameObject pingtai;

    public Action OnConversationOver;

    void Start()
    {
        OnConversationOver += () => {
            CloseBlack();
        };
    }

    public void LoadConversation()
    {
        ConversationManager.instance.LoadConversationByName("病危CG对话");
    }
    public void OpenBlack()
    {
        PureBlack.gameObject.SetActive(true);
    }
    public void CloseBlack()
    {
        PureBlack.gameObject.SetActive(false);
    }
    public void CloseCG()
    {
        CGImage.gameObject.SetActive(false);
    }

    public void Closepingtai()
    {

    }

    public void Stop1BGM()
    {
        AudioManager.Instance.StopBGM();
    }

    public void PlayXindian()
    {
        AudioManager.Instance.PlayBGM("心电");
    }
}
