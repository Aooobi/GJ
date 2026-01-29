using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火球子弹脚本  衡玉
/// </summary>
public class FireBall : MonoBehaviour
{
    [Header("火球基础配置")]
    [SerializeField] public float shootspeed = 8f;//火球速度
    [SerializeField] public float maxDistance = 10f;//最大飞行距离
    [SerializeField] private float fireBallDamage = 20f; //火球默认伤害

    [Header("火球大小")]
    private Vector3 fireBallScale = new Vector3(10f, 10f, 1f); // 固定放大2倍（可直接改数值）
    private float colliderRadius = 0.8f; // 固定碰撞体半径（和缩放匹配） 

    private Rigidbody2D rb;
    private Vector3 startPos;//发射起始位置
    private CharacterStats playerStats; //玩家属性脚本 取重攻击伤害
    private SpriteRenderer sr; 
    private void Awake()
    {
        // 新增：获取/添加精灵组件，设置火球缩放（核心放大逻辑）
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            transform.localScale = fireBallScale; //直接设置火球缩放，可视化调整
        }
        rb = GetComponent<Rigidbody2D>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; //防止穿模
        }

        Collider2D col = GetComponent<Collider2D>();
        if(col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.isTrigger = true;
            circleCol.radius = colliderRadius; //设置半径 

        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
            if(playerStats != null)
            {
                fireBallDamage = playerStats.heavyAttack; //火球伤害取自玩家重攻击伤
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
        if(other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            //获取怪物脚本
            CharacterStats targetStats = other.GetComponent<CharacterStats>();
            if(targetStats != null)
            {
                if(!targetStats.isDead)
                {
                    targetStats.TakeDamage(fireBallDamage , GameObject.FindWithTag("Player"));
                    Debug.Log($"火球击中{other.name}，造成{fireBallDamage}点伤害，剩余血量：{targetStats.currentHealth}");

                }

            }
            Debug.Log("火球击中");
            Destroy(gameObject);

        }



        

    }

}
