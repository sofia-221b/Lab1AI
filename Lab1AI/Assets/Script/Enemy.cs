using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<ClickToMove>().transform;
    }

    private void Update()
    {
        agent.SetDestination(player.position);
    }
}
