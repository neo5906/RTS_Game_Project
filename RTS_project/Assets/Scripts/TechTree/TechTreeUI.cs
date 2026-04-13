using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeUI : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private RectTransform contentArea;
    // 移除 LineRenderer 相关的预制体引用（如果有）
    // [SerializeField] private LineRenderer lineRendererPrefab; 
    [SerializeField] private Transform linesContainer; // 现在存放 UI Image 线条

    private TechnologyTreeSO techTree;
    private GameManager gameManager;
    private List<TechNodeUI> nodeUIs = new List<TechNodeUI>();
    private List<Image> drawnLines = new List<Image>(); // 用于存储绘制的线条，方便后续清除或修改

    public void Initialize(TechnologyTreeSO tree, GameManager gm)
    {
        techTree = tree;
        gameManager = gm;
        BuildTree();
    }

    private void BuildTree()
    {
        // 清除旧节点
        foreach (var nodeUI in nodeUIs)
            Destroy(nodeUI.gameObject);
        nodeUIs.Clear();

        // 清除旧线条
        foreach (var line in drawnLines)
            Destroy(line.gameObject);
        drawnLines.Clear();

        // 创建节点UI
        Dictionary<TechnologyNodeSO, TechNodeUI> nodeMap = new Dictionary<TechnologyNodeSO, TechNodeUI>();
        foreach (var node in techTree.AllNodes)
        {
            var go = Instantiate(nodePrefab, contentArea);
            var nodeUI = go.GetComponent<TechNodeUI>();
            nodeUI.Setup(node, gameManager);
            nodeUI.OnNodeClicked += HandleNodeClick;
            nodeUIs.Add(nodeUI);
            nodeMap[node] = nodeUI;
        }

        // 等待一帧绘制连线（确保节点布局已完成）
        StartCoroutine(DrawLinesNextFrame(nodeMap));
    }

    private IEnumerator DrawLinesNextFrame(Dictionary<TechnologyNodeSO, TechNodeUI> map)
    {
        yield return new WaitForEndOfFrame(); // 等待 UI 布局计算完成

        foreach (var node in techTree.AllNodes)
        {
            if (node.Prerequisites == null) continue;
            var targetUI = map[node];
            foreach (var prereq in node.Prerequisites)
            {
                if (map.TryGetValue(prereq, out var prereqUI))
                {
                    DrawLineBetween(prereqUI.RectTransform, targetUI.RectTransform);
                }
            }
        }
    }

    // 修改后的连线绘制方法，使用 Image 旋转拉伸
    private void DrawLineBetween(RectTransform from, RectTransform to)
    {
        // 可根据解锁状态改变颜色，这里简单用白色
        Color lineColor = Color.black;
        float lineWidth = 5f;

        Image lineImage = UILineDrawer.DrawLine(from, to, lineColor, lineWidth, linesContainer);
        drawnLines.Add(lineImage);
    }

    private void HandleNodeClick(TechnologyNodeSO node)
    {
        // 弹出详情界面
        var detailPanel = Instantiate(gameManager.techDetailPanelPrefab, transform.parent);
        var detailUI = detailPanel.GetComponent<TechDetailUI>();
        detailUI.Setup(node, gameManager);
    }

    public void RefreshAllNodes()
    {
        foreach (var nodeUI in nodeUIs)
        {
            nodeUI.RefreshVisual();
        }
    }

    public void ClosePanel()
    {
        gameManager.CloseTechTree();
    }

    public static class UILineDrawer
    {
        public static Image DrawLine(RectTransform from, RectTransform to, Color color, float width, Transform parent)
        {
            GameObject lineObj = new GameObject("UILine");
            lineObj.transform.SetParent(parent, false);
            Image img = lineObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            RectTransform rt = lineObj.GetComponent<RectTransform>();

            // 锚点：左上角单点锚定
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            // 轴心：中心点
            rt.pivot = new Vector2(0.5f, 0.5f);

            // 直接使用节点的 anchoredPosition（轴心相对于锚点的位置）
            Vector2 fromPos = from.anchoredPosition;
            Vector2 toPos = to.anchoredPosition;

            // 计算中点位置
            Vector2 midPoint = (fromPos + toPos) * 0.5f;
            rt.anchoredPosition = midPoint;

            // 计算方向、距离和旋转角度
            Vector2 dir = toPos - fromPos;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 设置线条尺寸（长度为两点距离，高度为线宽）
            rt.sizeDelta = new Vector2(distance, width);
            rt.rotation = Quaternion.Euler(0, 0, angle);

            return img;
        }
    }
}