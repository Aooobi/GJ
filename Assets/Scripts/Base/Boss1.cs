using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BOSS怪物脚本 - 整合巡逻/追逐/近战攻击+定时远程火球攻击
/// 预制体要求：挂载CharacterStats、Attack、Rigidbody2D、SpriteRenderer组件
/// </summary>
public class BossMonster : MonoBehaviour
{
    [Header("=== 基础移动配置 ===")]
    [SerializeField] public Transform place1; // 巡逻起点
    [SerializeField] public Transform place2; // 巡逻终点
    [SerializeField] private float moveSpeed = 1.5f; // 巡逻速度（BOSS建议比普通怪慢）
    [SerializeField] private float chaseRange = 8f; // 追逐玩家范围（BOSS建议更大）
    [SerializeField] private float chaseSpeed = 2.8f; // 追逐速度
    [SerializeField] private float chaseBuffer = 1.5f; // 脱离追逐缓冲距离
    private Vector2 targetPoint;
    private bool isChasing = false;

    [Header("=== 近战攻击配置 ===")]
    [SerializeField] private float attackCD = 1.5f; // 近战攻击冷却（BOSS建议稍长）
    [SerializeField] private bool useHeavyAttack = false; // 是否使用重击
    private float lastMeleeAttackTime; // 最后一次近战攻击时间

    [Header("=== 远程火球攻击配置【核心新增】===")]
    [SerializeField] private float fireBallCD = 3f; // 火球发射间隔（固定时间，可调整）
    [SerializeField] private int fireBallDamage = 5; // 火球基础伤害
    [SerializeField] private Vector2 fireBallOffset = new Vector2(1f, 0.8f); // 火球发射偏移（适配BOSS体型）
    [SerializeField] private float fireBallScale = 6f; // 火球大小
    [SerializeField] private Sprite fireBallSprite; // 火球贴图（拖入和玩家火球一样的贴图即可）
    private float lastFireBallTime; // 最后一次发射火球时间

    [Header("=== 基础配置 ===")]
    [SerializeField] private string groundTag = "Ground";
    private CharacterStats bossStats;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Attack bossAttack;
    private float detectInterval = 0.2f;
    private float lastDetectTime;
    private bool isOnGround = false;
    private Vector3 originalScale; // 保留BOSS原始体型，不自动缩放

    [Header("=== 死亡掉落配置 ===")]
    [SerializeField] private GameObject itemOnWorldPrefabs;
    [SerializeField] private ItemBase dropItem;
    [SerializeField, Range(0, 100)] private float dropRate = 80f; // BOSS掉落率建议提高
    [SerializeField] private Vector2 dropOffset = new Vector2(1f, 1f);


    [Header("计时器技能")]
    [SerializeField] private float interval = 1f; // 触发间隔（秒）
    public Action OnTimerTriggered; // 定时触发的事件

    private float timer;
    private void Awake()
    {
        // 获取核心组件
        bossStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<Attack>();

        // 保留BOSS原始体型，不修改
        originalScale = transform.localScale;

        // 刚体配置（适配BOSS重量）
        if (rb != null)
        {
            rb.gravityScale = 2.5f;
            rb.drag = 0;
            rb.angularDrag = 0.05f;
            rb.freezeRotation = true;
        }

        // 怪物之间忽略碰撞
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Monster"), LayerMask.NameToLayer("Monster"), true);

        // 组件校验（缺少则禁用脚本，避免报错）
        if (bossStats == null) { Debug.LogError("BOSS缺少CharacterStats组件！", this); enabled = false; return; }
        if (bossAttack == null) { Debug.LogError("BOSS缺少Attack组件！", this); enabled = false; return; }
        if (fireBallSprite == null) { Debug.LogError("请为BOSS分配火球贴图！", this); enabled = false; return; }

        // 查找玩家
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) { Debug.LogError("场景中未找到Player标签的对象！", this); enabled = false; return; }

        // 绑定死亡事件
        bossStats.OnDeath += OnBossDeath;


