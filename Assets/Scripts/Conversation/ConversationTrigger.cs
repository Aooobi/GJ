using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    public string conveName = "";
    public bool black_or_not = false;//进入该对话是否变黑屏
    public bool isOnce = false;//是否只触发一次
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ConversationManager.instance.LoadConversationByName(conveName, black_or_not);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isOnce)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
