using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    private Image image => GetComponent<Image>();

    [Header("UI References")]
    [SerializeField] private GameObject actionButtonPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    private List<ActionButton> actionButtons = new List<ActionButton>();

    private void Start()
    {
        HideActionBar();
    }

    public void RegisterActionButton(Sprite icon, UnityAction action)
    {
        if (content == null)
        {
            Debug.LogError("ActionBar: Content RectTransform 未赋值！");
            return;
        }

        GameObject newButton = Instantiate(actionButtonPrefab, content);
        ActionButton actionBtn = newButton.GetComponent<ActionButton>();
        actionBtn.InitializeButton(icon, action);
        actionButtons.Add(actionBtn);
    }

    public void ClearAllActionButtons()
    {
        foreach (var btn in actionButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        actionButtons.Clear();
    }

    public void ShowActionBar()
    {
        image.color = Color.white;
        if (scrollRect != null)
            scrollRect.horizontalNormalizedPosition = 0f; // 重置滚动位置到最左
    }

    public void HideActionBar()
    {
        image.color = Color.clear;
    }
}
