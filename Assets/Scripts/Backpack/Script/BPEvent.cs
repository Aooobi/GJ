using UnityEngine;
using UnityEngine.Events;

// 事件中心（全局事件管理，可复用）
public class BPEvent : MonoBehaviour
{
    public static BPEvent Instance;

    // 定义背包状态变化事件（参数：是否打开）
    public UnityEvent<bool> OnInventoryStateChanged = new UnityEvent<bool>();

    void Awake()
    {
        // 防止重复创建
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}