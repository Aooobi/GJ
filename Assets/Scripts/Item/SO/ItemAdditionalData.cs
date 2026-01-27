using UnityEngine;

// 用于扩展物品功能的附加数据基类
public class ItemAdditionalData : ScriptableObject
{
    [TextArea]
    public string description = "描述信息";
    
    // 可以被继承以提供特定功能的虚方法
    public virtual void OnItemUsed()
    {
        Debug.Log($"使用了 {this.name} 物品");
    }
    
    public virtual void OnItemEquipped()
    {
        Debug.Log($"{this.name} 物品已装备");
    }
    
    public virtual void OnItemUnequipped()
    {
        Debug.Log($"{this.name} 物品已卸下");
    }
}