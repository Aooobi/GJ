using UnityEngine;

[CreateAssetMenu(fileName = "新物品(New Item)", menuName = "物品(Inventory)/道具(Item)")]
public class ItemBase : ScriptableObject
{
    public string id;
    public new string name; // 物品名称
    public Sprite icon; // 物品图标（贴图）
    [TextArea]
    public string description;
    public int maxStack = 1;
    public int itemHeld; //这个变量的设计存在争议，因为只能适用于单人游戏并且需要记住最后导出之前要清除所有已有游玩痕迹，所有itemHeld需要重置为0
    public ItemType itemType;
    
    public enum ItemType
    {
        饰品,
        武器,
        道具,
        消耗品,
        其他
    };

    public ItemAdditionalData additionalData;
    public MonoBehaviour functionScript;
    public int damage;
    public int health;
    [TextArea]
    public string function;

}