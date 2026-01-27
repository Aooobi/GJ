using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    public string conveName = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ConversationManager.instance.LoadConversationByName(conveName);
        }
    }
}
