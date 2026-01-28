using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础攻击脚本   衡玉 
/// </summary>
public class Attack : MonoBehaviour
{
    [Header("攻击配置")]
    public float attackRange = 1.5f; //攻击范围
    public LayerMask targetLayer; //目标图层
    public Transform attackPoint; //攻击检测点

    //直接调用属性
    private CharacterStats attackStats;

    private void Awake()
    {
        attackStats = GetComponent<CharacterStats>();
        if(attackPoint == null)
        {
            attackPoint = transform;
        }

    }

    public void isAttack(bool isHeavy = false)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position,attackRange,targetLayer);
        float damage = isHeavy? attackStats.heavyAttack : attackStats.lightAttack;
        //遍历扣血
        foreach(var hit in hits)
        {
            CharacterStats targetStats = hit.GetComponent<CharacterStats>();
            if(targetStats != null)
            {
                if(!targetStats.isDead)
                {
                    targetStats.TakeDamage(damage,gameObject);//调用属性的扣血
                }

            }
        }


    }


}
