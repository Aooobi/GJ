using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public ItemBase itemOnslot;
    public Image slotImage;
    public Text Number;

    public void ItemOnClicked()
    {
        BPManager.UpdateItemInfo(itemOnslot.description);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键按下直接return
            return;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 右键按下触发其他逻辑
            print("按下右键了");
            // RightClickLogic();
        }
    }

}
