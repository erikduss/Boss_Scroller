using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTimer
{
    private float waitTime = 0;
    private float waitCounter = 0f;

    private bool finished = false;

    private void CheckTimer()
    {
        waitCounter += Time.deltaTime;

        if (waitCounter >= waitTime)
        {
            finished = true;
        }
    }

    public bool TimerFinished()
    {
        CheckTimer();

        return finished;
    }

    public void StartTimer(float _waitTime)
    {
        waitTime = _waitTime;

        waitCounter = 0f;

        finished = false;
    }
}
