using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTechNode", menuName = "TechTree/Node")]
public class TechnologyNodeSO : ScriptableObject
{
    public string NodeID; // 唯一标识符
    public string NodeName; // 显示名称
    [TextArea(3, 5)]
    public string Description; // 详细描述
    //public Sprite Icon; // 节点图标
    public int CultureCost; // 解锁所需文化值
    public Vector2 PositionInTree; // 在Content区域中的相对坐标
    public List<TechnologyNodeSO> Prerequisites; // 前置科技节点
    public bool IsUnlocked { get; private set; } // 是否已解锁（运行时状态）

    // 解锁节点（由GameManager调用）
    public void Unlock()
    {
        IsUnlocked = true;
    }

    // 检查前置条件是否满足（所有前置节点都已解锁）
    public bool ArePrerequisitesMet()
    {
        foreach (var prereq in Prerequisites)
        {
            if (!prereq.IsUnlocked)
                return false;
        }
        return true;
    }

    // 重置解锁状态（用于重新开始游戏等场景）
    public void ResetUnlockState()
    {
        IsUnlocked = false;
    }
}
