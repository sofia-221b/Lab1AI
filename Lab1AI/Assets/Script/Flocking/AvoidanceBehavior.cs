using System.Collections.Generic;
using UnityEngine;

public class AvoidanceBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        Vector3 avoidance = Vector3.zero;

        foreach (FlockAgent other in context)
        {
            Vector3 away = agent.transform.position - other.transform.position;
            float distance = away.magnitude;

            if (distance > 0f && distance < flock.AvoidanceRadius)
            {
                avoidance += away.normalized / distance;
            }
        }

        return avoidance;
    }
}
