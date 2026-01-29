using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "新快捷栏(New BP)", menuName = "物品(Inventory)/快捷栏(QuickBar)")]
public class QuickBar : ScriptableObject
{
    public List<ItemBase> itemList = new List<ItemBase>();
}