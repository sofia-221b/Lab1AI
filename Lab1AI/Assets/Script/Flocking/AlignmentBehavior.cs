using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        if (context.Count == 0) return agent.transform.forward;

        Vector3 heading = Vector3.zero;
        foreach (FlockAgent other in context)
        {
            heading += other.transform.forward;
        }
        heading = heading / context.Count;

        return heading;
    }
}
