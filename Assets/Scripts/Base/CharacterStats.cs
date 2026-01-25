using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*2.属性脚本，用于所有具有属性的游戏单位
  - 生命值
  - 花火值，用来释放技能，只能通过怪物掉落和睡觉补给获得填充，不同技能释放时会消耗火花值。还可以用来献祭，获得各种增益物品。
  - 各项移动参数控制，移动速度、攻击速度、跳跃高度
 */
public class CharacterStats : MonoBehaviour
{
    #region 基础数值
    //生命
    public float maxHealth; 
    public float currentHealth;

    //花火值
    public float maxSparks;
    public float currentSparks;

    //移动参数
    public float baseMoveSpeed;
    public float moveSpeed;//当前

    public float attackSpeed;
    public float jumpHeight;

    //攻击力
    public float baseLightAttack;
    public float baseHeavyAttack;
    public float lightAttack;//当前
    public float HeavyAttack;//当前

    //防御力
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

    #endregion

    
    //开始游戏
    private void Start()
    {
        currentHealth = maxHealth;
        currentSparks = maxSparks;

        LAgrowTime = 0f;
        HAgrowTime = 0f;
        lightAttack = baseLightAttack;
        HeavyAttack = baseHeavyAttack;

        moveSpeed = baseMoveSpeed;
        MovegrowTime = 0f;
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
                HeavyAttack = baseHeavyAttack;
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

        HeavyAttack += HAgrowValue;

        Debug.Log($"重攻击提升至{HeavyAttack}，持续{HAgrowTime}秒");
    }


    //速度
    public void UpMoveSpeed(float MovegrowValue,float MovegrowTime)
    {
        this.MovegrowTime = MovegrowTime;
        this.MovegrowValue = MovegrowValue;

        moveSpeed += MovegrowValue;

        Debug.Log($"速度提升至{moveSpeed},持续{MovegrowTime}秒");

    }



    #endregion







}
