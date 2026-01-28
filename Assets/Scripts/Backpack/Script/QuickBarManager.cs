using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickBarManager : MonoBehaviour
{
    static QuickBarManager instance;

    public Backpack Bag;
    public QuickBar QuickBar;
    public QuickBar_Slot[] Slots;
    public static int SlotNum = 3;
    
    void Awake() {
        if (instance != null) {
            Destroy(this);
        }
        instance = this;
    }
    
    private void OnEnable() {
        RefreshImage();
    }

    public static void AddItemToQuickBar(ItemBase item) {
        if (instance.QuickBar.itemList.Count < SlotNum) {
            if (!instance.QuickBar.itemList.Contains(item))
            instance.QuickBar.itemList.Add(item);
            else {
                print("物品已存在"); //写成通知提醒
            }
        }
        else {
            print("快捷栏已满"); //写成通知提醒
        }
        
        RefreshImage();
    }

    public static void RemoveItemFromQuickBar(ItemBase item) {
        if (instance.QuickBar.itemList.Contains(item))
            instance.QuickBar.itemList.Remove(item);
        else {
            print("物品不存在");
        }
        RefreshImage();
    }

    public static void RefreshImage() {
        for (int i = 0; i < instance.QuickBar.itemList.Count; i++) {
            instance.Slots[i].slotImage.sprite = instance.QuickBar.itemList[i].icon;
            instance.Slots[i].itemOnslot = instance.QuickBar.itemList[i];
        }
    }
}
