using System.Collections.Generic;
using UnityEngine;

public static class BackpackExtension
{
    /// <summary>
    /// 向背包中添加物品
    /// </summary>
    /// <param name="backpack">背包实例</param>
    /// <param name="itemId">物品ID</param>
    /// <param name="amount">添加数量</param>
    /// <returns>是否成功添加</returns>
    public static bool AddItem(this Backpack backpack, string itemId, int amount)
    {
        if (backpack == null) return false;
        
        // 查找对应ID的物品
        ItemBase item = FindItemById(itemId);
        if (item == null) return false;

        // 检查背包中是否已有该物品
        ItemBase existingItem = backpack.itemList.Find(i => i.id == itemId);
        
        if (existingItem != null)
        {
            // 如果已有该物品，增加数量直到达到最大堆叠数
            int remainingToAdd = amount;
            while (remainingToAdd > 0)
            {
                int spaceInCurrentStack = existingItem.maxStack - existingItem.itemHeld;
                int addToThisStack = Mathf.Min(spaceInCurrentStack, remainingToAdd);
                
                if (addToThisStack > 0)
                {
                    existingItem.itemHeld += addToThisStack;
                    remainingToAdd -= addToThisStack;
                }
                else
                {
                    // 当前堆栈已满，尝试添加新的堆栈
                    if (backpack.itemList.Count < 999) // 假设背包有容量限制
                    {
                        ItemBase newItem = CreateNewItemCopy(item);
                        newItem.itemHeld = remainingToAdd;
                        backpack.itemList.Add(newItem);
                        remainingToAdd = 0;
                    }
                    else
                    {
                        Debug.LogWarning("背包已满，无法添加更多物品");
                        return false;
                    }
                }
            }
        }
        else
        {
            // 如果没有该物品，添加新物品
            if (amount <= item.maxStack)
            {
                ItemBase newItem = CreateNewItemCopy(item);
                newItem.itemHeld = amount;
                backpack.itemList.Add(newItem);
            }
            else
            {
                // 如果数量超过最大堆叠数，需要分成多个堆栈
                int remainingAmount = amount;
                while (remainingAmount > 0)
                {
                    if (backpack.itemList.Count >= 999) // 假设背包有容量限制
                    {
                        Debug.LogWarning("背包已满，无法添加更多物品");
                        return false;
                    }
                    
                    int addToThisStack = Mathf.Min(item.maxStack, remainingAmount);
                    ItemBase newItem = CreateNewItemCopy(item);
                    newItem.itemHeld = addToThisStack;
                    backpack.itemList.Add(newItem);
                    remainingAmount -= addToThisStack;
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// 从背包中移除物品
    /// </summary>
    /// <param name="backpack">背包实例</param>
    /// <param name="itemId">物品ID</param>
    /// <param name="amount">移除数量</param>
    /// <returns>是否成功移除</returns>
    public static bool RemoveItem(this Backpack backpack, string itemId, int amount)
    {
        if (backpack == null) return false;
        
        // 查找对应ID的所有物品
        List<ItemBase> items = backpack.itemList.FindAll(i => i.id == itemId);
        
        if (items.Count == 0) return false;
        
        int remainingToRemove = amount;
        
        foreach (ItemBase item in items)
        {
            if (remainingToRemove <= 0) break;
            
            int removeFromThisStack = Mathf.Min(item.itemHeld, remainingToRemove);
            item.itemHeld -= removeFromThisStack;
            remainingToRemove -= removeFromThisStack;
            
            // 如果该堆栈数量变为0，从背包中移除
            if (item.itemHeld <= 0)
            {
                backpack.itemList.Remove(item);
            }
        }
        
        return remainingToRemove <= 0; // 如果成功移除了所需数量则返回true
    }

    /// <summary>
    /// 检查背包中是否有足够的物品
    /// </summary>
    /// <param name="backpack">背包实例</param>
    /// <param name="itemId">物品ID</param>
    /// <param name="amount">所需数量</param>
    /// <returns>是否有足够物品</returns>
    public static bool HasItem(this Backpack backpack, string itemId, int amount)
    {
        if (backpack == null) return false;
        
        int totalAmount = 0;
        foreach (ItemBase item in backpack.itemList)
        {
            if (item.id == itemId)
            {
                totalAmount += item.itemHeld;
            }
        }
        
        return totalAmount >= amount;
    }
    
    /// <summary>
    /// 根据ID查找物品定义
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <returns>物品定义</returns>
    private static ItemBase FindItemById(string id)
    {
        // 这里假设所有ItemBase资源都存储在Resources文件夹下
        // 实际项目中可能需要从数据库或其他地方加载
        ItemBase[] allItems = Resources.FindObjectsOfTypeAll<ItemBase>();
        foreach (ItemBase item in allItems)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 创建物品副本
    /// </summary>
    /// <param name="originalItem">原始物品</param>
    /// <returns>物品副本</returns>
    private static ItemBase CreateNewItemCopy(ItemBase originalItem)
    {
        ItemBase newItem = ScriptableObject.CreateInstance<ItemBase>();
        newItem.id = originalItem.id;
        newItem.name = originalItem.name;
        newItem.icon = originalItem.icon;
        newItem.description = originalItem.description;
        newItem.maxStack = originalItem.maxStack;
        newItem.itemType = originalItem.itemType;
        newItem.additionalData = originalItem.additionalData;
        newItem.functionScript = originalItem.functionScript;
        newItem.damage = originalItem.damage;
        newItem.health = originalItem.health;
        newItem.function = originalItem.function;
        newItem.Duration = originalItem.Duration;
        newItem.Speed = originalItem.Speed;
        newItem.Defense = originalItem.Defense;
        
        return newItem;
    }
}