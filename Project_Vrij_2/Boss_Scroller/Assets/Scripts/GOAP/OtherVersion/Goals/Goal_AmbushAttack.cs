using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_AmbushAttack : Goal_Base
{
    [SerializeField] int AmbushPriority = 50;

    [HideInInspector] public DeathBringerGOAP deathbringer;

    int CurrentPriority = 0;

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
        var vec = transform.position - deathbringer.player.transform.position;
        vec.y = 0;
        float dist = vec.magnitude;

        CurrentPriority = Mathf.FloorToInt(dist * 10f);

        return CurrentPriority;
    }

    public override bool CanRun()
    {
        if (deathbringer.attackFatigueTimer.TimerFinished())
        {
            return true;
        }
        return false;
    }
}
