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
    [SerializeField] private float moveSpeed =2f;
    private Vector2 targetPoint;

    [Header("怪物追踪逻辑")]
    [SerializeField] private float chaseRange = 5f;//追踪范围
    [SerializeField] private float chaseSpeed = 3f;//追踪速度
    [SerializeField] private float chaseBuffer = 1f;//脱战缓冲区间
    private bool isChasing = false;
    private float detectInterval = 0.2f;//检测间隔时间 避免每一帧检测 优化性能
    private float lastDetectTime; //上次检测时间


    private CharacterStats monstersStats;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private void Awake()
    {
        monstersStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Monster"),LayerMask.NameToLayer("Monster"),true);

        if(monstersStats == null )
        {
            Debug.Log("怪物未挂载属性脚本");
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.Log("角色未改变tag");
            return;
        }


    }

    private void Start()
    {
        RandomPoint();

        //初始化检测时间
        lastDetectTime = Time.time;

    }


    private void Update()
    {
        //性能优化 每隔0.2秒检测一次玩家 而不是每一帧
        if(Time.time - lastDetectTime >= detectInterval)
        {
            CheckPlayerInRange();
            lastDetectTime = Time.time;

        }

        if(isChasing)
        {
            if(player != null)
            {

                ChasePlayer();

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
        Vector2 direction  = (targetPoint - (Vector2)transform.position).normalized;
        //移动
         rb.velocity = new Vector2(direction.x * moveSpeed , rb.velocity.y);
        
        if(Vector2.Distance(transform.position,targetPoint) < 0.1f)
        {
            RandomPoint();
        }


    }



    //随机生成目标点
    private void RandomPoint()
    {
        if(place1 != null && place2 != null)
        {
            float minX = Mathf.Min(place1.position.x, place2.position.x);
            float maxX = Mathf.Max(place1.position.x, place2.position.x);

            float randomX = Random.Range(minX,maxX);
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

        float distance = Vector2.Distance(transform.position , player.position);

        if(distance <= chaseRange)
        {
            isChasing = true;
            Debug.Log("检测到玩家，开始追逐");
        }
        else if(distance > chaseRange + chaseBuffer)
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
        //向玩家移动
        rb.velocity = new Vector2(direction.x * chaseSpeed , rb.velocity.y);

    }


    #endregion

}
