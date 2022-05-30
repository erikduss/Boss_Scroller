using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_MoveToAttackRange : Goal_Base
{
    [SerializeField] int MoveToPriority = 60;

    [HideInInspector] public DeathBringerGOAP deathbringer;

    int CurrentPriority = 0;

    public Vector3 MoveTarget => deathbringer.player != null ? deathbringer.player.transform.position : transform.position;

    private void Awake()
    {
        deathbringer = GetComponent<DeathBringerGOAP>();
    }

    public override void OnTickGoal()
    {
        CurrentPriority = 0;
    }

    public override void OnGoalDeactivated()
    {
        base.OnGoalDeactivated();
    }

    public override int CalculatePriority()
    {
        //add a formula that is based on distance and health of the enemy.

        return CurrentPriority;
    }

    private float playerDistance()
    {
        var vec = transform.position - deathbringer.player.transform.position;
        vec.y = 0;
        float dist = vec.magnitude;

        return dist;
    }

    public override bool CanRun()
    {
        if (deathbringer.walkFatigueTimer.TimerFinished())
        {
            if (playerDistance() > deathbringer.minDist)
            {
                return true;
            }
        }

        if(deathbringer.rbAI.velocity.magnitude > 0)
        {
            deathbringer.rbAI.velocity = Vector2.zero;
            deathbringer.animator.SetBool("Walking", false);
        }
        return false;
    }
}
