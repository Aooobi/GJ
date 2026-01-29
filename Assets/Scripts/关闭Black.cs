using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 关闭Black : MonoBehaviour
{
    public GameObject black;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            black.SetActive(false);
        }
    }
}
