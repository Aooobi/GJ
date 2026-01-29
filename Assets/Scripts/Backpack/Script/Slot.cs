using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public ItemBase itemOnslot;
    public Image slotImage;
    public Text Number;
    
    // 右键菜单相关
    public GameObject rightClickMenuPrefab;  // 右键菜单预制体
    private GameObject currentContextMenu;   // 当前显示的右键菜单
    
    public Sprite contextMenuBgSprite;   // 菜单背景
    public Sprite buttonNormalSprite;        // 按钮背景


    public void ItemOnClicked()
    {
        BPManager.UpdateItemInfo(itemOnslot.description);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键按下直接return
            return;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 右键按下触发其他逻辑
            print("按下右键了");
            ShowContextMenu(eventData.position);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        if (currentContextMenu != null)
        {
            Destroy(currentContextMenu);
        }

        var (canvasRect, uiCam) = GetCanvasInfo();
        if (canvasRect == null)
        {
            Debug.LogError("无法找到有效的 Canvas！");
            return;
        }

        if (rightClickMenuPrefab == null)
        {
            CreateAndShowContextMenu(mousePosition, canvasRect, uiCam);
        }
        else
        {
            currentContextMenu = Instantiate(rightClickMenuPrefab, canvasRect);
            RectTransform menuRect = currentContextMenu.GetComponent<RectTransform>();

            Vector2 position;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, mousePosition, uiCam, out position))
            {
                menuRect.anchoredPosition = position;
            }
            else
            {
                Debug.LogWarning("坐标转换失败，使用默认位置");
                menuRect.anchoredPosition = Vector2.zero;
            }

            Button[] buttons = currentContextMenu.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.GetComponentInChildren<Text>()?.text == "移动到快捷栏")
                {
                    button.onClick.AddListener(MoveToQuickBar);
                }
            }
        }
    }

