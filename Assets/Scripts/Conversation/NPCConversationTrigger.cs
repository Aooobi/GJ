using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// NPC交互触发对话脚本
/// </summary>
public class NPCConversationTrigger : MonoBehaviour
{
    public string conversationName; //需要触发的对话名称
    public bool black_or_not;
    public GameObject F_Interacter_Hint; //交互提示UI
    public bool canInteract = false;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //检测玩家进入触发区域
        if (collision.CompareTag("Player"))
        {
            F_Interacter_Hint.gameObject.SetActive(true);
            canInteract = true;
            //触发对话
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        //检测玩家进入触发区域
        if (collision.CompareTag("Player"))
        {
            F_Interacter_Hint.gameObject.SetActive(true);
            
            //触发对话
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
            if(Input.GetKeyDown(KeyCode.F))
            {
                ConversationManager.instance.LoadConversationByName(conversationName, black_or_not);
                canInteract = false;
            }
        }
    }
}
