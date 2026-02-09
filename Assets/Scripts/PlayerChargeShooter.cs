using System.Collections;
using UnityEngine;

public class PlayerChargeShooter : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private PlayerBallSize playerSize;
    [SerializeField] private Transform goalPoint;
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private PathTrigger pathTrigger;

    [Header("Charge (mass transfer)")]
    [SerializeField] private float chargeSpeed = 1.2f;
    [SerializeField] private float minPlayerMass = 0.6f;

    [Header("Shoot")]
    [SerializeField] private float shotSpeed = 18f;
    [SerializeField] private float shotSpawnOffset = 0.8f;

    [Header("Last shot")]
    [SerializeField] private float lastShotTimeout = 3f;

    private const float EPS = 0.0001f;

    private bool _charging;
    private bool _inputEnabled = true;

    private ShotProjectile _currentShot;
    private float _shotMass;

    private Collider _playerCol;
    private PlayerJumpMover _mover;

    private bool _waitingLastShotResult;
    private Coroutine _lastShotTimeoutRoutine;

    void Awake()
    {
        if (!playerSize) playerSize = GetComponent<PlayerBallSize>();
        _playerCol = GetComponent<Collider>();
        _mover = GetComponent<PlayerJumpMover>();
    }

    void Update()
    {
        if (!_inputEnabled) return;

        if (Input.GetMouseButtonDown(0)) StartCharge();
        if (_charging && Input.GetMouseButton(0)) ChargeTick(Time.deltaTime);
        if (_charging && Input.GetMouseButtonUp(0)) ReleaseShot();
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (!enabled) CancelCharge();
    }

    private void CancelCharge()
    {
        _charging = false;
        _shotMass = 0f;

        if (_currentShot != null)
        {
            Destroy(_currentShot.gameObject);
            _currentShot = null;
        }
    }

    private void StartCharge()
    {
        if (_charging) return;
        if (_waitingLastShotResult) return;
        if (_mover != null && _mover.IsMoving) return;

        if (!shotPrefab || !goalPoint || !playerSize) return;
        if (playerSize.Mass <= minPlayerMass + EPS) return;

        _charging = true;
        _shotMass = 0f;

        Vector3 dir = (goalPoint.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + dir * shotSpawnOffset;

        GameObject go = Instantiate(shotPrefab, spawnPos, Quaternion.identity);

        if (!go.TryGetComponent(out _currentShot))
        {
            Debug.LogError("ShotPrefab has no ShotProjectile component!");
            Destroy(go);
            _charging = false;
            return;
        }

        _currentShot.OnDetonated += HandleShotDetonated;

        var shotCol = go.GetComponent<Collider>();
        if (_playerCol && shotCol) Physics.IgnoreCollision(_playerCol, shotCol, true);

        _currentShot.SetKinematic(true);
        _currentShot.SetMass(EPS);
        _currentShot.FollowOwner(transform.position, goalPoint.position, shotSpawnOffset);
    }

    private void ChargeTick(float dt)
    {
        if (_currentShot == null) { CancelCharge(); return; }

        float canTake = Mathf.Max(0f, playerSize.Mass - minPlayerMass);
        float add = Mathf.Min(chargeSpeed * dt, canTake);

        if (add <= 0f)
        {
            ReleaseShot();
            return;
        }

        _shotMass += add;
        playerSize.AddMass(-add);

        _currentShot.SetMass(_shotMass);
        _currentShot.FollowOwner(transform.position, goalPoint.position, shotSpawnOffset);

        if (playerSize.Mass <= minPlayerMass + EPS)
        {
            ReleaseShot();
        }
    }

    private void ReleaseShot()
    {
        if (_currentShot == null) { _charging = false; return; }

        _charging = false;

        Vector3 dir = (goalPoint.position - _currentShot.transform.position).normalized;
        _currentShot.Launch(dir, shotSpeed);

        if (playerSize.Mass <= minPlayerMass + EPS)
        {
            BeginWaitingLastShot();
        }

        _currentShot = null;
        _shotMass = 0f;
    }

    private void BeginWaitingLastShot()
    {
        _waitingLastShotResult = true;
        _inputEnabled = false;

        if (_lastShotTimeoutRoutine != null) StopCoroutine(_lastShotTimeoutRoutine);
        _lastShotTimeoutRoutine = StartCoroutine(LastShotTimeoutRoutine(lastShotTimeout));
    }

    private IEnumerator WaitPathClearThenDecide(float maxWait)
    {
        float t = 0f;

        while (t < maxWait)
        {
            if (pathTrigger != null && pathTrigger.IsEmpty)
            {
                _waitingLastShotResult = false;
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        _waitingLastShotResult = false;
        if (pathTrigger != null && !pathTrigger.IsEmpty)
            GameFlow.I?.Lose();
    }

    private IEnumerator LastShotTimeoutRoutine(float t)
    {
        yield return new WaitForSeconds(t);

        if (!_waitingLastShotResult) yield break;

        _waitingLastShotResult = false;

        if (pathTrigger != null && pathTrigger.IsEmpty) yield break;

        GameFlow.I?.Lose();
    }

    private void HandleShotDetonated()
    {
        if (!_waitingLastShotResult) return;

        if (_lastShotTimeoutRoutine != null)
        {
            StopCoroutine(_lastShotTimeoutRoutine);
            _lastShotTimeoutRoutine = null;
        }
        StartCoroutine(WaitPathClearThenDecide(lastShotTimeout));
    }
}