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

    private Vector3 originalScale;

    private bool isCanSleep = false;

    private bool isPlayerLocked = false;

    [Header("初始火球贴图")]
    [SerializeField] private Sprite fireBallSprite;

    [Header("人物贴图")]
    [SerializeField] private Sprite characterSprite;

    [Header("复活&传送配置")]
    public Transform villageRelifePoint;
    public Transform monsterParent;
    private GameTimeEvent gameTimeEvent;
    private bool isInGodStatueRange = false; //是否在神像旁边


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
        attackSystem = GetComponent<Attack>();
        originalScale = transform.localScale;

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


        gameTimeEvent = GameTimeEvent.Instance;
        if(gameTimeEvent == null )
        {
            Debug.LogError("场景中无GameTimeEvent脚本！");
        }
        if(villageRelifePoint == null )
        {
            Debug.LogWarning("未赋值村庄复活点！");
        }

        
        
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
			anim.ResetTrigger("move");
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
			}

            if(!isGrounded) {
				anim.SetBool("isJumping", true);
			}
			else {
				anim.SetBool("isJumping", false);
			}
			

        }

        //轻攻击
        if(Input.GetMouseButtonDown(0))
        {
            //轻攻击
            LightAttack();
			anim.SetTrigger("LAtt");

        }

        //重攻击
        if(Input.GetMouseButtonDown(1))
        {
            HeavyAttack();
			anim.SetTrigger("HAtt");

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
            PlayerInteract();

        }

        //快捷键
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("按下快捷键1");

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("按下快捷键2");

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("按下快捷键3");

        }



        #region 发射火球的方向/角色朝向


        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        //角色翻转
        if(mouseWorldPos.x < transform.position.x)
        {
            //鼠标在左侧 角色朝左
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, 1);
        }
        else if(mouseWorldPos.x > transform.position.x)
        {
            //鼠标在右侧 角色朝右
            transform.localScale = new Vector3(originalScale.x, originalScale.y, 1);
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
        if(isPlayerLocked)
        {
            return;
        }
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
        if(isPlayerLocked)
        {
            return;
        }
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
            if(isPlayerLocked)
            {
                return;
            }
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
            else
            {
                Debug.LogWarning("未获取到Attack攻击脚本，请检查是否挂载！");
             }
             Debug.Log($"释放轻攻击,当前攻速{characterStats.attackSpeed},剩余冷却时间：{currentCD}秒");

         }

    #endregion

        #region 重攻击方法
    private void HeavyAttack()
        {
            if(isPlayerLocked)
        {
            return;
        }
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

        #region F交互按键（睡觉/神像兑换）
        private void PlayerInteract()
        {
            if(isCanSleep)
            {
                PlayerDoSleep();
                return;
            }
            else if(isInGodStatueRange)
             {
                GodStatueSacrifice();
                return;

            }

            Debug.Log("没有可交互的对象");
        }
        #endregion

        #region 技能R
        private void SkillR()
        {
            Debug.Log("释放技能R");

        }
    #endregion


    #endregion


    #region 火球建立
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
    #endregion


    #region 场景交互

    #region 靠近床睡觉/ 靠近神像
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bed"))
        {
            isCanSleep = true;
            Debug.Log("靠近床，可以睡觉(这个文本框还没做)");
        }
        else if (other.CompareTag("GodStatue"))
        {
            isInGodStatueRange = true;
            Debug.Log("靠近特殊神像，按F献祭花火解锁区域！");

        }

    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Bed"))
        {
            isCanSleep = false;
            Debug.Log("离开床，不能睡觉");

        }
        else if (other.CompareTag("GodStatue"))
        {
            isInGodStatueRange = false;

        }

    }

    private void PlayerDoSleep()
    {
        Debug.Log("开始睡觉,触发渐隐渐显");
        UIFadeEffect.Instance.FadeOutAndFadeIn(
            on_black: () =>
            {
                GameTimeEvent.Instance.SleepToNextDay();
                ResetMonsters();

        if (bpExitScript != null)
        {
            bpExitScript.SlideInFromLeft(new Vector2(-305.5f, 0f));
            BPEvent.Instance.OnInventoryStateChanged.Invoke(BP_Open);
        }
            },
            on_complete: () =>
            {
                Debug.Log("睡醒，启程——");

            },
            active_panel_on_end: false
    );
    
    
    
    
    }
    #endregion

    #region 神像献祭
    public void GodStatueSacrifice()
    {
        if (!characterStats.SacrificeSparks()) return; // 献祭失败直接返回
        // 献祭成功，调用淡入淡出传回村庄
        UIFadeEffect.Instance.FadeOutAndFadeIn(
            on_black: () => { RespawnToVillage(); },
            on_complete: () => { Debug.Log("献祭成功，已传回村庄！"); },
            active_panel_on_end: false
        );
    }
    #endregion

    
    #endregion


    #region 复活逻辑
        public void OnPlayerDead()
        {
            StopPlayerMovement();
            //调用渐隐渐显
            UIFadeEffect.Instance.FadeOutAndFadeIn
            (
                on_black: () =>
                {
                    ResetAllProgress(); // 清零玩家进度
                    RespawnToVillage(); // 传送回村庄
                    ResetMonsters();    // 刷新怪物
                    ResetGameDay();     // 重置游戏天数
                },
                on_complete: () =>
                {
                    RevivePlayer(); // 玩家满血复活
                    StartPlayerMovement(); // 解锁操作
                    
                    ConversationManager.instance.LoadConversationByName("玩家死亡复活后", false);
                    Debug.Log("玩家复活，进度已清零！");
                },
                active_panel_on_end: false
            );

        }
        #region 进度清零
            private void ResetAllProgress()
            {
                characterStats.unlockArea = 0; // 区域层数清零
                // 临时属性重置为基础值（攻速/移速/攻击力）
                characterStats.moveSpeed = characterStats.baseMoveSpeed;
                characterStats.attackSpeed = characterStats.baseAttackSpeed;
                characterStats.lightAttack = characterStats.baseLightAttack;
                characterStats.heavyAttack = characterStats.baseHeavyAttack;
            }


        #endregion

        #region 传送回起点
        private void RespawnToVillage()
        {
            transform.position = villageRelifePoint.position;
            rb.velocity = Vector2.zero; // 重置速度，防止漂移
        }



        #endregion

        #region 怪兽刷新
        private void ResetMonsters()
        {
            if (monsterParent == null)
            { 
             return;
            }

            MonsterSpawner spawner = monsterParent.GetComponent<MonsterSpawner>();
            if (spawner != null)
            {
                spawner.RefreshMonsters();
            }
            else
            {
            Debug.LogError("MonsterParent 上没有挂载 MonsterSpawner 脚本", this);
        }



    }


    #endregion

    #region 重置天数
    private void ResetGameDay()
        {
            gameTimeEvent.ResetGameDay();
        }

        #endregion

        #region 玩家复活
        private void RevivePlayer()
        {
            characterStats.Revive(1f); // 1f=满血复活
            characterStats.currentSparks = characterStats.maxSparks; // 满花火
        }
        #endregion



        #endregion

    #region 对话锁定实现接口
        public void StopPlayerMovement()
        {
            if(isPlayerLocked)
            {
                return;
            }
            isPlayerLocked = true;
            if(rb!=null)
            {
                rb.velocity = Vector2.zero;
            }
        
            Debug.Log("操作已锁定");
    
        }

        public void StartPlayerMovement()
        {
            if(isPlayerLocked==false)
            {
                return;
            }
            isPlayerLocked = false;
            Debug.Log("操作已恢复");
    
    
        }




    #endregion

    
}
