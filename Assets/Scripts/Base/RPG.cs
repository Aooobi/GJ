using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 人物操作控制脚本  衡玉
/// </summary>
public class RPG : MonoBehaviour
{
    [Header("交互提示")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private float tipYOffset = 2f;
    private GameObject currentInteractTip;

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
    // ====== 新增：祭坛相关变量（开始）======
    private bool isInAltarRange = false; //是否在祭坛旁边
    private GameObject currentAltar; //当前靠近的祭坛对象
    private GameObject currentGodStatue; //当前靠近的神像对象
    private int lastAltarInteractDay = -1; //祭坛最后交互的游戏天数（每日限制）
    // ====== 新增：祭坛相关变量（结束）======


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

        if (characterSprite != null)
        {
            sr.sprite = characterSprite;

        }

        characterStats = GetComponent<CharacterStats>(); //把基础属性面板上的数值放进来
        if (characterStats == null)
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
        if (gameTimeEvent == null)
        {
            Debug.LogError("场景中无GameTimeEvent脚本！");
        }
        if (villageRelifePoint == null)
        {
            Debug.LogWarning("未赋值村庄复活点！");
        }



        // 获取背包面板上的BP_Exit脚本
        if (backpackPanel != null)
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

        if (moveInput == 0)
        {
            anim.ResetTrigger("move");
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetTrigger("move");
            anim.SetBool("isWalking", true);
        }

        //跳跃
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
                anim.SetTrigger("Jump");
            }

