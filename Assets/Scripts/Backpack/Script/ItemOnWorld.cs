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
            BPManager.CreateNewItem(item);
        }
        else
        {
            item.itemHeld += 1;
        }
    }
}
