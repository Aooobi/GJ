using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 人物操作控制脚本  衡玉
/// </summary>
public class RPG : MonoBehaviour
{
    private CharacterStats characterStats;
    private Attack attackSystem; //引用攻击脚本
    [Header("初始属性设置")]
    [SerializeField] private float initMoveSpeed = 1.0f;
    [SerializeField] private float initJumpHeight = 5.0f;

    [Header("攻击设置")]
    [SerializeField] private float heavyAttackCost = 1.0f;
    [SerializeField] private float baseAttackCD = 0.5f;
    private float lastLATime;
    private float lastHATime;
    
    [Header("背包")]
    [SerializeField] private GameObject backpackPanel;
    private BP_Exit bpExitScript;

    private bool BP_Open = false;

    private Animator anim;


    [Header("初始火球贴图")]
    [SerializeField] private Sprite fireBallSprite;

    [Header("人物贴图")]
    [SerializeField] private Sprite characterSprite;


    //组件引用 2d？
    private Rigidbody2D rb; //2d刚体
    private SpriteRenderer sr;  //精灵渲染器组件

    //状态变量
    private bool isGrounded; //区分跳跃和行走 //注意之后tag问题
    private float horizontalInput;
    private float moveInput; //动画判断组件


    //设置初始值，初始化
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if(characterSprite != null)
        {
            sr.sprite = characterSprite;

        }
        //float baseScale = 8f;
        //transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * baseScale, baseScale, 1);



        characterStats = GetComponent<CharacterStats>(); //把基础属性面板上的数值放进来
        if(characterStats == null)
        {
            characterStats = gameObject.AddComponent<CharacterStats>();
            characterStats.moveSpeed = initMoveSpeed;
            characterStats.jumpHeight = initJumpHeight;
            Debug.Log("添加基础属性组件，设置初始值");
        }
        else
        {
           
            Debug.Log("初始化属性");
        }

        lastLATime = -baseAttackCD;
        lastHATime = -baseAttackCD;
        
        
        // 获取背包面板上的BP_Exit脚本
        if(backpackPanel != null)
        {
            bpExitScript = backpackPanel.GetComponent<BP_Exit>();
        }

