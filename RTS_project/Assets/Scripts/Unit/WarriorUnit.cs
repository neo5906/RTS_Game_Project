using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorUnit : HumanoidUnit
{
    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer >= CheckFrequency)
        {
            FindClosestEnemyInRange();
            CheckTimer = Time.time;
            if (Target != null && !Target.IsDead)
            {
                if (CanAttackTarget())
                {
                    ai.ClearPath();
                    if (Time.time - AttackTimer >= AttackFrequency)
                    {
                        AttackTimer = Time.time;
                        ComboCounter %= 2;
                        anim.SetBool("Attack_Horizontal", true);
                        anim.SetInteger("ComboCounter", ComboCounter);
                    }
                }
                else
                {
                    MoveToDestination(Target.transform.position);
                }
            }
        }
    }
}
