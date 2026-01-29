using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CGConversation : MonoBehaviour
{
    public Image CGImage;
    public Image PureBlack;

    public GameObject pingtai;

    void Start()
    {
        
    }

    public void LoadConversation()
    {
        ConversationManager.instance.LoadConversationByName("²¡Î£CG¶Ô»°");
    }
    public void OpenBlack()
    {
        PureBlack.gameObject.SetActive(true);
    }
    public void CloseCG()
    {
        CGImage.gameObject.SetActive(false);
    }

    public void Closepingtai()
    {

    }
}
