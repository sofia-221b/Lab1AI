using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        Vector3 direction = agent.velocity;
        if (direction.sqrMagnitude < 0.01f) direction = agent.transform.forward;
        direction.Normalize();

        RaycastHit hit;
        bool blocked = Physics.SphereCast(agent.transform.position, flock.boidRadius,
                                          direction, out hit, flock.lookAhead, flock.obstacleMask);
        if (!blocked) return Vector3.zero;

        Vector3 slide = Vector3.ProjectOnPlane(direction, hit.normal);

        if (slide.sqrMagnitude < 0.01f)
        {
            slide = Vector3.Cross(hit.normal, Vector3.up);
            if (slide.sqrMagnitude < 0.01f) slide = Vector3.Cross(hit.normal, Vector3.right);
        }

        Vector3 result = slide.normalized + hit.normal * 0.5f;

        float urgency = 1f - hit.distance / flock.lookAhead;
        return result.normalized * (1f + urgency);
    }
}
