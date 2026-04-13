using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TechNodeUI : MonoBehaviour
{
    //[SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image background; // 可根据解锁状态
    [SerializeField] private Sprite unlockedbackground;
    [SerializeField] private Sprite lockedbackground;

    private GameManager m_GameManager;

    public RectTransform RectTransform => transform as RectTransform;
    public event Action<TechnologyNodeSO> OnNodeClicked;

    private TechnologyNodeSO node;
    private Button button;

    private void Awake()
    {
        m_GameManager = GameManager.Get();
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => OnNodeClicked?.Invoke(node));
    }

    public void Setup(TechnologyNodeSO techNode, GameManager gm)
    {
        node = techNode;
        nameText.text = node.NodeName;
        //iconImage.sprite = node.Icon;
        RectTransform.anchoredPosition = node.PositionInTree;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (node.IsUnlocked)
        {
            background.sprite = unlockedbackground;
            nameText.color = Color.black;
        }
        else
        {
            background.sprite = lockedbackground;
            nameText.color = Color.black;
        }
    }

    public void OnClickOpenDetail()
    {
        if (node == null)
        {
            Debug.LogWarning("TechNodeUI: 节点数据为空，无法打开详情界面。");
            return;
        }

        // 通过 GameManager 单例打开详情界面
        m_GameManager.OpenTechDetail(node);
    }
}
