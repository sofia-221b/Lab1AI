using UnityEngine;

public class FlockAgent : MonoBehaviour
{
    [HideInInspector] public Flock flock;
    public bool isLeader;
    public Vector3 velocity;

    
    [HideInInspector] public Vector3 desiredMove;   
    [HideInInspector] public float nextCalcTime;    
    [HideInInspector] public Vector3 wanderTarget;  
    [HideInInspector] public float wanderExpire;

    [HideInInspector] public float front;           
    [HideInInspector] public float leaderSince = -100f;  
    [HideInInspector] public float aheadSince = -1f;     

    float speedSeed;      
    Vector3 smoothRate;   
    Renderer[] rends;

    public float SpeedFactor
    {
        get
        {
            float noise = Mathf.PerlinNoise(Time.time * flock.speedChangeRate, speedSeed) - 0.5f;
            return 1f + noise * 2f * flock.speedVariation;
        }
    }

    public void Init(Flock f)
    {
        flock = f;
        rends = GetComponentsInChildren<Renderer>();
        speedSeed = Random.value * 100f;

        Vector3 dir = Random.onUnitSphere;
        velocity = dir * flock.minSpeed;
        transform.rotation = Quaternion.LookRotation(dir);

        nextCalcTime = Time.time + Random.value * flock.updateInterval;
        SetLeader(false);
    }

    public void SetLeader(bool value)
    {
        isLeader = value;
        wanderExpire = 0f;
        foreach (Renderer r in rends)
        {
            r.material.color = value ? flock.leaderColor : flock.followerColor;
        }
        transform.localScale = Vector3.one * (value ? 1.6f : 1f);
    }

    public void Move(Vector3 targetVelocity)
    {
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothRate, flock.smoothTime);
        if (velocity.magnitude < flock.minSpeed)
        {
            velocity = transform.forward * flock.minSpeed;
        }

        Vector3 dir = velocity.normalized;
        float distance = velocity.magnitude * Time.deltaTime;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, flock.boidRadius, dir, out hit,
                               distance + 0.05f, flock.obstacleMask))
        {
            distance = Mathf.Max(0f, hit.distance - 0.05f);           
            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);  
        }

        transform.position += dir * distance;

        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 10f * Time.deltaTime);
        }
    }
}
