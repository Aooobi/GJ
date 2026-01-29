using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 记录全局的时间  事件系统
/// </summary>
public class GameTimeEvent : MonoBehaviour
{
    //单例
    public static GameTimeEvent Instance;

    //睡觉刷新第二天
    public UnityEvent OnSleepRefreshNextDay;

    [Header("游戏内天数配置")]
    [SerializeField] private int startDay = 1;
    public int currentDay { get; private set; }
   

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentDay = startDay;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void SleepToNextDay()
    {
        currentDay++;
        Debug.Log($"游戏时间已更新为第{currentDay}天");
        if(OnSleepRefreshNextDay!=null)
        {
            OnSleepRefreshNextDay.Invoke();
        }
       
    }


}
