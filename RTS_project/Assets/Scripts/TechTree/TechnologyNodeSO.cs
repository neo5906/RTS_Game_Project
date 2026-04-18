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
