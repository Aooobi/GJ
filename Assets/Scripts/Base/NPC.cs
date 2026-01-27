using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 村民npc移动脚本  基础的移动+npc之间接触会穿透+npc碰到主角会停住   衡玉 
/// </summary>
public class NPC : MonoBehaviour
{
    [Header("村民移动")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform villageleft;
    [SerializeField] private Transform villageright;
    private Vector2 targetPoint;

    [Header("玩家碰撞检测")]
    [SerializeField] private string playerTag = "Player";
    private bool isTouchingPlayer = false;

    [Header("祭坛刷新")]
    [SerializeField] private Transform FreshPoint;


    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("NPC"),true);
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Player"),true);

        if(rb == null)
        {
            Debug.Log("NPC未挂载Rigidbody2D");
            return;
        }
        if(FreshPoint == null)
        {
            Debug.Log("脚本未关联祭坛位置");
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

    #region 传送到预设位置
    public void TeleportToFreshPoint()
    {
        Vector2 targetPos = FreshPoint != null ? FreshPoint.position : transform.position;

        //传送
        transform.position = targetPos;
        Debug.Log("村民已传送到祭坛位置");

        //重置状态
        if(rb != null)
        {
            rb.velocity = Vector2.zero;
            isTouchingPlayer = false;
        }

        //重新随机目标点
        RandomPoint();


    }


    #endregion


    #region 巡逻
    private void NPCpatrol()
    {
        //碰到主角会停止移动
        if(isTouchingPlayer)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Player"),false);
            return;

        }

        //移动时，强制忽略主碰撞体和玩家的物理碰撞
        //Physics.Ignore
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Player"),true);

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

    #region 检测是否碰撞主角
    //碰撞
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag(playerTag))
        {
            isTouchingPlayer = true;
            Debug.Log("检测碰撞到主角，npc停下");
        }
    }
    //离开 
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.CompareTag(playerTag))
        {
            isTouchingPlayer = false;
            Debug.Log("离开主角,继续巡逻");
        }

    }



    #endregion


}
