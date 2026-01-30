using UnityEngine;

/// <summary>
/// 攻击脚本 最终定稿版
/// 功能：圆形范围检测/指定层攻击/轻重攻击区分/伤害触发
/// </summary>
public class Attack : MonoBehaviour
{
    [Header("攻击基础配置")]
    public float attackRange = 2f; // 扩大攻击范围，更容易触发
    public LayerMask targetLayer;  // 仅选择Player层
    public Transform attackPoint;  // 攻击检测点（怪物身前）

    private CharacterStats _selfStats;

    private void Awake()
    {
        _selfStats = GetComponent<CharacterStats>();
        // 默认攻击点为自身Transform，防止空值
        if (attackPoint == null) attackPoint = transform;
        // 空值保护
        if (_selfStats == null) Debug.LogError($"{gameObject.name} 缺少CharacterStats，无法攻击！");
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    /// <param name="isHeavy">是否为重攻击</param>
    public void isAttack(bool isHeavy = false)
    {
        // 核心组件缺失则直接返回
        if (_selfStats == null || attackPoint == null || targetLayer == 0) return;

        // 圆形范围检测目标层对象
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayer);
        if (hitTargets.Length == 0) return; // 无目标则返回

        // 计算伤害（轻/重攻击）
        float damage = isHeavy ? _selfStats.heavyAttack : _selfStats.lightAttack;
        if (damage <= 0) damage = 10f; // 保底伤害，防止伤害为0不生效

        // 对所有目标造成伤害
        foreach (var target in hitTargets)
        {
            CharacterStats targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null && !targetStats.isDead)
            {
                targetStats.TakeDamage(damage, gameObject);
                Debug.Log($"{gameObject.name} 对{target.name}造成{damage}点伤害！");
            }
        }
    }

  
}