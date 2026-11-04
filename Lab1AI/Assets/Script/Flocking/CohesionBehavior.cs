using System.Collections.Generic;
using UnityEngine;

public class CohesionBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        if (context.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (FlockAgent other in context)
        {
            center += other.transform.position;
        }
        center = center / context.Count;

        return center - agent.transform.position;
    }
}
