using UnityEngine;

[CreateAssetMenu(fileName = "新物品(New Item)", menuName = "物品(Inventory)/道具(Item)")]
public class ItemBase : ScriptableObject
{
    public new string name; // 物品名称
    public Sprite icon; // 物品图标（贴图）
    public string description;
    public int maxStack = 1;
    public ItemType itemType;
    
    public enum ItemType
    {
        装备,
        武器,
        道具,
        其他
    };

    public int damage;
    public int health;
    public string function;
}