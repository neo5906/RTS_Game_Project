using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTechNode", menuName = "TechTree/Node")]
public class TechnologyNodeSO : ScriptableObject
{
    public string NodeID;
    public string NodeName;
    [TextArea(3, 5)]
    public string Description;
    public int CultureCost;
    public Vector2 PositionInTree;
    public List<TechnologyNodeSO> Prerequisites;
    
   
    [Header("UnlockBuildings")]
    [SerializeField] private List<BuildingActionSO> unlockedBuildings;

    [Header("Victory Condition")]
    [SerializeField] private bool isVictoryNode = false;

    [Header("Worker Upgrade Unlocks")]
    [SerializeField] private int unlockFortifyLevel = 0;   // 0 表示不升级
    [SerializeField] private int unlockDecorateLevel = 0;
    [SerializeField] private bool unlockRepair = false;

    public int UnlockFortifyLevel => unlockFortifyLevel;
    public int UnlockDecorateLevel => unlockDecorateLevel;
    public bool UnlockRepair => unlockRepair;
    public bool IsVictoryNode => isVictoryNode;

    public List<BuildingActionSO> UnlockedBuildings => unlockedBuildings;
    public bool IsUnlocked { get; private set; } 

    public void Unlock()
    {
        IsUnlocked = true;
    }

    public bool ArePrerequisitesMet()
    {
        foreach (var prereq in Prerequisites)
        {
            if (!prereq.IsUnlocked)
                return false;
        }
        return true;
    }

    public void ResetUnlockState()
    {
        IsUnlocked = false;
    }
}
