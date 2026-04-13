using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechDetailUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private TechnologyNodeSO currentNode;
    private GameManager gameManager;

    public void Setup(TechnologyNodeSO node, GameManager gm)
    {
        currentNode = node;
        gameManager = gm;

        titleText.text = node.NodeName;
        descriptionText.text = node.Description;
        costText.text = $"{node.CultureCost}";

        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);

        // 如果已解锁，禁用确认按钮
        confirmButton.interactable = !node.IsUnlocked;
    }

    private void OnConfirm()
    {
        bool success = gameManager.TryUnlockTechNode(currentNode);
        if (success)
        {
            // 刷新当前详情界面
            confirmButton.interactable = false;
            // 也可提示成功
        }
        else
        {
            // 提示失败原因（资源不足或前置未解锁）
        }
    }

    private void OnCancel()
    {
        Destroy(gameObject);
    }
}