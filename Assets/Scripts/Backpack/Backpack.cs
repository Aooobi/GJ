using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "新背包(New BP)", menuName = "物品(Inventory)/背包(BP)")]
public class Backpack : ScriptableObject
{
    public List<ItemBase> itemList = new List<ItemBase>();
}
