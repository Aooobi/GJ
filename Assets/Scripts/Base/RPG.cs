using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 人物操作控制脚本  衡玉
/// </summary>
public class RPG : MonoBehaviour
{
    private CharacterStats characterStats;
    [Header("初始属性设置")]
    [SerializeField] private float initMoveSpeed = 1.0f;
    [SerializeField] private float initJumpHeight = 5.0f;

    [Header("攻击设置")]
    [SerializeField] private float heavyAttackCost = 1.0f;
    [SerializeField] private float baseAttackCD = 0.5f;
    private float lastLATime;
    private float lastHATime;



    //组件引用 2d？
    private Rigidbody2D rb; //2d刚体
    private SpriteRenderer sr;  //精灵渲染器组件

    //状态变量
    private bool isGrounded; //区分跳跃和行走 //注意之后tag问题
    private float horizontalInput;


    //设置初始值，初始化
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

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
    }
    private void Start()
    {
        
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal"); //-1左 0不动 1右

        //跳跃
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
             Jump();

        }

        //轻攻击
        if(Input.GetMouseButtonDown(0))
        {
            //轻攻击
            
            //背包的UI组件以及以后可能的UI控制：对此有修改
            //如果点击的是 UI，就不攻击
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            LightAttack();

        }

        //重攻击
        if(Input.GetMouseButtonDown(1))
        {
            //如果点击的是 UI，就不攻击
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            HeavyAttack();

        }

        //E技能
        if(Input.GetKeyDown(KeyCode.E))
        {
            SkillE();

        }


        //R技能
        if(Input.GetKeyDown(KeyCode.R))
        {
            SkillR();

        }

        //F技能
        if(Input.GetKeyDown(KeyCode.F))
        {
            SkillF();

        }

        #region 发射火球的方向
        //Vector3 mousePos = Camera.main.ScreenToWord



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
        if (horizontalInput != 0)
        {
            sr.flipX = horizontalInput < 0;
        }
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
            characterStats.fireBallShot();

            lastHATime = Time.time;
  
            Debug.Log($"释放重攻击,消耗{heavyAttackCost}花火，剩余{characterStats.currentSparks}花火");
            Debug.Log("效果拔群！");
        }

         #endregion

     #endregion

    #region 技能

        #region 技能E
        private void SkillE()
        {
            Debug.Log("释放技能E");

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






}
