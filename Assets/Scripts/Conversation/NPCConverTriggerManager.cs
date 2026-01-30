using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCConverTriggerManager : MonoBehaviour
{
    public static NPCConverTriggerManager Instance;

    public NPCConversationTrigger 初;
    public NPCConversationTrigger 费米;
    public NPCConversationTrigger 艾米;
    public NPCConversationTrigger 艾莉;
    public NPCConversationTrigger 米特;
    public NPCConversationTrigger 加里尔;
    public NPCConversationTrigger 弗恩;

    private void Awake()
    {
        if(Instance==null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        //调用示例
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.初, "二层-与初对话");
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.费米, "二层-与费米对话");
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.艾米, "二层-与艾米对话");
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.艾莉, "二层-与艾莉对话");
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.米特, "二层-与米特对话");
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.加里尔, "二层-与加里尔对话");
        //NPCConverTriggerManager.Instance.SetNewConversation(NPCConverTriggerManager.Instance.弗恩, "二层-与弗恩对话");
    }
    public void SetNewConversation(NPCConversationTrigger npc_trigger,string target_name)
    {
        npc_trigger.conversationName = target_name;
    }
}
