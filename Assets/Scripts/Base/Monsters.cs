using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// 怪物脚本  巡逻  掉落   衡玉
/// </summary>

public class Monsters : MonoBehaviour
{
    [Header("怪物巡逻逻辑")]
    [SerializeField] private Transform place1;//起点
    [SerializeField] private Transform place2;//终点
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float monsterScale = 5f;//新增怪物基础缩放
    private Vector2 targetPoint;

    [Header("怪物追踪逻辑")]
    [SerializeField] private float chaseRange = 5f;//追踪范围
    [SerializeField] private float chaseSpeed = 3f;//追踪速度
    [SerializeField] private float chaseBuffer = 1f;//脱战缓冲区间
    private bool isChasing = false;
    private float detectInterval = 0.2f;//检测间隔时间 避免每一帧检测 优化性能
    private float lastDetectTime; //上次检测时间

    [Header("怪物攻击逻辑(复用攻击脚本)")]
    //[SerializeField] private float attackRange = 1.5f;//攻击范围
    //[SerializeField] private float attackCD = 1f;
    //[SerializeField] private bool useStatsAttack = true;
    //[SerializeField] private float fixedAttackDamage = 10f;
    [SerializeField] private float attackCD = 1f; //攻击冷却
    [SerializeField] private bool useHeavyAttack = false; //是否使用重攻击


    [Header("怪物贴图")]
    [SerializeField] private Sprite monsterSprite;

    [Header("怪物掉落配置")]
    [SerializeField] private GameObject itemOnWorldPrefabs;//预设体
    [SerializeField] private ItemBase dropItem; //掉落物品
    [SerializeField, Range(0f, 100f)] private float dropRate = 50f; //掉落概率 
    [SerializeField] private Vector2 dropOffset = new Vector2(0.5f, 0.5f); //掉落位置






    private CharacterStats monstersStats;
    private CharacterStats playerStats;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Attack monsterAttack;//引用通用Attack脚本

    private float lastAttackTime;//上次攻击时间

    private Vector3 originalScale;//记录初始缩放 用于朝向反向


    private void Awake()
    {
        monstersStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Monster"), LayerMask.NameToLayer("Monster"), true);
        monsterAttack = GetComponent<Attack>();

        //SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (monstersStats == null)
        {
            Debug.Log("怪物未挂载属性脚本");
            return;
        }

        if (monsterSprite != null)
        {
            sr.sprite = monsterSprite;

        }


        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.Log("角色未改变tag");
            return;
        }

        playerStats = player.GetComponent<CharacterStats>();
        if (playerStats == null)
        {
            Debug.Log("玩家未挂载属性脚本");
            return;
        }

        //监听
        monstersStats.OnDeath += OnMonsterDeath;


    }

    private void Start()
    {
        RandomPoint();

        //初始化检测时间
        lastDetectTime = Time.time;

        //初始化朝向
        //transform.localScale = new Vector3(monsterScale, monsterScale, 1);

        //初始化攻击时间
        lastAttackTime = -attackCD;

    }


    private void Update()
    {
        if (monstersStats.isDead || (playerStats != null && playerStats.isDead))
        {
            return;
        }


        //性能优化 每隔0.2秒检测一次玩家 而不是每一帧
        if (Time.time - lastDetectTime >= detectInterval)
        {
            CheckPlayerInRange();
            lastDetectTime = Time.time;

        }

        if (isChasing)
        {
            if (player != null)
            {

                ChasePlayer();

                AttackPlayer();

            }

        }
        else
        {
            Patrol();

        }

    }


    #region 巡逻
    private void Patrol()
    {
        //方向
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;

        //调用朝向反向
        FilpMonster(direction.x);

        //移动
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            RandomPoint();
        }


    }



    //随机生成目标点
    private void RandomPoint()
    {
        if (place1 != null && place2 != null)
        {
            float minX = Mathf.Min(place1.position.x, place2.position.x);
            float maxX = Mathf.Max(place1.position.x, place2.position.x);

            float randomX = Random.Range(minX, maxX);
            targetPoint = new Vector2(randomX, place1.position.y);

            Debug.Log("开始下一次巡逻");
        }
        else
        {
            Debug.Log("未设置巡逻起点和终点");
        }


    }

    #endregion


    #region 检测玩家
    private void CheckPlayerInRange()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            isChasing = true;
            Debug.Log("检测到玩家，开始追逐");
        }
        else if (distance > chaseRange + chaseBuffer)
        {
            isChasing = false;
            Debug.Log("未检测到，脱战");
        }




    }




    #endregion


    #region 追踪玩家
    private void ChasePlayer()
    {
        //计算方向
        Vector2 direction = (player.position - transform.position).normalized;

        //调用朝向反向
        FilpMonster(direction.x);

        //向玩家移动
        rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);

    }


    #endregion

    #region 朝向反向
    private void FilpMonster(float moveDirectionX)
    {
        if (moveDirectionX == 0)
        {
            return;
        }

        //计算新的缩放： 保留y,z不变  x轴 = 方向*基础缩放
        float newScaleX = Mathf.Sign(moveDirectionX) * originalScale.x;
        transform.localScale = new Vector3(newScaleX, originalScale.y, 1);

    }

    #endregion


    #region 攻击玩家核心逻辑

    private void AttackPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > monsterAttack.attackRange)
        {
            return;
        }

        if (Time.time - lastAttackTime < attackCD)
        {
            return;
        }

        monsterAttack.isAttack(useHeavyAttack);
        Debug.Log("怪物使用普通攻击");

        //记录时间+攻击时停步
        lastAttackTime = Time.time;
        rb.velocity = Vector2.zero;



    }






    #endregion


    #region 怪物阵亡掉落
    private void OnMonsterDeath()
    {
        if (itemOnWorldPrefabs == null || dropItem == null)
        {
            Debug.LogWarning($"{gameObject.name}未配置掉落预设体/掉落物品，跳过掉落");
            return;
        }

        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= dropRate)
        {
            try
            {
                Vector3 dropPos = transform.position + new Vector3(dropOffset.x, dropOffset.y, 0);

                GameObject dropObj = Instantiate(itemOnWorldPrefabs, dropPos, Quaternion.identity);
                ItemOnWorld itemComp = dropObj.GetComponent<ItemOnWorld>();
                SpriteRenderer dropsr =dropObj.GetComponent<SpriteRenderer>();
                if (itemComp != null)
                {
                    itemComp.item = dropItem;
                    //itemComp.item = dropItem;
                    // 新增：设置掉落物贴图
                    
                }
               
                if(dropsr != null)
                {
                    if(dropItem.icon != null)
                    {
                        dropsr.sprite = dropItem.icon;
                    }
                    else
                    {
                        Debug.LogWarning($"{dropItem.name} 未设置itemIcon图标！");
                    }
                    

                }
                else
                {
                    Debug.LogWarning($"{itemOnWorldPrefabs.name} 预设体未挂载SpriteRenderer组件，无法显示贴图！");
                }
                Debug.Log($"{gameObject.name}阵亡，触发掉落！生成物品：{dropItem.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"掉落生成失败：{e.Message}");  
                // 即使掉落失败，也不影响怪物销毁
            }

        }
        else
        {
            Debug.Log($"{gameObject.name}阵亡，掉落失败（概率{dropRate}%，随机数{randomValue:F1}）");

        }

    }


}

    #endregion

