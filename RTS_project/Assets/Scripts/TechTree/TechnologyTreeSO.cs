using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TechTree", menuName = "TechTree/Tree")]
public class TechnologyTreeSO : ScriptableObject
{
    public List<TechnologyNodeSO> AllNodes;

    public void ResetAllNodes()
    {
        foreach (var node in AllNodes)
        {
            node.ResetUnlockState();
        }
    }
}