using UnityEngine;

[CreateAssetMenu(fileName = "新物品(New Item)", menuName = "物品(Inventory)/道具(Item)")]
public class GameItem : ScriptableObject
{
    public new string name; // 物品名称
    public Sprite icon; // 物品图标（贴图）
    public string description;
    [Header("堆叠数量")]
    public int maxStack = 1;
    [Header("物品类型")]
    public ItemType itemType;
    
    public enum ItemType
    {
        装备,
        武器,
        道具,
        其他
    };
    
}