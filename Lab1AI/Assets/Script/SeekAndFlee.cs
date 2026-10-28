using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SeekAndFlee : MonoBehaviour 
{
    private NavMeshAgent agent;
    private Transform player;
    public float safetyDistance = 5f;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<ClickToMove>().transform;
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    private void Update()
    {
        float distance = Vector3.Distance(agent.transform.position, player.position);

        if (distance > safetyDistance)
        {
            Seek(player.position);
        }
        else
        {
            Flee(player.position);
        }
    }
}
