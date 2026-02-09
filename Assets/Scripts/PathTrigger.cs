using System.Collections.Generic;
using UnityEngine;

public class PathTrigger : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private PlayerJumpMover player;

    private readonly HashSet<Obstacle> inside = new HashSet<Obstacle>();

    public bool IsEmpty => inside.Count == 0;

    private void OnTriggerEnter(Collider other)
    {
        var o = other.GetComponentInParent<Obstacle>();
        if (!o) return;

        inside.Add(o);
    }

    private void OnTriggerExit(Collider other)
    {
        var o = other.GetComponentInParent<Obstacle>();
        if (!o) return;

        inside.Remove(o);
        CheckEmpty();
    }

    private void Update()
    {
        if (inside.Count == 0) return;

        bool removed = inside.RemoveWhere(o => o == null || !o.gameObject.activeInHierarchy) > 0;
        if (removed) CheckEmpty();
    }

    private void CheckEmpty()
    {
        if (GameFlow.I && !GameFlow.I.IsPlaying) return;

        if (inside.Count == 0)
            player.StartMove();
    }
}