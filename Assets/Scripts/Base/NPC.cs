using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 村民npc移动脚本  基础的移动   衡玉 
/// </summary>
public class NPC : MonoBehaviour
{
    [Header("村民移动")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform villageleft;
    [SerializeField] private Transform villageright;
    private Vector2 targetPoint;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("NPC"),true);


        if(rb == null)
        {
            Debug.Log("NPC未挂载Rigidbody2D");
            return;
        }

    }

    private void Start()
    {
        RandomPoint();

    }

    private void Update()
    {
        NPCpatrol();


    }

    #region 巡逻
    private void NPCpatrol()
    {
        //计算方向
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
        //移动
        rb.velocity = new Vector2(direction.x * moveSpeed,rb.velocity.y);
        //重新随机目标点
        if(Vector2.Distance(transform.position,targetPoint) < 0.1f)
        {
            RandomPoint();

        }

    }

    private void RandomPoint()
    {
        if (villageleft != null && villageright !=null)
        {
            float maxX = Mathf.Max(villageleft.position.x,villageright.position.x);
            float minX = Mathf.Min(villageleft.position.x,villageright.position.x);

            float randomX = Random.Range(minX,maxX);

            targetPoint = new Vector2(randomX, villageleft.position.y);

            Debug.Log("村民继续巡逻");

        }
        else
        {
            Debug.Log("村民左右巡逻点未关联");

        }



    }



    #endregion
}
