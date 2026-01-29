using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickBarManager : MonoBehaviour
{
    static QuickBarManager instance;

    public Backpack Bag;
    public QuickBar QuickBar;
    public QuickBar_Slot[] Slots;
    public Sprite DefaultIcon;
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
            if (!instance.QuickBar.itemList.Contains(item)) {
                instance.QuickBar.itemList.Add(item);
                instance.Bag.itemList.Remove(item);
                BPManager.RefreshItem();
            }
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
        if (instance.QuickBar.itemList.Contains(item)) {
            instance.QuickBar.itemList.Remove(item);
            instance.Bag.itemList.Add(item);
            BPManager.RefreshItem();
        }
        else {
            print("物品不存在");
        }
        RefreshImage();
    }

    public static void DropItem(ItemBase item) {
        if (instance.QuickBar.itemList.Contains(item)) {
            instance.QuickBar.itemList.Remove(item);
        }
        else {
            print("物品不存在");
        }
        RefreshImage();
    }

    public static void RefreshImage() {
        for (int i = 0; i < SlotNum; i++) {
            instance.Slots[i].slotImage.sprite = instance.DefaultIcon;
            instance.Slots[i].Number.text = "00";
        }
        for (int i = 0; i < instance.QuickBar.itemList.Count; i++) {
            instance.Slots[i].slotImage.sprite = instance.QuickBar.itemList[i].icon;
            instance.Slots[i].itemOnslot = instance.QuickBar.itemList[i];
            instance.Slots[i].Number.text = instance.QuickBar.itemList[i].itemHeld.ToString();
        }
    }

    public static void UseItem(int i) {
        var attrObj = FindObjectOfType<CharacterStats>();
        var item = instance.QuickBar.itemList[i];
        if (instance.QuickBar.itemList[i].itemHeld == 1) {
            DropItem(item);
        }
        else {
            instance.QuickBar.itemList[i].itemHeld--;
        }
        if (attrObj != null && item != null) {
            // 根据物品类型和属性值来增加角色属性
            //
            // // 如果物品有生命值加成
            // if (item.health != 0) {
            //     attrObj.WearAdd(AddType.MaxHealth, item.health, true);
            // }
            //
            // // 如果物品有攻击力加成（轻攻击）
            // if (item.damage != 0) {
            //     attrObj.WearAdd(AddType.LightAttack, item.damage, true);
            // }
            //
            // // 如果物品有防御力加成
            // if (item.Defense != 0) {
            //     attrObj.WearAdd(AddType.Defense, item.Defense, true);
            // }
            //
            // // 如果物品有速度加成
            // if (item.Speed != 0) {
            //     attrObj.WearAdd(AddType.MoveSpeed, item.Speed, true);
            // }
            
            // 如果物品有持续时间，则应用临时效果
            if (item.Duration > 0) {
                if (item.health != 0) {
                    attrObj.WearAdd(AddType.CurrentHealth, item.health, false, item.Duration);
                }
                
                if (item.damage != 0) {
                    attrObj.WearAdd(AddType.LightAttack, item.damage, false, item.Duration);
                }
                
                if (item.Defense != 0) {
                    attrObj.WearAdd(AddType.Defense, item.Defense, false, item.Duration);
                }
                
                if (item.Speed != 0) {
                    attrObj.WearAdd(AddType.MoveSpeed, item.Speed, false, item.Duration);
                }
            }
        }
        
        RefreshImage();
    }
}
