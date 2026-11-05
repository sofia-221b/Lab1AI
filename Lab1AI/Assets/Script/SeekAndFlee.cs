using UnityEngine;
using UnityEngine.AI;

public class SeekAndFlee : MonoBehaviour 
{
    private NavMeshAgent agent;
    private Transform player;
    public float detectionRadius = 10f;
    public float catchDistance = 1.5f;

    public float fleeDistance = 5f;

    private enum EnemyState
    {
        Idle, Seek, Flee
    }
    private EnemyState currentState = EnemyState.Idle;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<ClickToMove>().transform;
    }

    void Idle()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    void Seek(Vector3 location)
    {
        agent.isStopped = false;
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        agent.isStopped = false;
        Vector3 fleeVector = this.transform.position - location;
        if (fleeVector.sqrMagnitude < 0.001f)
        {
            fleeVector = transform.forward;
        }
        fleeVector.Normalize();
        Vector3 fleeDestination = transform.position + fleeVector * fleeDistance;
        agent.SetDestination(fleeDestination);
    }

    private void Update()
    {
        float distance = Vector3.Distance(agent.transform.position, player.position);

        if (distance > detectionRadius)
        {
            currentState = EnemyState.Idle;
        } else if (currentState != EnemyState.Flee && distance <= catchDistance) {
            currentState = EnemyState.Flee;
        } else if (currentState == EnemyState.Idle)
        {
            currentState = EnemyState.Seek;
        }
        switch (currentState) {
            case EnemyState.Idle:
                Idle();
                break;

            case EnemyState.Seek:
                Seek(player.position);
                break;

            case EnemyState.Flee:
                Flee(player.position);
                break;
        }

    }
}
