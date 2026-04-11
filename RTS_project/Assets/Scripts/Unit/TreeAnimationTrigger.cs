using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAnimationTrigger : MonoBehaviour
{
    private TreeUnit tree => GetComponentInParent<TreeUnit>();

    private void DestroyTree() => tree?.DestroyUnit();
}
