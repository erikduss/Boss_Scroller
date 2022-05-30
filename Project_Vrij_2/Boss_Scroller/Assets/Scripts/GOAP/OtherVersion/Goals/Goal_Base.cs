using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGoal
{
    int CalculatePriority();
    bool CanRun();

    void OnTickGoal();
    void OnGoalActivated(Action_Base _linkedAction);
    void OnGoalDeactivated();
}

public class Goal_Base : MonoBehaviour, IGoal
{
    protected Rigidbody2D rbAi;
    protected Action_Base LinkedAction;

    void Awake()
    {
        rbAi = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
    }

    void Update()
    {
        OnTickGoal();
    }

    public virtual int CalculatePriority()
    {
        return -1;
    }

    public virtual bool CanRun()
    {
        return false;
    }

    public virtual void OnTickGoal()
    {
        CalculatePriority();
    }

    public virtual void OnGoalActivated(Action_Base _linkedAction)
    {
        LinkedAction = _linkedAction;
    }

    public virtual void OnGoalDeactivated()
    {
        LinkedAction = null;
    }
}
