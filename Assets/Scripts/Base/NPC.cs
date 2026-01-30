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
   // [SerializeField] private float npcScale = 2f;//新增村民基础缩放
    private Vector2 targetPoint;
    private bool isStop = false;

    [Header("玩家碰撞检测")]
    [SerializeField] private string playerTag = "Player";
    private bool isTouchingPlayer = false;

    [Header("村民贴图")]
    [SerializeField] private Sprite npcSprite;


    [Header("祭坛刷新")]
    [SerializeField] private Transform FreshPoint;

    [Header("刷新头顶提示配置")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas mainCanvas;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("NPC"),true);
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Player"),true);

        if(rb == null)
        {
            Debug.Log("NPC未挂载Rigidbody2D");
            return;
        }

        if(npcSprite != null)
        {
            sr.sprite = npcSprite;

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

        if(GameTimeEvent.Instance != null)
        {
            GameTimeEvent.Instance.OnSleepRefreshNextDay.AddListener(TeleportToFreshPoint);
        }
        else
        {
            Debug.LogError("场景中未挂载GameTimeEvent脚本！请挂到EventSystem上");
        }

    }

    // 新增2：移除监听（防止场景销毁时内存泄漏，规范写法）
    private void OnDestroy()
    {
        if (GameTimeEvent.Instance != null)
        {
            GameTimeEvent.Instance.OnSleepRefreshNextDay.RemoveListener(TeleportToFreshPoint);
        }
    }

    private void Update()
    {
        NPCpatrol();


    }

    #region 传送到预设位置
    public void TeleportToFreshPoint()
    {
        Vector2 targetPos = FreshPoint != null ? FreshPoint.position : transform.position;

        targetPos.y = -1.94f;
        //传送
        transform.position = targetPos;
        Debug.Log("村民已传送到祭坛位置");

        ShowRefreshText();
        //重置状态
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            isTouchingPlayer = false;
        }

        //重新随机目标点
        RandomPoint();

        // 新增以下3行 ↓ 核心延迟逻辑
        isStop = true; // 传送后立刻锁定，NPC停住
                       // 开启协程：等待4秒后，解除锁定让NPC移动
        StartCoroutine(StopForSeconds(4f));

    }

    #endregion

    #region 头顶刷新显示
    private void ShowRefreshText()
    {
        if(textPrefab == null || mainCanvas == null)
        {
            return;
        }
        //实例化
        GameObject textObj = Instantiate(textPrefab,mainCanvas.transform);
       

        Vector3 worldPos = transform.position + new Vector3(0,2f,0);

        //Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        //textObj.GetComponent<RectTransform>().position = screenPos;

        //坐标转换（兼容所有Canvas渲染模式的通用写法）
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null)
        {
            Debug.LogError("textPrefab缺少RectTransform组件！请确保是UI预设");
            Destroy(textObj);
            return;
        }
        // 关键：用Canvas的Camera进行坐标转换（支持Overlay/Screen Space - Camera/World Space）
        Vector2 screenPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(worldPos),
            mainCanvas.worldCamera,
            out screenPos))
        {
            textRect.anchoredPosition = screenPos;
        }
        else
        {
            Debug.LogError("坐标转换失败，请检查Canvas和相机设置");
            Destroy(textObj);
            return;
        }

        //3秒后自动销毁
        Destroy(textObj, 3f);


    }

    #endregion



    #region 巡逻
    private void NPCpatrol()
    {
        if (isStop) { rb.velocity = Vector2.zero; return; }

        //碰到主角会停止移动
        if (isTouchingPlayer)
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

        //朝向反向
        FlipNPC(direction.x);
        
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

            targetPoint = new Vector2(randomX, transform.position.y);

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

    #region 朝向反向
    private void FlipNPC(float DirectionX)
    {
        if(DirectionX == 0)
        {
            return;
        }

        float scaleX = Mathf.Sign(DirectionX) * originalScale.x;
        transform.localScale = new Vector3( scaleX , originalScale.y , originalScale.z);

    }


    #endregion


    #region 携程作为延迟
    // 新增：协程方法（专门做延迟解锁，只用写一次）
    private IEnumerator StopForSeconds(float delayTime)
    {
        yield return new WaitForSeconds(delayTime); // 等待指定秒数（这里4秒）
        isStop = false; // 时间到，解除锁定，NPC恢复巡逻
    }
    #endregion
}
