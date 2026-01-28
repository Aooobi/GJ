using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 属性脚本  衡玉
/// </summary>

/*2.属性脚本，用于所有具有属性的游戏单位
  - 生命值
  - 花火值，用来释放技能，只能通过怪物掉落和睡觉补给获得填充，不同技能释放时会消耗火花值。还可以用来献祭，获得各种增益物品。
  - 各项移动参数控制，移动速度、攻击速度、跳跃高度
 */

#region 道具稀有度

public enum rareLevel
{
    one,  //村庄
    two,
    three, //最高级
    player
};

#endregion

#region  道具属性加成
public enum AddType
{ 
    //生命
    MaxHealth,
    CurrentHealth,

    //花火值
    MaxSparks,
    CurrentSparks,

    //移动参数
    MoveSpeed,
    JumpHeight,

    //攻击
    LightAttack,
    LAspeed,
    HeavyAttack,
    HAspeed,

    //防御力
    Defense,

}


#endregion


public class CharacterStats : MonoBehaviour
{
    #region 基础数值
    [Header("生命")]
    //生命
    public float maxHealth; 
    public float currentHealth;

    //花火值
    [Header("花火值")]
    public float maxSparks = 3f;
    public float currentSparks = 3f;

    //移动参数
    [Header("移动参数")]
    [HideInInspector]public float baseMoveSpeed=5f;
    public float moveSpeed;//当前
  
    [HideInInspector]public float baseJumpHeight=10f;
    public float jumpHeight;//当前

    //攻击力
    [Header("攻击")]
    public float baseLightAttack;
    public float baseHeavyAttack;
    public float lightAttack;//当前
    public float heavyAttack;//当前
    [HideInInspector] public float baseAttackSpeed = 1f;
    public float attackSpeed;

    //防御力
    [Header("防御")]
    public float Defense;

    #endregion

    #region 增幅计时
    //轻攻击增加
    private float LAgrowValue;
    private float LAgrowTime;

    //重攻击增加
    private float HAgrowValue;
    private float HAgrowTime;

    //移动速度增加
    private float MovegrowValue;
    private float MovegrowTime;

    //攻击速度增加
    private float APgrowValue;
    private float APgrowTime;

    #endregion


    //#region 花火子弹
    //[Header("花火子弹")]
    //public GameObject fireBall;
    //public Transform fireBallPoint;

    //#endregion

    #region 稀有度
    [Header("稀有度")]
    public rareLevel nowRareLevel;
    #endregion

    //开始游戏
    private void Start()
    {
        currentHealth = maxHealth;
        currentSparks = maxSparks;

        LAgrowTime = 0f;
        HAgrowTime = 0f;
        APgrowTime = 0f;
        lightAttack = baseLightAttack;
        heavyAttack = baseHeavyAttack;

        moveSpeed = baseMoveSpeed;
        MovegrowTime = 0f;

        attackSpeed = baseAttackSpeed;

        jumpHeight = baseJumpHeight;

        nowRareLevel = rareLevel.one;
        
    }

    //增幅道具
    //攻击力在一定时间内增加
    private void Update()
    {
        #region 攻击短时间内增加
        //轻攻击
         if (LAgrowTime > 0f)
         {
            LAgrowTime -= Time.deltaTime; //倒计时
            if(LAgrowTime <= 0f)
            {
                lightAttack = baseLightAttack;
                LAgrowTime = 0f;

                Debug.Log("轻攻击增益结束，恢复基础值：" + baseLightAttack);
            }
         }
        //重攻击
        if (HAgrowTime > 0f)
        {
            HAgrowTime -= Time.deltaTime; //倒计时
            if (HAgrowTime <= 0f)
            {
                heavyAttack = baseHeavyAttack;
                HAgrowTime = 0f;

                Debug.Log("重攻击增益结束，恢复基础值：" + baseHeavyAttack);
            }
        }
        #endregion

        #region 速度短时间内增加
        if(MovegrowTime > 0f)
        {
            MovegrowTime -= Time.deltaTime;
            if(MovegrowTime <= 0f)
            {
                moveSpeed = baseMoveSpeed;
                MovegrowTime = 0f;

                Debug.Log("速度增益结束，恢复基础值：" + baseMoveSpeed);
            }



        }


        #endregion

        #region 攻速短时间内增加
        if(APgrowTime > 0f)
        {
            APgrowTime -= Time.deltaTime;
            if(APgrowTime <= 0f)
            {   
                attackSpeed = baseAttackSpeed;
                APgrowTime = 0f;

                Debug.Log("攻速增益结束，恢复基础值：" + baseAttackSpeed);
            }


        }



        #endregion

    }


