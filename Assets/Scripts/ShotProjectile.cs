using System;
using UnityEngine;

public class ShotProjectile : MonoBehaviour
{
    private const float EPS = 0.0001f;

    [Header("Visual (must match PlayerBallSize.baseRadius)")]
    [SerializeField] private float visualBaseRadius = 0.5f;

    [Header("Infection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float baseInfectRadius = 0.4f;
    [SerializeField] private float radiusPerShotRadius = 1.5f;
    [SerializeField] private float maxInfectRadius = 2.5f;
    [SerializeField] private float waveDelayPerMeter = 0.05f;

    [Header("Life")]
    [SerializeField] private float destroyDelayAfterHit = 0.05f;

    public Action OnDetonated;

    private Rigidbody _rb;
    private SphereCollider _col;

    private float _mass = EPS;
    private bool _triggered;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<SphereCollider>();
    }

    public void SetKinematic(bool k)
    {
        if (_rb) _rb.isKinematic = k;
    }

    public void SetMass(float m)
    {
        _mass = Mathf.Max(EPS, m);
        float radius = visualBaseRadius * Mathf.Sqrt(_mass);

        transform.localScale = Vector3.one * (radius * 2f);
        if (_col) _col.radius = 0.5f;
    }

    public void FollowOwner(Vector3 playerPos, Vector3 goalPos, float offset)
    {
        Vector3 dir = (goalPos - playerPos).normalized;
        transform.position = playerPos + dir * offset;
    }

    public void Launch(Vector3 dir, float speed)
    {
        SetKinematic(false);

        if (_rb)
        {
            _rb.linearVelocity = dir * speed;
            _rb.angularVelocity = Vector3.zero;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_triggered) return;

        if (!collision.collider.TryGetComponent(out Obstacle obs))
        {
            obs = collision.collider.GetComponentInParent<Obstacle>();
            if (!obs) return;
        }

        _triggered = true;

        Vector3 center = obs.transform.position;

        float shotRadiusWorld = transform.lossyScale.x * 0.5f;

        float radius = baseInfectRadius + shotRadiusWorld * radiusPerShotRadius;
        radius = Mathf.Min(radius, maxInfectRadius);

        Infect(center, radius);

        OnDetonated?.Invoke();
        OnDetonated = null;

        Destroy(gameObject, destroyDelayAfterHit);
    }

    void Infect(Vector3 center, float radius)
    {
        var hits = Physics.OverlapSphere(center, radius, obstacleLayer);

        foreach (var h in hits)
        {
            var o = h.GetComponentInParent<Obstacle>();
            if (!o) continue;

            float d = Vector3.Distance(center, o.transform.position);
            float delay = d * waveDelayPerMeter;

            o.ArmExplodeDelayed(delay);
        }
    }
}