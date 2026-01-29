using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDisapear : MonoBehaviour
{
    [Header("隐藏前等待时间(秒)")]
    public float delayTime = 10f;

    void Start()
    {
        StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        gameObject.SetActive(false);
    }
}
