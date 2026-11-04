using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [Header("Cantidades")]
    [Min(1)] public int agentCount = 50;   
    [Min(0)] public int leaderCount = 1;   

    [Header("Fuerzas (cuánto pesa cada regla)")]
    [Range(0, 5)] public float cohesionWeight = 1f;       
    [Range(0, 5)] public float alignmentWeight = 1f;      
    [Range(0, 5)] public float avoidanceWeight = 1.5f;    
    [Range(0, 5)] public float leaderWeight = 1.5f;       
    [Range(0, 5)] public float wanderWeight = 1f;         
    [Range(0, 5)] public float stayInRadiusWeight = 2f;   
    [Range(0, 10)] public float obstacleWeight = 5f;      

    [Header("Movimiento")]
    public float driveFactor = 10f;        
    public float maxSpeed = 5f;
    public float minSpeed = 1f;
    public float smoothTime = 0.3f;        
    public float updateInterval = 0.1f;    
    [Range(0, 0.5f)] public float speedVariation = 0.15f; 
    public float speedChangeRate = 0.2f;   

    [Header("Vecinos")]
    public float neighborRadius = 3f;                                
    [Range(0.1f, 1f)] public float avoidanceRadiusMultiplier = 0.5f;

    [Header("Líderes")]
    public float leaderCheckInterval = 0.5f;     
    [Min(1)] public int minGroupSize = 3;        
    public float followRadius = 10f;             
    public float followDistance = 3f;            
    public float leaderOvertakeDistance = 2f;    
    public float leaderOvertakeTime = 1f;        
    public float leaderMinTime = 4f;             

    [Header("Zona")]
    public float areaRadius = 20f;

    [Header("Obstáculos")]
    public LayerMask obstacleMask;         
    public float lookAhead = 4f;           
    public float boidRadius = 0.4f;

    [Header("Aspecto")]
    public Texture2D arrowTexture;         
    public Color followerColor = Color.cyan;
    public Color leaderColor = Color.yellow;

    public List<FlockAgent> agents = new List<FlockAgent>();
    public List<FlockAgent> leaders = new List<FlockAgent>();

    public float AvoidanceRadius { get { return neighborRadius * avoidanceRadiusMultiplier; } }

    CompositeBehavior followerBehavior;
    CompositeBehavior leaderBehavior;
    Texture2D texture;
    float leaderTimer;
    int lastLeaderCount = -1;
    int lastAgentCount = -1;

    void Awake()
    {
        texture = arrowTexture != null ? arrowTexture : MakeArrowTexture();

        followerBehavior = new CompositeBehavior(new FlockBehavior[]
        {
            new CohesionBehavior(),
            new AlignmentBehavior(),
            new AvoidanceBehavior(),
            new LeaderFollowBehavior(),
            new WanderBehavior(),
            new StayInRadiusBehavior(),
            new ObstacleAvoidanceBehavior()
        });

        leaderBehavior = new CompositeBehavior(new FlockBehavior[]
        {
            new WanderBehavior(),
            new AvoidanceBehavior(),
            new StayInRadiusBehavior(),
            new ObstacleAvoidanceBehavior()
        });
    }

    void Update()
    {
        while (agents.Count < agentCount)
        {
            agents.Add(SpawnAgent());
        }
        while (agents.Count > agentCount)
        {
            FlockAgent last = agents[agents.Count - 1];
            agents.RemoveAt(agents.Count - 1);
            leaders.Remove(last);
            Destroy(last.gameObject);
        }

        leaderTimer += Time.deltaTime;
        bool changed = leaderCount != lastLeaderCount || agentCount != lastAgentCount;
        if (leaderTimer >= leaderCheckInterval || changed)
        {
            UpdateLeaders();
        }

        followerBehavior.weights = new float[]
        {
            cohesionWeight, alignmentWeight, avoidanceWeight, leaderWeight,
            wanderWeight, stayInRadiusWeight, obstacleWeight
        };
        leaderBehavior.weights = new float[]
        {
            wanderWeight, avoidanceWeight, stayInRadiusWeight, obstacleWeight
        };

        Physics.SyncTransforms();
        foreach (FlockAgent agent in agents)
        {
            if (Time.time >= agent.nextCalcTime)
            {
                agent.nextCalcTime = Time.time + updateInterval;

                List<FlockAgent> context = GetNearbyAgents(agent);
                CompositeBehavior behavior = agent.isLeader ? leaderBehavior : followerBehavior;

                Vector3 move = behavior.CalculateMove(agent, context, this);
                move *= driveFactor;

                float speedLimit = maxSpeed * agent.SpeedFactor;
                if (agent.isLeader)
                {
                    speedLimit *= LeaderSlowdown(agent);
                }

                if (move.sqrMagnitude > speedLimit * speedLimit)
                {
                    move = move.normalized * speedLimit;
                }
                agent.desiredMove = move;
            }

            agent.Move(agent.desiredMove);
        }
    }

    List<FlockAgent> GetNearbyAgents(FlockAgent agent)
    {
        List<FlockAgent> context = new List<FlockAgent>();
        Collider[] hits = Physics.OverlapSphere(agent.transform.position, neighborRadius,
                                                Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (Collider c in hits)
        {
            FlockAgent other = c.GetComponent<FlockAgent>();
            if (other != null && other != agent)
            {
                context.Add(other);
            }
        }
        return context;
    }

    void UpdateLeaders()
    {
        leaderTimer = 0f;
        lastLeaderCount = leaderCount;
        lastAgentCount = agentCount;

        List<List<FlockAgent>> groups = FindGroups();
        groups.Sort((a, b) => b.Count.CompareTo(a.Count));

        int[] quota = new int[groups.Count];
        int remaining = Mathf.Min(leaderCount, agents.Count);

        for (int i = 0; i < groups.Count; i++)
        {
            if (remaining > 0 && groups[i].Count >= minGroupSize && CountLeaders(groups[i]) > 0)
            {
                quota[i] = 1;
                remaining--;
            }
        }
        for (int i = 0; i < groups.Count; i++)
        {
            if (remaining > 0 && groups[i].Count >= minGroupSize && quota[i] == 0)
            {
                quota[i] = 1;
                remaining--;
            }
        }
        for (int i = 0; i < groups.Count; i++)
        {
            int have = CountLeaders(groups[i]);
            while (remaining > 0 && quota[i] > 0 && quota[i] < have && quota[i] < MaxLeaders(groups[i].Count))
            {
                quota[i]++;
                remaining--;
            }
        }
        bool added = true;
        while (remaining > 0 && added)
        {
            added = false;
            for (int i = 0; i < groups.Count; i++)
            {
                if (remaining > 0 && quota[i] > 0 && quota[i] < MaxLeaders(groups[i].Count))
                {
                    quota[i]++;
                    remaining--;
                    added = true;
                }
            }
        }

        for (int i = 0; i < groups.Count; i++)
        {
            ApplyGroupLeaders(groups[i], quota[i]);
        }

        leaders.Clear();
        foreach (FlockAgent a in agents)
        {
            if (a.isLeader) leaders.Add(a);
        }
    }

    int MaxLeaders(int groupSize)
    {
        return Mathf.Max(1, groupSize / minGroupSize);
    }

    int CountLeaders(List<FlockAgent> group)
    {
        int count = 0;
        foreach (FlockAgent a in group)
        {
            if (a.isLeader) count++;
        }
        return count;
    }

    void ApplyGroupLeaders(List<FlockAgent> group, int quota)
    {
        Vector3 center = Vector3.zero;
        Vector3 heading = Vector3.zero;
        foreach (FlockAgent a in group)
        {
            center += a.transform.position;
            heading += a.transform.forward;
        }
        center = center / group.Count;
        float coherence = heading.magnitude / group.Count;
        heading = heading.normalized;

        foreach (FlockAgent a in group)
        {
            Vector3 fromCenter = a.transform.position - center;
            a.front = heading == Vector3.zero ? -fromCenter.magnitude : Vector3.Dot(fromCenter, heading);
        }

        List<FlockAgent> sorted = new List<FlockAgent>(group);
        sorted.Sort((x, y) => y.front.CompareTo(x.front));

        List<FlockAgent> current = sorted.FindAll(a => a.isLeader);

        if (current.Count > quota)
        {
            for (int k = current.Count - 1; k >= quota; k--)
            {
                current[k].SetLeader(false);
            }
        }
        else if (current.Count < quota)
        {
            foreach (FlockAgent a in sorted)
            {
                if (current.Count >= quota) break;
                if (!a.isLeader)
                {
                    Promote(a);
                    current.Add(a);
                }
            }
        }
        else if (quota > 0 && coherence > 0.5f)
        {
            FlockAgent rear = current[current.Count - 1];
            FlockAgent best = null;
            foreach (FlockAgent a in sorted)
            {
                if (!a.isLeader) { best = a; break; }
            }

            foreach (FlockAgent a in group)
            {
                if (!a.isLeader && a != best) a.aheadSince = -1f;
            }

            if (best != null)
            {
                bool leaderIsVeteran = Time.time - rear.leaderSince > leaderMinTime;
                bool isAhead = best.front - rear.front > leaderOvertakeDistance;

                if (leaderIsVeteran && isAhead)
                {
                    if (best.aheadSince < 0f)
                    {
                        best.aheadSince = Time.time;
                    }
                    else if (Time.time - best.aheadSince >= leaderOvertakeTime)
                    {
                        rear.SetLeader(false);
                        Promote(best);
                    }
                }
                else
                {
                    best.aheadSince = -1f;
                }
            }
        }
    }

    void Promote(FlockAgent agent)
    {
        agent.SetLeader(true);
        agent.leaderSince = Time.time;
        agent.aheadSince = -1f;
    }

    List<List<FlockAgent>> FindGroups()
    {
        List<List<FlockAgent>> groups = new List<List<FlockAgent>>();
        HashSet<FlockAgent> visited = new HashSet<FlockAgent>();
        float radiusSqr = neighborRadius * neighborRadius;

        foreach (FlockAgent start in agents)
        {
            if (visited.Contains(start)) continue;

            List<FlockAgent> group = new List<FlockAgent>();
            Stack<FlockAgent> pending = new Stack<FlockAgent>();
            pending.Push(start);
            visited.Add(start);

            while (pending.Count > 0)
            {
                FlockAgent current = pending.Pop();
                group.Add(current);

                foreach (FlockAgent other in agents)
                {
                    if (visited.Contains(other)) continue;
                    float sqr = (other.transform.position - current.transform.position).sqrMagnitude;
                    if (sqr < radiusSqr)
                    {
                        visited.Add(other);
                        pending.Push(other);
                    }
                }
            }
            groups.Add(group);
        }
        return groups;
    }

    float LeaderSlowdown(FlockAgent leader)
    {
        int total = 0;
        int lagging = 0;
        foreach (FlockAgent a in agents)
        {
            if (a == leader || a.isLeader) continue;
            float d = Vector3.Distance(a.transform.position, leader.transform.position);
            if (d > followRadius) continue;
            total++;
            if (d > followDistance * 2f) lagging++;
        }
        if (total == 0) return 1f;
        return 1f - 0.6f * ((float)lagging / total);
    }

    public Vector3 RandomPoint(float radius)
    {
        Vector3 p = transform.position;
        for (int i = 0; i < 10; i++)
        {
            p = transform.position + Random.insideUnitSphere * radius;
            if (!Physics.CheckSphere(p, boidRadius * 3f, obstacleMask)) break;
        }
        return p;
    }

    FlockAgent SpawnAgent()
    {
        GameObject go = new GameObject("Agent");
        go.transform.SetParent(transform);
        go.transform.position = RandomPoint(areaRadius * 0.5f);

        SphereCollider col = go.AddComponent<SphereCollider>();
        col.radius = boidRadius;
        col.isTrigger = true;

        Quaternion flat = Quaternion.Euler(90f, 0f, 0f);
        CreateArrowQuad(go.transform, flat);
        CreateArrowQuad(go.transform, Quaternion.AngleAxis(90f, Vector3.forward) * flat);

        FlockAgent agent = go.AddComponent<FlockAgent>();
        agent.Init(this);
        return agent;
    }

    void CreateArrowQuad(Transform parent, Quaternion rotation)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent, false);
        quad.transform.localRotation = rotation;

        Material mat = quad.GetComponent<Renderer>().material;
        mat.shader = Shader.Find("Sprites/Default");
        mat.mainTexture = texture;
    }

    Texture2D MakeArrowTexture()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (size - 1f);
                float v = y / (size - 1f);
                float halfWidth = 0.4f * (1f - v);
                bool inside = v > 0.05f && Mathf.Abs(u - 0.5f) < halfWidth;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return tex;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}