private void CreateAndShowContextMenu(Vector2 mousePosition, RectTransform canvasRect, Camera uiCam)
{
    GameObject menuGO = new GameObject("ContextMenu");
    menuGO.transform.SetParent(canvasRect, false);

    Image backgroundImage = menuGO.AddComponent<Image>();
    // backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f); // 深灰色半透明背景
    if (contextMenuBgSprite != null)
    {
        backgroundImage.sprite = contextMenuBgSprite;
        backgroundImage.type = Image.Type.Sliced; // 如果是九宫格
    }
    else
    {
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f); // fallback
    }

    RectTransform menuRect = menuGO.GetComponent<RectTransform>();
    menuRect.sizeDelta = new Vector2(120, 80); // 宽度120，高度80（两个按钮）
    menuRect.pivot = new Vector2(0, 1); // 左上角为轴心

    Vector2 position;
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, mousePosition, uiCam, out position))
    {
        menuRect.anchoredPosition = position;
    }
    else
    {
        Debug.LogWarning("坐标转换失败，使用默认位置");
        menuRect.anchoredPosition = Vector2.zero;
    }

    // 创建垂直布局组（可选，但更稳定）
    VerticalLayoutGroup layoutGroup = menuGO.AddComponent<VerticalLayoutGroup>();
    layoutGroup.spacing = 5; // 按钮间距
    layoutGroup.padding.left = 0;
    layoutGroup.padding.right = 0;
    layoutGroup.padding.top = 0;
    layoutGroup.padding.bottom = 0;

    // 设置每个按钮的大小
    float buttonWidth = 80;
    float buttonHeight = 30;

    // === 按钮1: 移动到快捷栏 ===
    GameObject moveButtonGO = new GameObject("MoveToQuickBarButton");
    moveButtonGO.transform.SetParent(menuGO.transform, false);

    Button moveButton = moveButtonGO.AddComponent<Button>();
    Image moveImage = moveButtonGO.AddComponent<Image>();
    if (buttonNormalSprite != null)
    {
        moveImage.sprite = buttonNormalSprite;
        moveImage.type = Image.Type.Sliced; // 如果是九宫格切图
    }
    else
    {
        moveImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // fallback
    }

    RectTransform moveRect = moveButtonGO.GetComponent<RectTransform>();
    moveRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
    moveRect.anchorMin = Vector2.zero;
    moveRect.anchorMax = Vector2.one;
    moveRect.offsetMin = Vector2.zero;
    moveRect.offsetMax = Vector2.zero;

    // 添加文本
    GameObject moveTextGO = new GameObject("ButtonText");
    moveTextGO.transform.SetParent(moveButtonGO.transform, false);
    Text moveText = moveTextGO.AddComponent<UnityEngine.UI.Text>();
    moveText.text = "移动到快捷栏";
    moveText.color = Color.white;
    moveText.fontSize = 14;
    moveText.alignment = TextAnchor.MiddleCenter;

    // 设置字体
    if (moveText.font == null)
    {
        moveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    RectTransform moveTextRect = moveTextGO.GetComponent<RectTransform>();
    moveTextRect.sizeDelta = moveRect.sizeDelta;
    moveTextRect.anchorMin = Vector2.zero;
    moveTextRect.anchorMax = Vector2.one;
    moveTextRect.offsetMin = Vector2.zero;
    moveTextRect.offsetMax = Vector2.zero;

    // 添加点击事件
    moveButton.onClick.AddListener(MoveToQuickBar);

    // === 按钮2: 关闭菜单 ===
    GameObject closeButtonGO = new GameObject("CloseButton");
    closeButtonGO.transform.SetParent(menuGO.transform, false);

    Button closeButton = closeButtonGO.AddComponent<Button>();
    Image closeImage = closeButtonGO.AddComponent<Image>();
    if (buttonNormalSprite != null)
    {
        closeImage.sprite = buttonNormalSprite;
        closeImage.type = Image.Type.Sliced; // 如果是九宫格切图
    }
    else
    {
        closeImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // fallback
    }

    RectTransform closeRect = closeButtonGO.GetComponent<RectTransform>();
    closeRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
    closeRect.anchorMin = Vector2.zero;
    closeRect.anchorMax = Vector2.one;
    closeRect.offsetMin = Vector2.zero;
    closeRect.offsetMax = Vector2.zero;

    GameObject closeTextGO = new GameObject("ButtonText");
    closeTextGO.transform.SetParent(closeButtonGO.transform, false);
    Text closeText = closeTextGO.AddComponent<UnityEngine.UI.Text>();
    closeText.text = "关闭菜单";
    closeText.color = Color.white;
    closeText.fontSize = 14;
    closeText.alignment = TextAnchor.MiddleCenter;

    if (closeText.font == null)
    {
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
    closeTextRect.sizeDelta = closeRect.sizeDelta;
    closeTextRect.anchorMin = Vector2.zero;
    closeTextRect.anchorMax = Vector2.one;
    closeTextRect.offsetMin = Vector2.zero;
    closeTextRect.offsetMax = Vector2.zero;

    // 点击关闭菜单
    closeButton.onClick.AddListener(() =>
    {
        Destroy(menuGO);
        currentContextMenu = null;
    });

    // // 添加点击外部关闭触发器（同前）
    // GameObject closeTrigger = new GameObject("CloseTrigger");
    // closeTrigger.transform.SetParent(canvasRect, false);
    // Image triggerImage = closeTrigger.AddComponent<Image>();
    // triggerImage.color = new Color(0, 0, 0, 0); // 透明
    // triggerImage.raycastTarget = true;
    //
    // RectTransform triggerRect = closeTrigger.GetComponent<RectTransform>();
    // triggerRect.sizeDelta = new Vector2(Screen.width, Screen.height);
    // triggerRect.anchorMin = Vector2.zero;
    // triggerRect.anchorMax = Vector2.one;
    // triggerRect.anchoredPosition = Vector2.zero;
    //
    // Button triggerButton = closeTrigger.AddComponent<Button>();
    // triggerButton.onClick.AddListener(() =>
    // {
    //     Destroy(menuGO);
    //     Destroy(closeTrigger);
    //     currentContextMenu = null;
    // });

    currentContextMenu = menuGO;
}
    // 获取Canvas RectTransform
    private RectTransform GetCanvasRectTransform()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            return parentCanvas.GetComponent<RectTransform>();
        }
        
        // 如果没找到父级Canvas，则查找场景中的主Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            {
                return canvas.GetComponent<RectTransform>();
            }
        }
        
        return null;
    }

    // 移动到快捷栏的逻辑
    private void MoveToQuickBar()
    {
        if (itemOnslot != null)
        {
            Debug.Log("移动物品 " + itemOnslot.name + " 到快捷栏");
            // 在这里实现移动到快捷栏的具体逻辑
            // 例如：发送事件、调用管理器方法等
            QuickBarManager.AddItemToQuickBar(itemOnslot);
        }
        else
        {
            Debug.LogWarning("槽位中没有物品，无法移动到快捷栏");
        }
        
        // 关闭右键菜单
        if (currentContextMenu != null)
        {
            Destroy(currentContextMenu);
            currentContextMenu = null;
        }
    }
    
    private (RectTransform rectTransform, Camera uiCamera) GetCanvasInfo()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || 
                          parentCanvas.renderMode == RenderMode.WorldSpace)
                ? parentCanvas.worldCamera
                : null;
            return (parentCanvas.GetComponent<RectTransform>(), cam);
        }

        // Fallback: 查找根 Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            {
                Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    ? canvas.worldCamera
                    : null;
                return (canvas.GetComponent<RectTransform>(), cam);
            }
        }

        return (null, null);
    }
}