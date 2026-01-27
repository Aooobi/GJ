using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    [SerializeField] public List<ConversationUnit> conversations = new List<ConversationUnit>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ConversationManager.instance.LoadConversation(conversations, this.transform);
        }
    }
}
