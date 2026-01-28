using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 消息UI对象池
/// </summary>
public class MessagePool : MonoBehaviour
{
    public static MessagePool Instance;

    [Header("对象池配置")]
    public Transform poolParent; // 对象池父节点（隐藏的）
    public Transform messagePanel; // 显示消息的UI面板

    [Header("预制体")]
    [SerializeField] private MessageUI messageUIPrefab;

    // 对象池
    private Stack<MessageUI> pool = new Stack<MessageUI>();
    private List<MessageUI> activeMessages = new List<MessageUI>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 如果没有设置父节点，使用自己
        if (poolParent == null) poolParent = transform;

        // 如果没有设置消息面板，尝试查找
        if (messagePanel == null)
        {
            GameObject panel = gameObject;
            if (panel != null)
            {
                messagePanel = panel.transform;
            }
        }

        // 初始化池
        InitializePool(10);

        //测试
        //CreateMessage("Welcome to the Message System!", Color.green);
        //CreateMessage("Welcome to the Message System!", Color.green);
        //CreateMessage("Welcome to the Message System!", Color.red);
        //CreateMessage("Welcome to the Message System!", Color.yellow);

    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool(int initialSize)
    {
        for (int i = 0; i < initialSize; i++)
        {
            GenerateMessageUIInPool();
        }
    }

    /// <summary>
    /// 在池中创建新消息UI
    /// </summary>
    private void GenerateMessageUIInPool()
    {
        if (messageUIPrefab == null)
        {
            Debug.LogError("MessageUI prefab is not assigned!");
            return;
        }

        MessageUI messageUI = Instantiate(messageUIPrefab, poolParent);
        messageUI.gameObject.SetActive(false);
        pool.Push(messageUI);
    }

    /// <summary>
    /// 从对象池获取消息UI
    /// </summary>
    public MessageUI CreateMessage(string text, Color color)
    {
        MessageUI messageUI;

        // 如果池为空，创建新的
        if (pool.Count == 0)
        {
            GenerateMessageUIInPool();
        }

        // 从池中取出
        messageUI = pool.Pop();

        // 设置到消息面板
        if (messagePanel != null)
        {
            messageUI.transform.SetParent(messagePanel, false);
        }

        // 初始化消息内容
        InitializeMessageUI(messageUI, text, color);

        // 激活并添加到活跃列表
        messageUI.gameObject.SetActive(true);
        activeMessages.Add(messageUI);
        messageUI.AutoFade();//调用一下自动淡出方法

        return messageUI;
    }

    /// <summary>
    /// 初始化消息UI数据
    /// </summary>
    private void InitializeMessageUI(MessageUI messageUI, string text, Color color)
    {
        if (messageUI == null) return;

        // 设置文本和颜色
        if (messageUI.messageText != null)
        {
            messageUI.messageText.text = text;
            messageUI.messageText.color = color;
        }
    }

    /// <summary>
    /// 归还消息UI到对象池
    /// </summary>
    public void ReturnMessageUIToPool(MessageUI messageUI)
    {
        if (messageUI == null) return;

        // 从活跃列表中移除
        if (activeMessages.Contains(messageUI))
        {
            activeMessages.Remove(messageUI);
        }

        // 重置状态
        ResetMessageUIState(messageUI);

        // 移动到对象池
        messageUI.transform.SetParent(poolParent, false);
        messageUI.gameObject.SetActive(false);

        // 放回池中
        pool.Push(messageUI);
    }

    /// <summary>
    /// 重置消息UI状态
    /// </summary>
    private void ResetMessageUIState(MessageUI messageUI)
    {
        if (messageUI == null) return;

        // 重置文本和颜色
        if (messageUI.messageText != null)
        {
            messageUI.messageText.text = "";
            messageUI.messageText.color = Color.white;
        }

        // 重置变换
        messageUI.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 清空所有活跃消息UI
    /// </summary>
    public void ClearAllActiveMessages()
    {
        var messagesToReturn = new List<MessageUI>(activeMessages);

        foreach (var messageUI in messagesToReturn)
        {
            ReturnMessageUIToPool(messageUI);
        }

        activeMessages.Clear();
    }

    /// <summary>
    /// 获取所有活跃消息
    /// </summary>
    public List<MessageUI> GetAllActiveMessages()
    {
        return new List<MessageUI>(activeMessages);
    }
}