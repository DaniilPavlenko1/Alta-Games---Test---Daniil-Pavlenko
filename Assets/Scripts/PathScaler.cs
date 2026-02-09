using UnityEngine;

public class PathScaler : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private PlayerBallSize player;

    [Header("Path Settings")]
    [SerializeField] private float widthPerRadius = 2f;
    [SerializeField] private float minWidth = 0.6f;
    [SerializeField] private float maxWidth = 3.0f;

    private Vector3 _baseScale;

    void Awake()
    {
        _baseScale = transform.localScale;
    }

    void OnEnable()
    {
        if (!player) return;

        player.OnRadiusChanged += HandleRadiusChanged;
        HandleRadiusChanged(player.Radius);
    }

    void OnDisable()
    {
        if (!player) return;
        player.OnRadiusChanged -= HandleRadiusChanged;
    }

    private void HandleRadiusChanged(float radius)
    {
        float width = Mathf.Clamp(radius * widthPerRadius, minWidth, maxWidth);

        var scale = _baseScale;
        scale.x = width;
        transform.localScale = scale;
    }
}