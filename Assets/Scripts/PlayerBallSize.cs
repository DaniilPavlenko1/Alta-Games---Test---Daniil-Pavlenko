using System;
using UnityEngine;

public class PlayerBallSize : MonoBehaviour
{
    private const float EPS = 0.0001f;

    [Header("Mass")]
    [SerializeField] private float mass = 3f;
    [SerializeField] private float minMass = 0.6f;

    [Header("Visual")]
    [SerializeField] private float baseRadius = 0.5f;

    public float Mass => mass;
    public float MinMass => minMass;

    public float Radius => baseRadius * Mathf.Sqrt(Mathf.Max(EPS, mass));

    public bool IsDead => mass <= minMass + EPS;

    public event Action<float> OnRadiusChanged;
    public event Action OnDeath;

    private float _lastRadius = -1f;

    void Awake()
    {
        ApplyVisual();
    }

    public void SetMass(float newMass)
    {
        float clamped = Mathf.Max(0f, newMass);
        if (Mathf.Abs(clamped - mass) < EPS) return;

        mass = clamped;
        ApplyVisual();

        if (IsDead)
            OnDeath?.Invoke();
    }

    public void AddMass(float delta)
    {
        if (Mathf.Abs(delta) < EPS) return;
        SetMass(mass + delta);
    }

    public void ApplyVisual()
    {
        float radius = Radius;

        if (Mathf.Abs(radius - _lastRadius) < EPS) return;
        _lastRadius = radius;

        transform.localScale = Vector3.one * (radius * 2f);
        OnRadiusChanged?.Invoke(radius);
    }
}