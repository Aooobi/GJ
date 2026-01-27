using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BPManager : MonoBehaviour
{
    static BPManager instance;
    
    public Backpack Bag; //需要背包
    public GameObject slotGrid;  //需要预设置好的网格
    public Slot gridPrefab; //格子预设体
    public Text itemInfo; //物品信息
    void Awake() {
        if (instance != null) {
            Destroy(this);
        }
        instance = this;
    }

    public static void CreateNewItem(ItemBase item)
    {
        Slot newItem = Instantiate(instance.gridPrefab, instance.slotGrid.transform.position, Quaternion.identity);
        newItem.gameObject.transform.SetParent(instance.slotGrid.transform);
        //重置本地变换，让布局组控制器来决定其位置和大小
        newItem.gameObject.transform.localPosition = Vector3.zero;
        newItem.gameObject.transform.localRotation = Quaternion.identity;
        newItem.gameObject.transform.localScale = Vector3.one;
        
        newItem.itemOnslot = item;
        newItem.slotImage.sprite = item.icon;
        newItem.Number.text = item.itemHeld.ToString();
    }
}
