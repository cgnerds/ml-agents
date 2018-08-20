using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SimpleAgent : Agent {



    public override void CollectObservations()
    {
        AddVectorObs(0);
    }

    public Bandit bandit;
    public override void AgentAction(float[] vectorAction, string textAction)
	{
        var action = (int)vectorAction[0];
        AddReward(bandit.PullArm(action));
        // Done();
    }

    public override void AgentReset()
    {
        bandit.Reset();
    }

    public override void AgentOnDone()
    {

    }

    public Academy academy;
    public float timeBetweenDesicionAtInference;
    private float tiemSinceDecision;

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    private void WaitTimeInference()
    {
        if(!academy.GetIsInference())
        {
            RequestDecision();
        }
        else
        {
            if(tiemSinceDecision >= timeBetweenDesicionAtInference)
            {
                tiemSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                tiemSinceDecision += Time.fixedDeltaTime;

            }
        }
    }
}
