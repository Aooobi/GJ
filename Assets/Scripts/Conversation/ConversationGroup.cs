using System.Collections.Generic;

[System.Serializable]

public class ConversationGroup
{
    public string GroupName;
    public List<ConversationUnit> Conversations = new List<ConversationUnit>();

    //指向下一个对话组 可以有多个选项
    public List<ConversationGroup> NextGroup = new List<ConversationGroup>();
    public List<string> NextGroupName = new List<string>();
}