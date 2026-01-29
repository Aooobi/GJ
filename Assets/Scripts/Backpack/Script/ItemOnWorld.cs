using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOnWorld : MonoBehaviour
{
    public ItemBase item;
    public Backpack playerBackPack;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            AddNewItem();
            Destroy(gameObject);
        }
    }

    public void AddNewItem() {
        if (!playerBackPack.itemList.Contains(item)) {
            playerBackPack.itemList.Add(item);
            UpdateAttribute();
            // BPManager.CreateNewItem(item);
        }
        else
        {
            if (item.itemHeld < item.maxStack) {
                item.itemHeld += 1;
            }
            else 
            {
                print("物品已满");
            }
        }
        
        BPManager.RefreshItem();
    }

    public void UpdateAttribute() {
        var attrObj = FindObjectOfType<CharacterStats>();
        if (attrObj != null && item != null) {
            // 根据物品类型和属性值来增加角色属性
            
            // 如果物品有生命值加成
            if (item.health != 0) {
                attrObj.WearAdd(AddType.MaxHealth, item.health, true);
            }
            
            // 如果物品有攻击力加成（轻攻击）
            if (item.damage != 0) {
                attrObj.WearAdd(AddType.LightAttack, item.damage, true);
            }
            
            // 如果物品有防御力加成
            if (item.Defense != 0) {
                attrObj.WearAdd(AddType.Defense, item.Defense, true);
            }
            
            // 如果物品有速度加成
            if (item.Speed != 0) {
                attrObj.WearAdd(AddType.MoveSpeed, item.Speed, true);
            }
            
            // // 如果物品有持续时间，则应用临时效果
            // if (item.Duration > 0) {
            //     // 例如，如果是临时效果，可以这样调用
            //     if (item.health != 0) {
            //         attrObj.WearAdd(AddType.CurrentHealth, item.health, false, item.Duration);
            //     }
            //     
            //     if (item.damage != 0) {
            //         attrObj.WearAdd(AddType.LightAttack, item.damage, false, item.Duration);
            //     }
            //     
            //     if (item.Defense != 0) {
            //         attrObj.WearAdd(AddType.Defense, item.Defense, false, item.Duration);
            //     }
            //     
            //     if (item.Speed != 0) {
            //         attrObj.WearAdd(AddType.MoveSpeed, item.Speed, false, item.Duration);
            //     }
            // }
        }
    }
}