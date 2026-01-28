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
  
    private Rigidbody2D rb;
    private Vector3 startPos;//发射起始位置
    private void Awake()
    {
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
            circleCol.radius = 0.2f; //设置半径 

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
        if(other.GetComponent<Rigidbody2D>())
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("Monster"))
            {
                Debug.Log("火球击中");
                Destroy(gameObject);
            }



        }

    }

}
