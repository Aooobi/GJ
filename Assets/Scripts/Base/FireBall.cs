using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火球子弹脚本  衡玉
/// </summary>
public class FireBall : MonoBehaviour
{
    [Header("火球基础配置")]
    [SerializeField] public float shootspeed = 1f;//火球速度
    [SerializeField] public float maxDistance = 10f;//最大飞行距离
    [SerializeField] public float fireBallDamage = 20f; // 火球默认伤害

    [Header("火球大小")]
    //private Vector3 fireBallScale = new Vector3(10f, 10f, 1f); // 固定放大2倍（可直接改数值）
    //rivate float colliderRadius = 0.8f; // 固定碰撞体半径（和缩放匹配） 

    private Rigidbody2D rb;
    private Vector3 startPos;//发射起始位置
    private CharacterStats playerStats; //玩家属性脚本 取重攻击伤害
    private SpriteRenderer sr;
    private void Awake()
    {
        // 注释原有缩放/贴图逻辑，保留获取组件即可，贴图由RPG的CreateFireBall设置
        sr = GetComponent<SpriteRenderer>();
       
        // if (sr != null) { transform.localScale = fireBallScale; }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        }

       

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                fireBallDamage = playerStats.heavyAttack;
            }
        }
    }


    private void Start()
    {
        startPos = transform.position;

        //初始化 根据自身缩放x值 设置飞行方向
        InitMoveDirection();


    }

    private void FixedUpdate()
    {
        //检查自身飞行距离
        CheckFlyDistance();

    }

    //飞行方向
    private void InitMoveDirection()
    { 
        float moveDir = transform.localScale.x < 0 ? -1 : 1;
        rb.velocity = new Vector2(moveDir * shootspeed , rb.velocity.y);


    }



    //子弹飞行距离
    private void CheckFlyDistance()
    {
        float currentDistance = Vector2.Distance(startPos, transform.position);
        if(currentDistance >= maxDistance)
        {
            Destroy(gameObject);

        }

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // 核心修改：用Tag判断怪物（你场景中怪物的Tag是Monster，直接匹配）
        // 保留玩家检测（后续Boss火球可用），优先判断怪物
        if (other.CompareTag("Monster") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            CharacterStats targetStats = other.GetComponent<CharacterStats>();
            if (targetStats != null && !targetStats.isDead)
            {
                GameObject attacker = gameObject.name.Contains("Boss") ? transform.parent?.gameObject : GameObject.FindWithTag("Player");
                targetStats.TakeDamage(fireBallDamage, attacker);
                Debug.Log($"火球击中{other.name}，造成{fireBallDamage}点伤害");
            }
            Destroy(gameObject); // 击中立刻销毁，不会继续穿过
        }
        // 新增：碰到地面也销毁（可选，防止火球穿墙飞太远）
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

}
