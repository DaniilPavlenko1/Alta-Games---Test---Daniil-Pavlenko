using System.Collections;
using UnityEngine;

public class PlayerJumpMover : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Transform door;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerBallSize ballSize;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRayLength = 10f;

    [Header("Jump Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpHeight = 0.8f;
    [SerializeField] private float jumpFrequency = 2.5f;

    [Header("Door")]
    [SerializeField] private float doorTriggerDistance = 5f;

    private bool _moving;
    public bool IsMoving => _moving;

    private float _baseY;
    private float _time;
    private bool _doorTriggered;
    private float _doorDistSqr;

    void Awake()
    {
        if (!ballSize) ballSize = GetComponent<PlayerBallSize>();
        if (!anim && door) anim = door.GetComponent<Animator>();

        _doorDistSqr = doorTriggerDistance * doorTriggerDistance;
    }

    void Start()
    {
        _baseY = GetBaseY();
    }

    void Update()
    {
        if (!_moving) return;

        _time += Time.deltaTime;

        var pos = transform.position;
        pos.z += moveSpeed * Time.deltaTime;

        float jump = Mathf.Abs(Mathf.Sin(_time * jumpFrequency)) * jumpHeight;
        pos.y = _baseY + jump;

        transform.position = pos;

        if (!_doorTriggered && door)
        {
            Vector3 d = transform.position - door.position;
            if (d.sqrMagnitude <= _doorDistSqr)
            {
                _doorTriggered = true;
                if (anim) anim.SetTrigger("Open");
                StartCoroutine(StopAfterAnimation(anim));
            }
        }
    }

    public void StartMove()
    {
        if (_moving) return;

        _baseY = GetBaseY();
        _time = 0f;
        _moving = true;
    }

    private float GetBaseY()
    {
        float radius = ballSize ? ballSize.Radius : 0f;

        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out var hit, groundRayLength, groundMask))
        {
            return hit.point.y + radius;
        }

        return transform.position.y;
    }

    IEnumerator StopAfterAnimation(Animator animator)
    {
        if (!animator)
        {
            StopMove();
            GameFlow.I?.Win();
            yield break;
        }

        yield return null;

        while (animator.IsInTransition(0)) yield return null;

        var st = animator.GetCurrentAnimatorStateInfo(0);
        while (st.normalizedTime < 1f)
        {
            st = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        StopMove();
        GameFlow.I?.Win();
    }

    public void StopMove()
    {
        _moving = false;

        _baseY = GetBaseY();

        var pos = transform.position;
        pos.y = _baseY;
        transform.position = pos;

        _doorTriggered = false;
    }
}