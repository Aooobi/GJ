using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 第一个Boss脚本 - 火球技能（复用玩家火球逻辑）
/// 核心：自动朝向玩家 + 间隔发射火球 + 继承属性脚本 + 复用检测逻辑
/// 已移除Gizmos调试圈绘制
/// </summary>
public class Boss01_FireBall : MonoBehaviour
{
    [Header("Boss基础配置")]
    [SerializeField] private Sprite bossSprite; //Boss贴图
    [SerializeField] private float bossScale = 6f; //Boss缩放（比普通怪物大）
    [SerializeField] private LayerMask playerLayer; //玩家图层（用于检测）

    [Header("Boss火球技能配置（和玩家重攻击火球一致）")]
    [SerializeField] private Sprite fireBallSprite; //直接用玩家的火球贴图
    [SerializeField] private float fireBallSkillCD = 3f; //火球技能冷却（比玩家长，合理难度）
    [SerializeField] private float fireBallLaunchOffset = 1f; //火球发射偏移（避免贴脸生成）
    [SerializeField] private float fireRange = 8f; //火球发射距离（离玩家多远开始放）
    [SerializeField] private float fireBuffer = 1f; //发射缓冲（过近/过远都不发射）

    [Header("Boss移动&检测配置")]
    [SerializeField] private float detectRange = 10f; //检测玩家范围（比普通怪物大）
    [SerializeField] private float moveSpeed = 2.5f; //Boss移动速度
    [SerializeField] private float detectInterval = 0.2f; //检测间隔（优化性能）

    [Header("Boss属性关联")]
    [Tooltip("Boss火球伤害是否取自自身重攻击属性")]
    public bool useBossHeavyAttack = true;

    // 核心组件引用（全部复用现有脚本）
    private CharacterStats bossStats; //Boss自身属性（血量/防御/重攻击伤害）
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player; //玩家Transform
    private CharacterStats playerStats; //玩家属性（检测玩家是否阵亡）

    // 状态变量
    private float lastFireTime; //上次发射火球时间
    private float lastDetectTime; //上次检测玩家时间
    private Vector3 originalScale; //初始缩放（用于朝向翻转）
    private bool isPlayerInRange; //玩家是否在检测范围内

    private void Awake()
    {
        // 获取基础组件，和普通怪物逻辑一致
        bossStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // 初始化检测
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
        }
        else
        {
            Debug.LogError("场景中未找到Tag为Player的玩家对象！");
        }

        // 初始化缩放和贴图
        originalScale = new Vector3(bossScale, bossScale, 1f);
        transform.localScale = originalScale;
        if (bossSprite != null && sr != null)
        {
            sr.sprite = bossSprite;
        }

        // 忽略怪物层之间的碰撞（和普通怪物一致）
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Monster"), LayerMask.NameToLayer("Monster"), true);

        // 监听Boss死亡事件
        if (bossStats != null)
        {
            bossStats.OnDeath += OnBossDeath;
        }
        else
        {
            Debug.LogError("Boss未挂载CharacterStats属性脚本！");
        }

