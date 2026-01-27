using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "新对话", menuName = "对话组",order =999)]
public class ConversationGroup: ScriptableObject
{
    public string GroupName;
    public List<ConversationUnit> Conversations = new List<ConversationUnit>();

    //指向下一个对话组 可以有多个选项
    public List<ConversationGroup> NextGroup = new List<ConversationGroup>();
    public List<string> NextGroupText = new List<string>();
}