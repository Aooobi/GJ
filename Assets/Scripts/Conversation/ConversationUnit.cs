using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConversationUnit
{
    public string SpeakerName;
    [TextArea(3,20)]public string Content;
}
