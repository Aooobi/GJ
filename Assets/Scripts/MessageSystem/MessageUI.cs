using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MessageUI : MonoBehaviour
{
    public Text messageText; // UI Text组件
    public float creationTime = 2; // 存在时间 
    public float fadeTime = 0.6f;// 用于自动消失


    public void AutoFade()
    {
        //创建后使用Dotween消失
        messageText.DOFade(0, fadeTime).SetDelay(creationTime).OnComplete(() =>
        {
            MessagePool.Instance.ReturnMessageUIToPool(this);
        });
    }
}