        OnTimerTriggered += ()=>{
            int v = UnityEngine.Random.Range(1, 5);
            switch (v)
            {
                case 1:
                    Debug.Log("BOSS计时器触发事件：咆哮！");
                    GetComponent<Animator>().SetTrigger("attc");
                    break;
                case 2:
                    Debug.Log("BOSS计时器触发事件：振奋！");
                    GetComponent<Animator>().SetTrigger("attb");
                    break;
                case 3:
                    Debug.Log("BOSS计时器触发事件：狂怒！");
                    GetComponent<Animator>().SetTrigger("attl");
                    break;
                case 4:
                    Debug.Log("");
                    GetComponent<Animator>().SetTrigger("attr");
                    break;
                default:break;
            }

        };
    }

    private void Start()
    {
        // 初始化巡逻点、冷却时间
        RandomPatrolPoint();
        lastDetectTime = Time.time;
        lastMeleeAttackTime = -attackCD;
        lastFireBallTime = -fireBallCD; // 初始无冷却，可立即发射火球
        if (rb != null) rb.velocity = Vector2.zero;

        timer = interval; // 第一次立即触发
    }

    private void Update()
    {
        // 死亡/玩家为空时，停止所有行为
        if (bossStats.isDead || player == null)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        // 定时检测玩家（避免每帧检测，优化性能）
        if (Time.time - lastDetectTime >= detectInterval)
        {
            CheckPlayerInRange();
            lastDetectTime = Time.time;
        }

        // 地面检测：只有在地面才执行移动/攻击
        if (isOnGround)
        {
            if (isChasing)
            {
                ChasePlayer(); // 追逐玩家
                MeleeAttackPlayer(); // 近战攻击玩家
            }
            else
            {
                Patrol(); // 巡逻
            }
            FireBallAttack(); // 定时发射火球（无论巡逻/追逐，都自动发射）
        }
        else
        {
            if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
        }

        //尽快i事情

        // 使用 Time.deltaTime 累加时间
        timer += Time.deltaTime;

        // 检查是否达到触发间隔
        if (timer >= interval)
        {
            // 触发事件
            OnTimerTriggered?.Invoke();

            // 重置计时器（保持余量，避免误差累积）
            timer = 0f;
        }
    }

    #region 基础移动：巡逻
    private void Patrol()
    {
        if (place1 == null || place2 == null) return;

        // 朝向巡逻目标点
        float dirX = Mathf.Sign(targetPoint.x - transform.position.x);
        FlipBoss(dirX);

        // 巡逻移动
        if (rb != null) rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        // 到达巡逻点，随机切换下一个点
        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.5f)
        {
            RandomPatrolPoint();
        }
    }

    // 随机生成巡逻点（在place1和place2之间）
    private void RandomPatrolPoint()
    {
        float minX = Mathf.Min(place1.position.x, place2.position.x);
        float maxX = Mathf.Max(place1.position.x, place2.position.x);
        float randomX = UnityEngine.Random.Range(minX, maxX);
        targetPoint = new Vector2(randomX, transform.position.y);
    }
    #endregion

    #region 玩家检测：追逐/脱离追逐
    private void CheckPlayerInRange()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        // 进入追逐范围，开始追逐
        if (distance <= chaseRange) isChasing = true;
        // 超出追逐范围+缓冲距离，脱离追逐
        else if (distance > chaseRange + chaseBuffer) isChasing = false;
    }
    #endregion

    #region 玩家追逐
    private void ChasePlayer()
    {
        if (player == null || rb == null) return;

        // 朝向玩家
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        FlipBoss(dirX);

        // 追逐移动
        rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
    }
    #endregion

    #region 近战攻击：靠近玩家触发
    private void MeleeAttackPlayer()
    {
        if (player == null || bossAttack == null) return;

        // 近战攻击距离判定（BOSS体型大，距离稍远）
        float attackDistance = 2.5f;
        if (Vector2.Distance(transform.position, player.position) > attackDistance) return;

        // 攻击冷却判定
        if (Time.time - lastMeleeAttackTime < attackCD) return;

        // 执行近战攻击
        bossAttack.isAttack(useHeavyAttack);
        Debug.Log($"【BOSS近战攻击】触发攻击，伤害：{fireBallDamage}");
        lastMeleeAttackTime = Time.time;
        if (rb != null) rb.velocity = Vector2.zero; // 攻击时停住
    }
    #endregion

    #region 远程攻击：定时发射火球【核心新增】   
    private void FireBallAttack()
    {
        // 火球冷却判定
        if (Time.time - lastFireBallTime < fireBallCD) return;

        // 生成火球（朝BOSS当前朝向）
        CreateFireBall();
        Debug.Log($"【BOSS火球攻击】发射火球，伤害：{fireBallDamage}");
        lastFireBallTime = Time.time;
    }

    private void CreateFireBall()
    {
        // 1. 创建火球游戏对象
        GameObject fireBallObj = new GameObject("BossFireBall");
        // 火球发射位置：BOSS位置 + 偏移量（适配朝向）
        Vector3 spawnPos = transform.position + new Vector3(
            fireBallOffset.x * Mathf.Sign(transform.localScale.x),
            fireBallOffset.y,
            transform.position.z
        );
        fireBallObj.transform.position = spawnPos;
        // 火球大小
        fireBallObj.transform.localScale = new Vector3(fireBallScale, fireBallScale, 1);

        // 2. 添加火球核心组件
        SpriteRenderer sr = fireBallObj.AddComponent<SpriteRenderer>();
        sr.sprite = fireBallSprite;
        sr.sortingOrder = 10; // 层级高于BOSS，避免被遮挡

        CircleCollider2D circleCol = fireBallObj.AddComponent<CircleCollider2D>();
        circleCol.isTrigger = true; // 触发器，检测碰撞
        circleCol.radius = 1f; // 碰撞范围

        Rigidbody2D fireRb = fireBallObj.AddComponent<Rigidbody2D>();
        fireRb.gravityScale = 0; // 火球无重力，直线飞行
        fireRb.velocity = new Vector2(8f * Mathf.Sign(transform.localScale.x), 0); // 火球飞行速度（朝BOSS朝向）

        // 3. 添加火球伤害脚本（复用原FireBall脚本，需保证场景中有该脚本）
        FireBall fireBallScript = fireBallObj.AddComponent<FireBall>();
        fireBallScript.fireBallDamage = fireBallDamage; // 赋值火球伤害

        // 4. 火球自动销毁（避免内存泄漏，5秒后销毁）
        Destroy(fireBallObj, 5f);
    }
    #endregion

    #region 辅助方法：BOSS翻转（保留原始体型）
    private void FlipBoss(float dirX)
    {
        if (dirX == 0) return;
        // 只翻转X轴，保留BOSS原始大小和Y/Z轴
        transform.localScale = new Vector3(
            Mathf.Sign(dirX) * Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );
    }
    #endregion

    #region 地面检测：只有在地面才执行行为
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(groundTag))
        {
            isOnGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(groundTag))
        {
            isOnGround = false;
        }
    }
    #endregion

    #region 死亡处理：掉落物品+销毁BOSS
    private void OnBossDeath()
    {
        // 死亡时停止所有移动
        if (rb != null) rb.velocity = Vector2.zero;
        // 生成掉落物品
        SpawnDrop();
        // 解绑死亡事件，避免重复触发
        bossStats.OnDeath -= OnBossDeath;
        // 延迟销毁BOSS（留动画时间，可调整）
        Destroy(gameObject, 1f);
        Debug.Log($"【BOSS死亡】已被击败，触发掉落！");
    }

    // 掉落物品逻辑
    private void SpawnDrop()
    {
        if (itemOnWorldPrefabs == null || dropItem == null) return;
        // 按掉落率生成物品
        if (UnityEngine.Random.Range(0, 100) <= dropRate)
        {
            Vector3 dropPos = transform.position + new Vector3(dropOffset.x, dropOffset.y, 0);
            GameObject dropObj = Instantiate(itemOnWorldPrefabs, dropPos, Quaternion.identity);
            ItemOnWorld itemScript = dropObj.GetComponent<ItemOnWorld>();
            if (itemScript != null) itemScript.item = dropItem;
        }
    }
    #endregion
}