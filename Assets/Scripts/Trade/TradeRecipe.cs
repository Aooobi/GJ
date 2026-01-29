using UnityEngine;


[CreateAssetMenu(menuName = "兑换/兑换方案")]
public class TradeRecipe : ScriptableObject
{
    public string recipeId;
    public string displayName;

    [Header("消耗的物品（输入）")]
    public ItemBase[] inputs;
    public int[] inputAmounts;

    [Header("获得的物品（输出）")]
    public ItemBase[] outputs;
    public int[] outputAmounts;

    [Header("其他设置")]
    public bool isReversible = false; // 是否可反向兑换
    public Sprite icon; // 可选：显示在交易 UI 中

}