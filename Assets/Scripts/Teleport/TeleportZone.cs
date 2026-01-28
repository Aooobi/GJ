using System;
using System.Collections;
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [Header("传送设置")]
    [Tooltip("传送目标位置")]
    public Transform targetPoint;

    [Header("玩家设置")]
    [Tooltip("玩家标签")]
    public string playerTag = "Player";

    [Header("传送设置")]
    [Tooltip("传送冷却时间（秒）")]
    public float teleportCooldown = 0.5f;

    [Tooltip("是否需要淡入淡出效果")]
    public bool useFadeEffect = true;

    // 防止连续触发
    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家且不在传送冷却中
        if (other.CompareTag(playerTag) && !isTeleporting && targetPoint != null)
        {
            // 开始传送
            StartCoroutine(TeleportPlayer(other.gameObject));
        }
    }

    /// <summary>
    /// 传送玩家的协程
    /// </summary>
    private IEnumerator TeleportPlayer(GameObject player)
    {
        isTeleporting = true;

        if (useFadeEffect && UIFadeEffect.Instance != null)
        {
            // 使用淡入淡出效果进行传送
            UIFadeEffect.Instance.FadeOutAndFadeIn(
                on_black: () =>
                {
                    // 黑屏时执行传送
                    TeleportPlayerToTarget(player);
                },
                on_complete: null,
                active_panel_on_end: true
            );
        }
        else
        {
            // 直接传送（无效果）
            TeleportPlayerToTarget(player);
        }

        // 等待传送冷却时间
        yield return new WaitForSeconds(teleportCooldown);
        isTeleporting = false;
    }

    /// <summary>
    /// 将玩家传送到目标位置
    /// </summary>
    private void TeleportPlayerToTarget(GameObject player)
    {
        // 传送玩家到目标位置
        player.transform.position = targetPoint.position;

        // 如果需要，可以在这里添加其他传送后的逻辑
        // 例如：重置玩家速度、播放音效等
    }

#if UNITY_EDITOR
    // 在编辑器中绘制调试信息
    private void OnDrawGizmosSelected()
    {
        if (targetPoint != null)
        {
            // 绘制目标点
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPoint.position, 0.5f);

            // 绘制从触发器到目标点的连接线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint.position);

            // 绘制触发器范围
            if (GetComponent<Collider2D>() is BoxCollider2D boxCollider)
            {
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
            else if (GetComponent<Collider2D>() is CircleCollider2D circleCollider)
            {
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
        }
    }
#endif
}