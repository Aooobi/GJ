using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIFadeEffect : MonoBehaviour
{
    public static UIFadeEffect Instance;
    [Header("黑屏图片")]
    public Image Black;
    [Header("时间参数")]
    [SerializeField] private float fade_duration = 1f;
    [SerializeField] private float stop_duration = 1f;

    [Header("需要显隐控制的面板")]
    public GameObject TargetPanel;
    //[Header("黑屏时触发的事件")]
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //测试一下 淡入淡出
        //FadeOutAndFadeIn(() => { }, () => { }, false);
       

    }
    #region 单次方法
    //淡出屏幕渐黑 单次方法
    public void FadeOut()
    {
        TargetPanel.gameObject.SetActive(false);
        Black.DOFade(1, fade_duration).SetEase(Ease.Linear);
    }

    public void FadeOut(Action on_fade_out)
    {
        TargetPanel.gameObject.SetActive(false);
        Black.DOFade(1, fade_duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            on_fade_out?.Invoke();
        });
    }
    //淡入屏幕渐亮 单次方法
    public void FadeIn()
    {
        Black.DOFade(0, fade_duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            TargetPanel.gameObject.SetActive(true);
        });
    }
    #endregion


    public void FadeOutAndFadeIn(Action on_black, Action on_complete = null,bool active_panel_on_end = true)
    {
        if (on_black == null) return;
        if (TargetPanel != null)
            TargetPanel.gameObject.SetActive(false);
        Black.DOFade(1, fade_duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            on_black?.Invoke();

            transform.DOMove(transform.position, stop_duration).OnComplete(() =>
            {
                Black.DOFade(0, fade_duration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (TargetPanel != null)
                        TargetPanel.gameObject.SetActive(active_panel_on_end);
                    //结束委托调用
                    on_complete?.Invoke();
                });

            });
        });
    }

    public void FadeOutAndFadeIn(float stopblack_duration,Action on_black, Action on_complete = null, bool active_panel_on_end = true)
    {
        if (on_black == null) return;
        if (TargetPanel != null)
            TargetPanel.gameObject.SetActive(false);
        Black.DOFade(1, fade_duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            on_black?.Invoke();

            transform.DOMove(transform.position, stopblack_duration).OnComplete(() =>
            {
                Black.DOFade(0, fade_duration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (TargetPanel != null)
                        TargetPanel.gameObject.SetActive(active_panel_on_end);
                    //结束委托调用
                    on_complete?.Invoke();
                });

            });
        });
    }
}
