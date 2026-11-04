using System.Collections.Generic;
using UnityEngine;

public class LeaderFollowBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        if (agent.isLeader) return Vector3.zero;

        FlockAgent nearest = null;
        float bestDistance = 0f;
        foreach (FlockAgent leader in flock.leaders)
        {
            float d = Vector3.Distance(leader.transform.position, agent.transform.position);
            if (nearest == null || d < bestDistance)
            {
                nearest = leader;
                bestDistance = d;
            }
        }
        if (nearest == null) return Vector3.zero;
        if (bestDistance > flock.followRadius) return Vector3.zero;
        if (bestDistance < flock.followDistance) return Vector3.zero;

        Vector3 fromLeader = agent.transform.position - nearest.transform.position;
        if (Vector3.Dot(fromLeader, nearest.transform.forward) > 0f) return Vector3.zero;

        Vector3 target = nearest.transform.position - nearest.transform.forward * flock.followDistance;
        return target - agent.transform.position;
    }
}

public class WanderBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        if (!agent.isLeader && context.Count > 0) return Vector3.zero;

        Vector3 toTarget = agent.wanderTarget - agent.transform.position;

        if (Time.time > agent.wanderExpire || toTarget.magnitude < 3f)
        {
            agent.wanderTarget = flock.RandomPoint(flock.areaRadius * 0.9f);
            agent.wanderExpire = Time.time + 10f;
            toTarget = agent.wanderTarget - agent.transform.position;
        }

        return toTarget.normalized;
    }
}

public class StayInRadiusBehavior : FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        Vector3 toCenter = flock.transform.position - agent.transform.position;

        float t = toCenter.magnitude / flock.areaRadius;
        if (t < 0.9f) return Vector3.zero;

        return toCenter * t * t;
    }
}
