using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 用于新手引导对话的特殊对话触发器
/// </summary>
public class ConversationSpecialTrigger : MonoBehaviour
{
    public string conveName;


    private bool isConStart = false;
    private bool isConPanelOopen = false;
    private bool flag = false;

    public Transform targetPoint;//传送目标点

    void Start()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isConStart = true;
            ConversationManager.instance.LoadConversationByName(conveName, true);
            isConPanelOopen = true;
            //停止玩家移动
        }
    }
    private void Update()
    {
        if (ConversationManager.instance.currentConversation == null)
        {
            isConPanelOopen = false;
        }
        if (isConStart && !isConPanelOopen)
        {
            if (!flag)
            {
                //对话结束 且未标记
                TeleportPlayerToViliage();
                flag = true;
            }
        }
    }

    private void TeleportPlayerToViliage()
    {
        UIFadeEffect.Instance.FadeOutAndFadeIn(4f,
            () =>
            {
                //传送玩家
                // 传送玩家到目标位置
                GameObject.FindWithTag("Player").transform.position = targetPoint.position;

            },
            () =>
            {
                //恢复玩家控制
                //隐藏触发器
                gameObject.SetActive(false);
                //顺便播放背景音乐
                AudioManager.Instance.PlayBGM("花火镇BGM");
            }, false);
    }
}
