using UnityEngine;

public class FlockControlPanel : MonoBehaviour
{
    Flock flock;

    void Awake()
    {
        flock = GetComponent<Flock>();
    }

    void OnGUI()
    {
        if (flock == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 260, 700), GUI.skin.box);

        flock.agentCount = IntSlider("Agents", flock.agentCount, 1, 500);
        flock.leaderCount = IntSlider("Leaders (max)", flock.leaderCount, 0, Mathf.Min(flock.agentCount, 20));

        flock.cohesionWeight = FloatSlider("Cohesion", flock.cohesionWeight, 0f, 5f);
        flock.alignmentWeight = FloatSlider("Alignment", flock.alignmentWeight, 0f, 5f);
        flock.avoidanceWeight = FloatSlider("Avoidance", flock.avoidanceWeight, 0f, 5f);
        flock.leaderWeight = FloatSlider("leader followship", flock.leaderWeight, 0f, 5f);
        flock.obstacleWeight = FloatSlider("Obstacle avoidance", flock.obstacleWeight, 0f, 10f);

        flock.leaderOvertakeDistance = FloatSlider("Overtake to change the leader", flock.leaderOvertakeDistance, 0.5f, 6f);
        flock.speedVariation = FloatSlider("Velocity variation", flock.speedVariation, 0f, 0.5f);

        flock.neighborRadius = FloatSlider("Neighbour radius", flock.neighborRadius, 1f, 10f);
        flock.maxSpeed = FloatSlider("Maximum velocity", flock.maxSpeed, 1f, 15f);

        GUILayout.EndArea();
    }

    int IntSlider(string name, int value, int min, int max)
    {
        GUILayout.Label(name + ": " + value);
        return Mathf.RoundToInt(GUILayout.HorizontalSlider(value, min, max));
    }

    float FloatSlider(string name, float value, float min, float max)
    {
        GUILayout.Label(name + ": " + value.ToString("0.0"));
        return GUILayout.HorizontalSlider(value, min, max);
    }
}