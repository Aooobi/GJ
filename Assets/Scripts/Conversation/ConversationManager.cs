using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

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
    public List<ConversationGroup> AllConversation = new List<ConversationGroup>();

    public ConversationGroup currentConversation;
    //public List<ConversationUnit> CurrentConversation;

    [Header("预制体")]
    public RectTransform OptionParent;//gridlayout
    public Button OptionPrefab;
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
            currentConversation = null;
        BackgroundPanel.gameObject.SetActive(false);
        //StartConversation(SpeakerTransform);//测试

        //初始化所有对话组
        var allConversations = Resources.LoadAll<ConversationGroup>("ConversationGroups"); 
        foreach(var convo in allConversations)
        {
            AllConversation.Add(convo);
        }

        LoadConversationByName("测试对话1");
    }

    public void StartConversation(bool fade_or_not = false)
    {
        if (currentConversation == null)
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

            //开始对话
            currentIndex = 0;
            BackgroundPanel.gameObject.SetActive(true);
            Go();
        }
        

    }

    //装载、前进对话
    public void Go()
    {
        if (currentConversation == null || currentConversation.Conversations == null)
        {
            //对话未装载
            Debug.LogWarning("对话未装载");
            //MessagePool.Instance.CreateMessage("对话未装载", Color.red);
            return;
        }
        if (currentConversation.Conversations.Count == 0)
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
        if (currentIndex >= currentConversation.Conversations.Count)
        {
            //如果该对话组没有后续的对话组 直接结束对话
            if(currentConversation.NextGroup.Count == 0)
            {
                //没有下一组对话 结束对话
                EndConversation();
            }
            else
            {
                //有后续对话组 那么就按照下一组对话的string生成选项 选项绑定前往对应对话组
                //注意 选项的Text 实际上就是玩家对上一个对话组最后一个Content的回答
                if (OptionParent.childCount > 0) return; //已经生成选项按钮 则不再生成
                                                         //TODO 根据TExt生成选项按钮
                                                         // 修改Go()方法中的选项生成部分：
                for (int i = 0; i < currentConversation.NextGroup.Count; i++)
                {
                    // 关键：创建局部变量
                    int index = i;
                    Button option = Instantiate<Button>(OptionPrefab, OptionParent, false);
                    option.GetComponentInChildren<Text>().text = currentConversation.NextGroupText[index];

                    option.onClick.AddListener(() =>
                    {
                        // 使用局部变量index，而不是i
                        currentConversation = currentConversation.NextGroup[index];
                        currentIndex = 0;

                        // 清空显示
                        Content.text = string.Empty;
                        Name.text = string.Empty;

                        // 销毁所有选项按钮
                        foreach (Transform child in OptionParent)
                        {
                            Destroy(child.gameObject);
                        }

                        // 开始新对话（不需要调用Go()，StartConversation会调用）
                        StartConversation();
                    });
                }

            }

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
            Content.text = currentConversation.Conversations[currentIndex].Content;
            currentIndex++;
            isTyping = false;
            return;
        }
        //按照字数计算打字时间
        float duration = TypeSpeed * currentConversation.Conversations[currentIndex].Content.Length;
        Name.text = currentConversation.Conversations[currentIndex].SpeakerName;
        Content.DOText(currentConversation.Conversations[currentIndex].Content, duration).SetEase(Ease.Linear)
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
    public void LoadConversationByName(string conversationName,bool black_or_not = false)
    {
        if (!BackgroundPanel.gameObject.activeInHierarchy)
        {
            BackgroundPanel.gameObject.SetActive(true);
        }


        for (int i = 0; i < AllConversation.Count; i++)
        {
            //找到对应名字的对话组
            if (AllConversation[i].name == conversationName)
            {
                currentConversation = AllConversation[i];
                currentIndex = 0;
                StartConversation(black_or_not);
                break;
            }
        }

    }

    public void EndConversation(bool fade_or_not = false)
    {
        //直接隐藏对话面板
        BackgroundPanel.gameObject.SetActive(false);
        //电影黑边收起
        TopBar.DOFillAmount(0,0.5f).SetEase(Ease.Linear);
        BottomBar.DOFillAmount(0, 0.5f).SetEase(Ease.Linear);
        //对话结束 还是调用淡出淡入
        if (fade_or_not)
        {
            UIFadeEffect.Instance.FadeOutAndFadeIn(() => {
                //第一个形参 黑屏调用
                currentIndex = -1;

                //当前装载的对话组清空
                currentConversation = null;
                Content.text = string.Empty;
                Name.text = string.Empty;

                Debug.Log("对话结束");
            }, null, true);
        }
        else
        {
            //false时不需要
            //直接对话结束
            //第一个形参 黑屏调用
            currentIndex = -1;

            //当前装载的对话组清空
            currentConversation = null;
            Content.text = string.Empty;
            Name.text = string.Empty;

            Debug.Log("对话结束");

        }
        //注意第三个参数为true infoPanel显示
    }
}
