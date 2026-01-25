using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPG : MonoBehaviour
{
    private CharacterStats characterStats;
    [Header("初始属性设置")]
    [SerializeField] private float initMoveSpeed = 1.0f;
    [SerializeField] private float initJumpHeight = 5.0f;

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
            // 右键？ 蓄力？（蓄力的动画/表示？）
            LightAttack();

        }

        //重攻击
        if(Input.GetMouseButtonDown(1))
        {
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
    }

    //物理更新
    private void FixedUpdate() //移动放在物理更新更稳定？    //跳跃不能放？
    {
         Move();
    }

    #region 移动逻辑
    private void Move()
    {
        rb.velocity = new Vector2(horizontalInput * characterStats.moveSpeed , rb.velocity.y);
        
        //角色翻转
        if(horizontalInput != 0)
        {
            sr.flipX = horizontalInput < 0; 
        }
    }

    #endregion


    #region 跳跃逻辑
    private void Jump()
    {
        float currentJumpHeight = characterStats.jumpHeight;
        rb.velocity = new Vector2(rb.velocity.x, currentJumpHeight);//x为跳跃前的movespeed
        isGrounded = false; //之后velocity会逐渐减少括号内的数值

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

        #region 轻攻击方法
        private void LightAttack()
        {
            Debug.Log("释放轻攻击");

        }

        #endregion

        #region 重攻击方法
        private void HeavyAttack()
        {
            Debug.Log("释放重攻击");

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
