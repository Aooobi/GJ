using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物最终定稿 - 解决体型过大+不攻击 衡玉
/// </summary>
public class Monsters : MonoBehaviour
{
    [Header("巡逻配置")]
    [SerializeField] public Transform place1;
    [SerializeField] public Transform place2;
    [SerializeField] private float moveSpeed = 2f;
    private Vector2 targetPoint;

    [Header("追逐配置")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float chaseSpeed = 3.2f;
    [SerializeField] private float chaseBuffer = 1.2f;
    private bool isChasing = false;

    [Header("攻击配置")]
    [SerializeField] private float attackCD = 1.2f;
    [SerializeField] private bool useHeavyAttack = false;
    private float lastAttackTime;

    [Header("地面检测")]
    [SerializeField] private string groundTag = "Ground";

    [Header("掉落")]
    [SerializeField] private GameObject itemOnWorldPrefabs;
    [SerializeField] private ItemBase dropItem;
    [SerializeField, Range(0, 100)] private float dropRate = 50f;
    [SerializeField] private Vector2 dropOffset = new Vector2(0.5f, 0);

    private CharacterStats monstersStats;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Attack monsterAttack;
    private float detectInterval = 0.2f;
    private float lastDetectTime;
    private bool isOnGround = false;

    // 彻底修复体型：直接记录预制体本身的Scale，脚本不再修改原始大小
    private Vector3 originalScale;

    private void Awake()
    {
        monstersStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        monsterAttack = GetComponent<Attack>();

        // 【核心修复体型】直接取你预制体自己的Scale，脚本不再额外放大！
        originalScale = transform.localScale;

        if (rb != null)
        {
            rb.gravityScale = 2.5f;
            rb.drag = 0;
            rb.angularDrag = 0.05f;
            rb.freezeRotation = true;
        }

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Monster"), LayerMask.NameToLayer("Monster"), true);

        if (monstersStats == null)
        {
            Debug.LogError("缺少CharacterStats");
            enabled = false;
            return;
        }
        if (monsterAttack == null)
        {
            Debug.LogError("缺少Attack");
            enabled = false;
            return;
        }

        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("找不到Player");
            enabled = false;
            return;
        }

        monstersStats.OnDeath += OnMonsterDeath;


    }

    private void Start()
    {
        RandomPoint();
        lastDetectTime = Time.time;
        lastAttackTime = -attackCD;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    private void Update()
    {
        if (monstersStats.isDead || player == null)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (Time.time - lastDetectTime >= detectInterval)
        {
            CheckPlayerInRange();
            lastDetectTime = Time.time;
        }

        if (isOnGround)
        {
            if (isChasing)
            {
                ChasePlayer();
                AttackPlayer();
            }
            else
            {
                Patrol();
            }
        }
        else
        {
            if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    #region 巡逻
    private void Patrol()
    {
        if (place1 == null || place2 == null)
        {
            if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        float dirX = Mathf.Sign(targetPoint.x - transform.position.x);
        FlipMonster(dirX);

        if (rb != null)
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.4f)
        {
            RandomPoint();
        }
    }

    private void RandomPoint()
    {
        float minX = Mathf.Min(place1.position.x, place2.position.x);
        float maxX = Mathf.Max(place1.position.x, place2.position.x);
        float rx = Random.Range(minX, maxX);
        targetPoint = new Vector2(rx, 0);
    }
    #endregion

    #region 玩家检测
    private void CheckPlayerInRange()
    {
        float dis = Vector2.Distance(transform.position, player.position);
        if (dis <= chaseRange)
            isChasing = true;
        else if (dis > chaseRange + chaseBuffer)
            isChasing = false;
    }
    #endregion

    #region 追逐
    private void ChasePlayer()
    {
        if (player == null || rb == null) return;
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        FlipMonster(dirX);
        rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
    }
    #endregion

    #region 攻击 - 强制触发修复
    private void AttackPlayer()
    {
        if (player == null || monsterAttack == null) return;

        // 【强制修复】直接用距离判定，不依赖Attack的范围，保证一定进攻击逻辑
        float realDis = Vector2.Distance(transform.position, player.position);
        if (realDis > 2.2f) return;

        if (Time.time - lastAttackTime < attackCD) return;

        // 强制调用攻击
        monsterAttack.isAttack(useHeavyAttack);
        Debug.Log("【怪物】正在执行攻击！");
        lastAttackTime = Time.time;
        if (rb != null) rb.velocity = Vector2.zero;
    }
    #endregion

    #region 翻转 - 只镜像，不改大小
    private void FlipMonster(float dirX)
    {
        if (dirX == 0) return;
        // 只翻转X符号，完全保留你预制体的原始大小，绝不放大
        transform.localScale = new Vector3(
            Mathf.Sign(dirX) * Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );
    }
    #endregion

    #region 地面碰撞
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

    #region 死亡掉落
    private void OnMonsterDeath()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        SpawnDrop();
        monstersStats.OnDeath -= OnMonsterDeath;
        Destroy(gameObject, 0.5f);
    }

    private void SpawnDrop()
    {
        if (itemOnWorldPrefabs == null || dropItem == null) return;
        if (Random.Range(0, 100) <= dropRate)
        {
            Vector3 pos = transform.position + new Vector3(dropOffset.x, dropOffset.y, 0);
            GameObject obj = Instantiate(itemOnWorldPrefabs, pos, Quaternion.identity);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null && dropItem.icon != null)
            {
                sr.sprite = dropItem.icon; // 直接赋值！
            }
            var item = obj.GetComponent<ItemOnWorld>();
            if (item != null) item.item = dropItem;
        }
    }
    #endregion
}