        BPEvent.Instance.OnInventoryStateChanged.AddListener(OnInventoryStateChange);
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal"); //-1左 0不动 1右
		moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput == 0) {
			anim.SetBool("isWalking", false);
		}
		else {
			anim.SetTrigger("move");
			//根据玩家速度属性判断走还是跑，逻辑等待实现
			//
			//
			//根据玩家速度属性判断走还是跑，逻辑等待实现
			anim.SetBool("isWalking", true);
		}

        //跳跃
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(isGrounded) {
				Jump();
				anim.SetTrigger("Jump");
				anim.SetBool("isJumping", true);
			}
			

        }

        //轻攻击
        if(Input.GetMouseButtonDown(0))
        {
            //轻攻击
            LightAttack();

        }

        //重攻击
        if(Input.GetMouseButtonDown(1))
        {
            HeavyAttack();

        }

        //E技能
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (BP_Open == false) {
                OpenInventory();
                BP_Open = true;
            } else {
                CloseInventory();
                BP_Open = false;
            }

        }


        //R技能
        if (Input.GetKeyDown(KeyCode.R))
        {
            SkillR();

        }

        //F技能
        if(Input.GetKeyDown(KeyCode.F))
        {
            SkillF();

        }

        #region 发射火球的方向/角色朝向

        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        //角色翻转
        if(mouseWorldPos.x < transform.position.x)
        {
            //鼠标在左侧 角色朝左
            transform.localScale = new Vector3(-1,1,1);
        }
        else if(mouseWorldPos.x > transform.position.x)
        {
            //鼠标在右侧 角色朝右
            transform.localScale = new Vector3(1,1,1);
        }





        #endregion

    }

    //物理更新
    private void FixedUpdate() //移动放在物理更新更稳定？    //跳跃不能放？
    {
         Move();
    }

    #region 移动逻辑
    private void Move()
    {
        rb.velocity = new Vector2(horizontalInput * characterStats.moveSpeed, rb.velocity.y);
       // Debug.Log("Horizontal Input: " + horizontalInput);
        //角色翻转
        //if (horizontalInput != 0)
        //{
        //    sr.flipX = horizontalInput < 0;
        //}
        Debug.Log("行走");
    }

    #endregion


    #region 跳跃逻辑
    private void Jump()
    {
        float currentJumpHeight = characterStats.jumpHeight;
        rb.velocity = new Vector2(rb.velocity.x, currentJumpHeight);//x为跳跃前的movespeed
        isGrounded = false; //之后velocity会逐渐减少括号内的数值
        Debug.Log("跳跃");
    }

    #endregion

    //记得改tag

    #region 地面检测 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        

    }


    #endregion

    #region 攻击


        #region 计算攻速
        private float GetAttackCD()
        {
            if(characterStats.attackSpeed <= 0)
            {
                return baseAttackCD;
            }

            //冷却时间 = 基础冷却 / 当前攻速
            return baseAttackCD / characterStats.attackSpeed;

        }


        #endregion


         #region 轻攻击方法
         private void LightAttack()
         {
            float currentCD = GetAttackCD();
            if(Time.time - lastLATime < currentCD)
             {
                Debug.Log($"攻击冷却中，剩余{currentCD - (Time.time - lastLATime)}秒");
                return;
             }
            lastLATime = Time.time;
            
            //调用轻攻击
            if(attackSystem != null)
            {
                attackSystem.isAttack(false);    

            }
            Debug.Log($"释放轻攻击,当前攻速{characterStats.attackSpeed},剩余冷却时间：{currentCD}秒");

          }

    #endregion

    #region 重攻击方法
    private void HeavyAttack()
        {
            float currentCD = GetAttackCD();
            //检查攻击冷却
            if(Time.time - lastHATime < currentCD)
            {
                Debug.Log($"攻击冷却中，剩余{currentCD - (Time.time - lastHATime):F1}秒");
                return; 
            }
            //检查花火剩余值
            if(!characterStats.ConsumeSparks(heavyAttackCost))
            {
                Debug.Log("花火值不足!");
                return;

            }

            CreateFireBall();

            lastHATime = Time.time;
  
            Debug.Log($"释放重攻击,消耗{heavyAttackCost}花火，剩余{characterStats.currentSparks}花火");
            Debug.Log("效果拔群！");
        }

    #endregion


    #endregion

    #region 技能

    #region E交互背包
    private void OpenInventory()
    {
        Debug.Log("打开背包");

        if(bpExitScript != null)
        {
            bpExitScript.SlideInFromLeft(new Vector2(-305.5f,0f));
			BPEvent.Instance.OnInventoryStateChanged.Invoke(BP_Open);
        }

    }
    private void CloseInventory()
    {
        Debug.Log("关闭背包");

        if(bpExitScript != null)
        {
            bpExitScript.SlideOutToLeft();
			BPEvent.Instance.OnInventoryStateChanged.Invoke(BP_Open);
        }

    }

    private void OnInventoryStateChange(bool newState)
    {
        BP_Open = newState;
        // 可选：这里可以加额外逻辑，比如背包关闭时恢复玩家移动
        // PlayerCanMove = !newState;
    }

    #endregion

        #region 技能F
        private void SkillF()
        {
            Debug.Log("释放技能F");

        }
        #endregion

        #region 技能R
        private void SkillR()
        {
            Debug.Log("释放技能R");

        }
    #endregion


    #endregion



    public void CreateFireBall()
    {
        //动态创建空物体
        GameObject fireBallObj = new GameObject("FireBall");
        //设置火球位置 玩家位置 
        fireBallObj.transform.position = transform.position + new Vector3(0.5f * transform.localScale.x, 0.5f, 0);
        //继承玩家的缩放（保证和玩家的朝向一致）再乘以一个系数
        //float fireBallScale = Mathf.Abs(transform.localScale.x) * 0.1f;
        fireBallObj.transform.localScale = transform.localScale;
        //fireBallObj.transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * fireBallScale,fireBallScale,1);

        //贴图核心
        SpriteRenderer sr = fireBallObj.AddComponent<SpriteRenderer>();
        sr.sprite = fireBallSprite;//赋值拖入的火球脚本
        sr.sortingOrder = 10;//贴图层级置顶，不被遮挡


        FireBall fireBallScript = fireBallObj.AddComponent<FireBall>();

    }

}
