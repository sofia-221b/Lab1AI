using System.Collections.Generic;
using UnityEngine;

public abstract class FlockBehavior
{
    public abstract Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock);
}

public class CompositeBehavior : FlockBehavior
{
    public FlockBehavior[] behaviors;
    public float[] weights;

    public CompositeBehavior(FlockBehavior[] behaviors)
    {
        this.behaviors = behaviors;
        this.weights = new float[behaviors.Length];
    }

    public override Vector3 CalculateMove(FlockAgent agent, List<FlockAgent> context, Flock flock)
    {
        Vector3 move = Vector3.zero;

        for (int i = 0; i < behaviors.Length; i++)
        {
            Vector3 partialMove = behaviors[i].CalculateMove(agent, context, flock) * weights[i];

            if (partialMove.sqrMagnitude > weights[i] * weights[i])
            {
                partialMove = partialMove.normalized * weights[i];
            }

            move += partialMove;
        }

        return move;
    }
}
