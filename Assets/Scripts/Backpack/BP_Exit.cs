using UnityEngine;
using DG.Tweening;

public class BP_Exit : MonoBehaviour
{
    // 在Awake时将面板设置在摄像机左侧
    private void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();
        // 将面板初始位置设为屏幕左侧外
        // rect.anchoredPosition = new Vector2(-Screen.width, rect.anchoredPosition.y);
        rect.anchoredPosition = new Vector2(-680f,0f);
    }

    // 滑入视图（从左侧滑入到指定位置）
    public void SlideInFromLeft(Vector2 targetPos = default(Vector2))
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        // 如果没有指定目标位置，则默认为中心位置
        if(targetPos.Equals(default(Vector2)))
        {
            targetPos = Vector2.zero;
        }

        // 使用 DOAnchorPos（UI 专用移动）滑入到目标位置
        rect.DOAnchorPos(targetPos, 0.6f)
            .SetEase(Ease.OutCubic);
    }

    // 滑出到指定位置
    public void SlideOutToPosition(Vector2 targetPos)
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        // 使用 DOAnchorPos（UI 专用移动）滑出到指定位置
        rect.DOAnchorPos(targetPos, 0.6f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // 滑出后重新设置到初始位置，以便下次滑入
                // rect.anchoredPosition = new Vector2(-Screen.width, rect.anchoredPosition.y);
                rect.anchoredPosition = new Vector2(-680f,0f);
            });
    }

    // 滑出到左侧（屏幕外）- 保持原有方法兼容性
    public void SlideOutToLeft()
    {
        SlideOutToPosition(new Vector2(-680f,0f));
    }

    // 淡出并销毁
    public void FadeOutAndDestroy()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        cg.DOFade(0, 0.2f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    // 弹跳关闭（带缩放反馈）
    public void BounceOut()
    {
        transform.DOScale(0.8f, 0.1f)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}