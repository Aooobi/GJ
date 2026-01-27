using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager instance;

    public Text Content;
    public Text Name;
    public GameObject Player; //玩家引用
    [Header("需要控制的显示隐藏面板")]
    //需要控制的显示隐藏 开启对话隐藏 关闭对话显示
    public RectTransform BackgroundPanel;
    //电影黑边
    public Image TopBar;
    public Image BottomBar;

    [Header("对话参数")]
    public int currentIndex = -1;//-1代表未开始对话
    public float TypeSpeed = 0.05f;//打字机速度 每个字的速度
    public float CameraPosDistance = 3f;//摄像机默认到谈话者距离
    private bool isTyping = false;//是否正在打字
    private bool isLastModeClickMode;

    //当前装载的对话组
    public List<ConversationUnit> CurrentConversation;
    private void Awake()
    {
        if(instance==null)
            instance = this;
        else
        {
            Destroy(instance);
            instance = this;
        }
    }

    void Start()
    {
        if (Player == null)
       //     Player = Interactor.Instance_Player; //需要从单例获取
        CurrentConversation = null;
        BackgroundPanel.gameObject.SetActive(false);
        //StartConversation(SpeakerTransform);//测试
    }

    public void StartConversation(Transform target , bool fade_or_not = false)
    {
        if (CurrentConversation == null)
        {
            Debug.LogWarning("对话未装载，无法开始对话");
            return;
        }
        if (fade_or_not)
        {
            //如果需要使用淡入淡出
            //调用FadeOutFadeIn
            UIFadeEffect.Instance.FadeOutAndFadeIn(
                () =>
                {
                    //第一个形参 黑屏时调用
                    //控制对话时产生的电影黑边
                    TopBar.DOFillAmount(1, 0.5f).SetEase(Ease.Linear);
                    BottomBar.DOFillAmount(1, 0.5f).SetEase(Ease.Linear);
                },
                () =>
                {
                    //第二个形参 淡入淡出结束调用
                    //开始对话
                    currentIndex = 0;
                    BackgroundPanel.gameObject.SetActive(true);
                    Go();
                }
                , false);
        }
        else
        {
            //不需要淡入淡出 直接开始对话
            //控制对话时产生的电影黑边
            TopBar.DOFillAmount(1, 0.5f).SetEase(Ease.Linear);
            BottomBar.DOFillAmount(1, 0.5f).SetEase(Ease.Linear);

            //第二个形参 淡入淡出结束调用
            //开始对话
            currentIndex = 0;
            BackgroundPanel.gameObject.SetActive(true);
            Go();
        }
        

    }

    //装载、前进对话
    public void Go()
    {
        if (CurrentConversation == null)
        {
            //对话未装载
            Debug.LogWarning("对话未装载");
            //MessagePool.Instance.CreateMessage("对话未装载", Color.red);
            return;
        }
        if (CurrentConversation.Count == 0)
        {
            Debug.LogWarning("对话未装载");
            //MessagePool.Instance.CreateMessage("对话没有内容", Color.yellow);
            return;
        }
        if (currentIndex < 0)
        {
            Debug.LogWarning("为允许进行对话");
            //MessagePool.Instance.CreateMessage("为允许进行对话", Color.yellow);
            return;
        }
        //首先判断对话是否结束
        if (currentIndex >= CurrentConversation.Count)
        {
            EndConversation();
            return;
        }

        if (!isTyping)
        {
            Content.text = string.Empty;//清空
        }
        else
        {
            //正在打字中，直接显示完整内容
            Content.DOKill();//先杀掉之前的动画
            Content.text = CurrentConversation[currentIndex].Content;
            currentIndex++;
            isTyping = false;
            return;
        }
        //按照字数计算打字时间
        float duration = TypeSpeed * CurrentConversation[currentIndex].Content.Length;
        Name.text = CurrentConversation[currentIndex].SpeakerName;
        Content.DOText(CurrentConversation[currentIndex].Content, duration).SetEase(Ease.Linear)
            .OnStart(() =>
            {
                isTyping = true;
            }).OnComplete(() =>
            {
                currentIndex++;
                isTyping = false;
            });


    }

    /// <summary>
    /// 对外全局加载接口
    /// </summary>
    /// <param name="conversationUnits"></param>
    public void LoadConversation(List<ConversationUnit> conversationUnits,Transform speaker = null)
    {
        if(conversationUnits == null)
        {
            Debug.LogWarning("加载对话组不存在");
            return;
        }
        if(conversationUnits.Count == 0)
        {
            Debug.LogWarning("加载对话组0条消息");
            return;
        }
        if (CurrentConversation != null)
        {
            Debug.Log("已经有对话正在进行");
            return;
        }
        if(speaker == null)
        {
            Debug.Log("没有设置对话对象，摄像机将使用默认位置");
        }

        CurrentConversation = conversationUnits;
        currentIndex = 0;
        StartConversation(speaker);
    }

    public void EndConversation()
    {
        //直接隐藏对话面板
        BackgroundPanel.gameObject.SetActive(false);
        //对话结束 还是调用淡出淡入
        UIFadeEffect.Instance.FadeOutAndFadeIn(() => {
            //第一个形参 黑屏调用
            currentIndex = -1;
            
            //当前装载的对话组清空
            CurrentConversation = null;
            Content.text = string.Empty;
            Name.text = string.Empty;
            
            Debug.Log("对话结束");
        },null,true);//注意第三个参数为true infoPanel显示
    }
}
