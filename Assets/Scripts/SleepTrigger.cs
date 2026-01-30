using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepTrigger : MonoBehaviour
{
    public string conversationName; //需要触发的对话名称
    public bool black_or_not;
    public GameObject F_Interacter_Hint; //交互提示UI
    public bool canInteract = false;
    public RPG Player;

    [Header("Sprite Renderer显示隐藏")]
    // 新添加的属性：控制SpriteRenderer的enable
    public bool controlSpriteRenderer = false;

    // 用于存储SpriteRenderer组件
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // 在Start中获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //检测玩家进入触发区域
        if (collision.CompareTag("Player"))
        {
            F_Interacter_Hint.gameObject.SetActive(true);
            canInteract = true;
            //触发对话
            ConversationManager.instance.LoadConversationByName(conversationName, black_or_not);
            Player.StartPlayerMovement();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //检测玩家进入触发区域
        if (collision.CompareTag("Player"))
        {
            F_Interacter_Hint.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //检测玩家进入触发区域
        if (collision.CompareTag("Player"))
        {
            F_Interacter_Hint.gameObject.SetActive(false);
            canInteract = false;
            //触发对话
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                //睡觉逻辑
                Player.PlayerDoSleep();
                canInteract = false;
            }
        }
    }

    // 当Inspector中的值发生变化时调用（仅在编辑器模式下）
    private void OnValidate()
    {
        // 如果控制开关被勾选，则立即获取SpriteRenderer并设置其enable状态
        if (controlSpriteRenderer && Application.isEditor)
        {
            // 如果还没有获取SpriteRenderer，则立即获取
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            // 如果找到了SpriteRenderer，则启用它
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
        }
        // 如果控制开关被取消勾选，则禁用SpriteRenderer
        else if (!controlSpriteRenderer && spriteRenderer != null && Application.isEditor)
        {
            spriteRenderer.enabled = false;
        }
    }
}