        // 初始化时间（让Boss开局就能放技能）
        lastFireTime = -fireBallSkillCD;
        lastDetectTime = Time.time;
    }

    private void Update()
    {
        // 死亡判定：Boss/玩家阵亡则停止所有逻辑
        if (bossStats.isDead || (playerStats != null && playerStats.isDead))
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 性能优化：间隔检测玩家，不每一帧检测
        if (Time.time - lastDetectTime >= detectInterval)
        {
            CheckPlayerInDetectRange();
            lastDetectTime = Time.time;
        }

        // 玩家在范围内才执行后续逻辑（移动+发射火球）
        if (isPlayerInRange && player != null)
        {
            // 自动朝向玩家
            FlipToPlayer();
            // 移动向玩家
            MoveToPlayer();
            // 检测距离并发射火球
            FireBallToPlayer();
        }
        else
        {
            // 玩家不在范围内，停止移动
            rb.velocity = Vector2.zero;
        }
    }

    #region 核心逻辑1：检测玩家是否在范围内
    private void CheckPlayerInDetectRange()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        // 检测范围：包含缓冲，避免频繁切换状态
        isPlayerInRange = distanceToPlayer <= detectRange;
    }
    #endregion

    #region 核心逻辑2：自动朝向玩家（复用怪物翻转逻辑）
    private void FlipToPlayer()
    {
        if (player == null) return;

        // 计算朝向玩家的X方向
        float dirX = player.position.x - transform.position.x;
        if (dirX == 0) return;

        // 翻转Boss：根据方向设置缩放X轴
        float newScaleX = Mathf.Sign(dirX) * originalScale.x;
        transform.localScale = new Vector3(newScaleX, originalScale.y, originalScale.z);
    }
    #endregion

    #region 核心逻辑3：向玩家移动（匀速移动，无加速）
    private void MoveToPlayer()
    {
        if (player == null) return;

        // 计算朝向玩家的归一化方向（只走X轴，2D横向）
        Vector2 moveDir = new Vector2(player.position.x - transform.position.x, 0).normalized;
        // 移动：只改X轴速度，Y轴保持0（Boss不跳跃）
        rb.velocity = new Vector2(moveDir.x * moveSpeed, 0);
    }
    #endregion

    #region 核心逻辑4：发射火球（完全复用玩家CreateFireBall方法）
    private void FireBallToPlayer()
    {
        if (player == null || fireBallSprite == null) return;

        // 1. 技能冷却判定
        if (Time.time - lastFireTime < fireBallSkillCD) return;

        // 2. 发射距离判定：在[fireRange-fireBuffer, fireRange+fireBuffer]之间才发射
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < fireRange - fireBuffer || distanceToPlayer > fireRange + fireBuffer)
        {
            return;
        }

        // 3. 复刻玩家CreateFireBall方法，生成火球
        CreateBossFireBall();

        // 4. 记录发射时间，触发冷却
        lastFireTime = Time.time;
        Debug.Log($"Boss发射火球！冷却{fireBallSkillCD}秒，下次发射时间：{lastFireTime + fireBallSkillCD}");

        // 发射时停步（增加打击感）
        rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// Boss创建火球：完全复刻玩家RPG脚本的CreateFireBall方法
    /// 仅修改：发射偏移、火球伤害取自Boss自身重攻击
    /// </summary>
    private void CreateBossFireBall()
    {
        // 1. 创建火球游戏对象（和玩家一致）
        GameObject fireBallObj = new GameObject("Boss_FireBall");
        // 2. 设置火球生成位置：Boss位置 + 朝向偏移（避免贴脸生成）
        float offsetX = transform.localScale.x > 0 ? fireBallLaunchOffset : -fireBallLaunchOffset;
        fireBallObj.transform.position = transform.position + new Vector3(offsetX, 0.5f, 0);
        // 3. 设置火球缩放（和玩家一致，保证朝向正确）
        fireBallObj.transform.localScale = transform.localScale;

        // 4. 添加火球精灵组件（用和玩家一样的贴图）
        SpriteRenderer sr = fireBallObj.AddComponent<SpriteRenderer>();
        sr.sprite = fireBallSprite;
        sr.sortingOrder = 10; //和玩家火球同一层级，避免遮挡

        // 5. 添加火球核心脚本（复用现有FireBall脚本，关键！）
        FireBall fireBallScript = fireBallObj.AddComponent<FireBall>();

        // 6. 关键：如果开启Boss重攻击伤害，手动给火球赋值Boss的重攻击伤害
        if (useBossHeavyAttack && bossStats != null)
        {
            fireBallScript.fireBallDamage = bossStats.heavyAttack;
        }
    }
    #endregion

    #region Boss死亡逻辑
    private void OnBossDeath()
    {
        // 停止所有移动
        rb.velocity = Vector2.zero;
        // 禁用脚本（防止后续逻辑执行）
        this.enabled = false;
        // 隐藏Sprite（可后续加死亡动画）
        if (sr != null) sr.enabled = false;

        Debug.Log($"【Boss01】已阵亡！");
        // 后续可加：掉落稀有物品、解锁新区域、播放死亡音效等逻辑
    }
    #endregion
}