    #region 属性性能改变
    //当前生命值改变
    public void ChangeHealth(float value)
    {
        currentHealth += value;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    //最大生命值改变
    public void ChangeMaxHealth(float value)
    {
        maxHealth += value;
        
    }

    //消耗花火值
    public bool ConsumeSparks(float cost)
    {
        if (currentSparks >= cost)
        {
            currentSparks -= cost;
            return true;
        }
        return false;

    }

    //恢复花火值
    public void ResumeSparks(float value)
    {
        currentSparks += value;
        currentSparks = Mathf.Clamp(currentSparks, 0f, maxSparks);
    }

    //增加花火值上限
    public void ResumemaxSparks(float value)
    {
        maxSparks += value;

    }


    //增加攻击力
    //轻击
    public void UpLightAtk(float LAgrowValue , float LAgrowTime)
    {
        this.LAgrowTime = LAgrowTime;
        this.LAgrowValue = LAgrowValue;

        lightAttack += LAgrowValue;

        Debug.Log($"轻攻击提升至{lightAttack}，持续{LAgrowTime}秒");

    }

    //重击
    public void UpHeavyAtk(float HAgrowValue , float HAgrowTime)
    {
        this.HAgrowTime = HAgrowTime;
        this.HAgrowValue = HAgrowValue;

        heavyAttack += HAgrowValue;

        Debug.Log($"重攻击提升至{heavyAttack}，持续{HAgrowTime}秒");
    }


    //速度
    public void UpMoveSpeed(float MovegrowValue,float MovegrowTime)
    {
        this.MovegrowTime = MovegrowTime;
        this.MovegrowValue = MovegrowValue;

        moveSpeed += MovegrowValue;

        Debug.Log($"速度提升至{moveSpeed},持续{MovegrowTime}秒");

    }

    //攻速
    public void UpAttackSpeed(float APgrowValue,float APgrowTime)
    {
        this.APgrowTime = APgrowTime;
        this.APgrowValue = APgrowValue; 
        attackSpeed += APgrowValue;

        Debug.Log($"攻速提升至{attackSpeed},持续{APgrowTime}秒" );

    }

    #region 接口：属性增减
    /// <summary>
    /// 用于装备道具属性增减的接口
    /// </summary>
    /// 
    public bool WearAdd(AddType addType, float value, bool isPermanent = true, float duration = 0f)
    {
        try
        {
            if (isPermanent) //永久属性
            {
                switch (addType)
                {
                    #region 属性永久提升
                    //生命相关
                    case AddType.MaxHealth:
                        ChangeMaxHealth(value);
                        Debug.Log($"最大生命值永久提升+{value},当前最大生命值为{maxHealth}");
                        break;
                    case AddType.CurrentHealth:
                        ChangeHealth(value);
                        Debug.Log($"当前生命值提升+{value},当前生命值为{currentHealth}");
                        break;
                    //花火值相关
                    case AddType.MaxSparks:
                        ResumemaxSparks(value);
                        Debug.Log($"最大花火值永久提升+{value},当前最大花火值为{maxSparks}");
                        break;
                    case AddType.CurrentSparks:
                        ResumeSparks(value);
                        Debug.Log($"当前花火值提升+{value},当前花火值为{currentSparks}");
                        break;
                    //移动参数
                    case AddType.MoveSpeed:
                        moveSpeed += value;
                        Debug.Log($"移动速度永久提升+{value},当前移动速度为{moveSpeed}");
                        break;
                    case AddType.JumpHeight:
                        jumpHeight += value;
                        Debug.Log($"跳跃高度永久提升+{value},当前跳跃高度为{jumpHeight}");
                        break;

                    //攻击相关
                    case AddType.LightAttack:
                        lightAttack += value;
                        Debug.Log($"轻攻击永久提升+{value},当前轻攻击为{lightAttack}");
                        break;
                    case AddType.HeavyAttack:
                        heavyAttack += value;
                        Debug.Log($"重攻击永久提升+{value},当前重攻击为{heavyAttack}");
                        break;
                    case AddType.LAspeed:
                        attackSpeed += value;
                        Debug.Log($"攻击速度永久提升+{value},当前攻击速度为{attackSpeed}");
                        break;
                    case AddType.HAspeed:
                        attackSpeed += value;
                        Debug.Log($"攻击速度永久提升+{value},当前攻击速度为{attackSpeed}");
                        break;
                    //防御
                    case AddType.Defense:
                        Defense += value;
                        Debug.Log($"防御力永久提升+{value},当前防御力为{Defense}");
                        break;


                        #endregion

                }
            }
            else //短时间提升
            {
                switch (addType)
                {
                    #region 短时间提升
                    case AddType.LightAttack:
                        UpLightAtk(value, duration);
                        break;
                    case AddType.HeavyAttack:
                        UpHeavyAtk(value, duration);
                        break;
                    case AddType.MoveSpeed:
                        UpMoveSpeed(value, duration);
                        break;
                    case AddType.LAspeed: // 轻攻击速度=攻速
                        UpAttackSpeed(value, duration);
                        break;
                    case AddType.HAspeed: // 重攻击速度=攻速（简化）
                        UpAttackSpeed(value, duration);
                        break;
                    default:
                        Debug.LogWarning($"属性{addType}暂不支持临时修改，仅支持永久修改");
                        return false;

                        #endregion

                }

            }
            return true;
        }
        catch(System.Exception e)
        {
            Debug.LogError($"属性修改失败：{e.Message}");

            return false;
        }

    }

    #endregion


    #endregion


    //public void fireBallShot()
    //{
    //    if (fireBall != null && fireBallPoint != null)
    //    {
    //        GameObject fireball = Instantiate(fireBall, fireBallPoint.position, fireBallPoint.rotation);
    //        Debug.Log("发射火球");
    //    }
    //    else
    //    {
    //        Debug.Log("没有火球子弹");

    //    }


    //}




}
