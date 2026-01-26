using UnityEngine;
using DG.Tweening;

public class BP_Exit : MonoBehaviour
{
    // 滑出到左侧（屏幕外）
    public void SlideOutToLeft()
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        // 屏幕宽度（注意：UI 坐标系以像素为单位）
        float targetX = -Screen.width; // 滑出到左侧外

        // 使用 DOAnchorPos（UI 专用移动）
        rect.DOAnchorPosX(targetX, 2.4f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                gameObject.SetActive(false); // 动画完成后隐藏
            });
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