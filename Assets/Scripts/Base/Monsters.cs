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

    }


    private void Update()
    {
        Patrol();


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


}
