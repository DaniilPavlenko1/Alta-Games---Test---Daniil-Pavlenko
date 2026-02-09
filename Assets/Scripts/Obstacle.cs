using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float shakeTime = 0.6f;

    private bool _armed;
    private WaitForSeconds _shakeWait;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        _shakeWait = new WaitForSeconds(shakeTime);

        if (animator) animator.enabled = false;
    }

    public void ArmExplodeDelayed(float delay)
    {
        if (_armed) return;
        _armed = true;

        StartCoroutine(ExplodeAfter(delay));
    }

    private IEnumerator ExplodeAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (animator) animator.enabled = true;

        yield return _shakeWait;

        ExplosionFXService.I?.PlayExplosion(transform.position);
        gameObject.SetActive(false);
    }
}