            if (!isGrounded)
            {
                anim.SetBool("isJumping", true);
            }
            else
            {
                anim.SetBool("isJumping", false);
            }
        }

        //轻攻击
        if (Input.GetMouseButtonDown(0))
        {
            LightAttack();
            anim.SetTrigger("LAtt");
        }

        //重攻击
        if (Input.GetMouseButtonDown(1))
        {
            HeavyAttack();
            anim.SetTrigger("HAtt");
        }

        //E技能
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (BP_Open == false)
            {
                OpenInventory();
                BP_Open = true;
            }
            else
            {
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
        if (Input.GetKeyDown(KeyCode.F))
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
        if (mouseWorldPos.x < transform.position.x)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, 1);
        }
        else if (mouseWorldPos.x > transform.position.x)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, 1);
        }
        #endregion
    }

    //物理更新
    private void FixedUpdate()
    {
        Move();
    }

    #region 移动逻辑
    private void Move()
    {
        if (isPlayerLocked)
        {
            return;
        }
        rb.velocity = new Vector2(horizontalInput * characterStats.moveSpeed, rb.velocity.y);
        Debug.Log("行走");
    }
    #endregion

    #region 跳跃逻辑
    private void Jump()
    {
        if (isPlayerLocked)
        {
            return;
        }
        float currentJumpHeight = characterStats.jumpHeight;
        rb.velocity = new Vector2(rb.velocity.x, currentJumpHeight);
        isGrounded = false;
        Debug.Log("跳跃");
    }
    #endregion

    #region 地面检测 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    #endregion

    #region 攻击
    #region 计算攻速
    private float GetAttackCD()
    {
        if (characterStats.attackSpeed <= 0)
        {
            return baseAttackCD;
        }
        return baseAttackCD / characterStats.attackSpeed;
    }
    #endregion

    #region 轻攻击方法
    private void LightAttack()
    {
        if (isPlayerLocked)
        {
            return;
        }
        float currentCD = GetAttackCD();
        if (Time.time - lastLATime < currentCD)
        {
            Debug.Log($"攻击冷却中，剩余{currentCD - (Time.time - lastLATime)}秒");
            return;
        }
        lastLATime = Time.time;

        if (attackSystem != null)
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
        if (isPlayerLocked)
        {
            return;
        }
        float currentCD = GetAttackCD();
        if (Time.time - lastHATime < currentCD)
        {
            Debug.Log($"攻击冷却中，剩余{currentCD - (Time.time - lastHATime):F1}秒");
            return;
        }
        if (!characterStats.ConsumeSparks(heavyAttackCost))
        {
            Debug.Log("花火值不足!");
            return;
        }

        CreateFireBall();
        lastHATime = Time.time;
        Debug.Log($"释放重攻击,消耗{heavyAttackCost}花火，剩余{characterStats.currentSparks}花火");
    }
    #endregion
    #endregion

    #region 技能
    #region E交互背包
    private void OpenInventory()
    {
        Debug.Log("打开背包");
        if (bpExitScript != null)
        {
            bpExitScript.SlideInFromLeft(new Vector2(-305.5f, 0f));
            BPEvent.Instance.OnInventoryStateChanged.Invoke(BP_Open);
        }
    }
    private void CloseInventory()
    {
        Debug.Log("关闭背包");
        if (bpExitScript != null)
        {
            bpExitScript.SlideOutToLeft();
            BPEvent.Instance.OnInventoryStateChanged.Invoke(BP_Open);
        }
    }
    private void OnInventoryStateChange(bool newState)
    {
        BP_Open = newState;
    }
    #endregion

    #region F交互按键（睡觉/神像/祭坛）- 仅修改此方法，新增祭坛判断
    private void PlayerInteract()
    {
        if (isCanSleep)
        {
            PlayerDoSleep();
            return;
        }
        // 新增：优先判断祭坛（每日花火+1）
        else if (isInAltarRange && currentAltar != null)
        {
            AltarDailyAddSparks();
            return;
        }
        // 原有：神像献祭（解锁层数）
        else if (isInGodStatueRange && currentGodStatue != null)
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
        GameObject fireBallObj = new GameObject("FireBall");
        fireBallObj.transform.position = transform.position + new Vector3(0.5f * transform.localScale.x, 0.5f, 0);
        fireBallObj.transform.localScale = transform.localScale * 5f;

        SpriteRenderer sr = fireBallObj.AddComponent<SpriteRenderer>();
        sr.sprite = fireBallSprite;
        sr.sortingOrder = 10;

        CircleCollider2D circleCol = fireBallObj.AddComponent<CircleCollider2D>();
        circleCol.isTrigger = true; // 必须设为Trigger，才能触发OnTriggerEnter2D
        circleCol.radius = 1f; // 半径1f，适配你5倍的缩放，足够检测到怪物

        FireBall fireBallScript = fireBallObj.AddComponent<FireBall>();
        fireBallScript.fireBallDamage = characterStats.heavyAttack;
    }
    #endregion

    #region 场景交互
    #region 靠近床/神像/祭坛 - 修改检测逻辑，新增祭坛判断
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bed"))
        {
            isCanSleep = true;
            Debug.Log("靠近床，可以睡觉");
            ShowInteractTip(other.transform);
        }
        // 原有：神像检测，新增记录当前神像对象
        else if (other.CompareTag("GodStatue"))
        {
            isInGodStatueRange = true;
            currentGodStatue = other.gameObject;
            Debug.Log("靠近特殊神像，按F献祭花火解锁区域！");
            ShowInteractTip(other.transform);
            // 新增：进入神像范围时，清空祭坛的标记，防止误判
            isInAltarRange = false;
            currentAltar = null;
        }
        // 新增：祭坛检测（Tag=Altar）
        else if (other.CompareTag("Altar"))
        {
            isInAltarRange = true;
            currentAltar = other.gameObject;
            Debug.Log("靠近祭坛，按F每日领取1点花火！");
            ShowInteractTip(other.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bed"))
        {
            isCanSleep = false;
            HideInteractTip();
        }
        // 原有：神像离开，新增清空神像对象
        else if (other.CompareTag("GodStatue"))
        {
            isInGodStatueRange = false;
            currentGodStatue = null;
            HideInteractTip();
        }
        // 新增：祭坛离开，清空祭坛标记
        else if (other.CompareTag("Altar"))
        {
            isInAltarRange = false;
            currentAltar = null;
            HideInteractTip();
        }
    }
    #endregion

   public  void PlayerDoSleep()
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

    #region 神像献祭 - 原有逻辑，新增【神像销毁+层数+1】
    public void GodStatueSacrifice()
    {
        if (!characterStats.SacrificeSparks()) return; // 献祭失败直接返回

        // 新增1：交互后销毁神像，防止重复刷进度
        if (currentGodStatue != null)
        {
            Destroy(currentGodStatue);
            currentGodStatue = null;
            isInGodStatueRange = false;
            Debug.Log("神像已销毁，无法重复交互");
        }

        // 新增2：解锁层数，后台记录进度
        characterStats.unlockArea += 1;
        Debug.Log($"【层数解锁】已通关第{characterStats.unlockArea}层！");

        // 原有：传送回村庄
        UIFadeEffect.Instance.FadeOutAndFadeIn(
            on_black: () => { RespawnToVillage(); },
            on_complete: () => { Debug.Log("献祭成功，已传回村庄！"); },
            active_panel_on_end: false
        );
    }
    #endregion

    #region 新增：祭坛每日花火+1核心方法
    private void AltarDailyAddSparks()
    {
        // 校验游戏天数脚本
        if (gameTimeEvent == null)
        {
            Debug.LogError("场景中无GameTimeEvent脚本，无法实现祭坛每日限制！");
            return;
        }
        int currentGameDay = gameTimeEvent.currentDay;

        // 每日限制：判断当天是否已交互
        if (lastAltarInteractDay == currentGameDay)
        {
            Debug.Log($"【祭坛每日限制】今日（第{currentGameDay}天）已领取花火，明日再来！");
            return;
        }

        // 花火+1，限制不超过最大值（防止溢出）
        characterStats.currentSparks += 1;
        if (characterStats.maxSparks > 0)
        {
            characterStats.currentSparks = Mathf.Min(characterStats.currentSparks, characterStats.maxSparks);
        }
        Debug.Log($"【祭坛领取成功】花火+1，当前花火：{characterStats.currentSparks}");

        // 更新最后交互天数，标记当天已领取
        lastAltarInteractDay = currentGameDay;
    }
    #endregion

    #region 交互提示 - 原有逻辑，无修改
    private void ShowInteractTip(Transform targetTrans)
    {
        if (textPrefab == null || mainCanvas == null || currentInteractTip != null)
        {
            return;
        }
        GameObject textObj = Instantiate(textPrefab, mainCanvas.transform);
        textObj.name = "InteractTipText";

        Vector3 worldPos = targetTrans.position + new Vector3(0, tipYOffset, 0);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null)
        {
            Debug.LogError("textPrefab缺少RectTransform组件！");
            Destroy(textObj);
            return;
        }
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
            Debug.LogError("坐标转换失败！");
            Destroy(textObj);
            return;
        }

        currentInteractTip = textObj;
        Destroy(textObj, 3f);
    }

    private void HideInteractTip()
    {
        if (currentInteractTip != null)
        {
            Destroy(currentInteractTip);
            currentInteractTip = null;
        }
    }

    private void OnDestroy()
    {
        HideInteractTip();
    }
    #endregion
    #endregion

    #region 复活逻辑 - 原有逻辑，无修改
    public void OnPlayerDead()
    {
        StopPlayerMovement();
        UIFadeEffect.Instance.FadeOutAndFadeIn
        (
            on_black: () =>
            {
                ResetAllProgress();
                RespawnToVillage();
                ResetMonsters();
                ResetGameDay();
            },
            on_complete: () =>
            {
                RevivePlayer();
                StartPlayerMovement();
                ConversationManager.instance.LoadConversationByName("玩家死亡复活后", false);
                Debug.Log("玩家复活，进度已清零！");
            },
            active_panel_on_end: false
        );
    }
    #region 进度清零
    private void ResetAllProgress()
    {
        characterStats.unlockArea = 0;
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
        rb.velocity = Vector2.zero;
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
        characterStats.Revive(1f);
        characterStats.currentSparks = characterStats.maxSparks;
    }
    #endregion
    #endregion

    #region 对话锁定实现接口 - 原有逻辑，无修改
    public void StopPlayerMovement()
    {
        if (isPlayerLocked)
        {
            return;
        }
        isPlayerLocked = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        Debug.Log("操作已锁定");
    }

    public void StartPlayerMovement()
    {
        if (isPlayerLocked == false)
        {
            return;
        }
        isPlayerLocked = false;
        Debug.Log("操作已恢复");
    }
    #endregion
}