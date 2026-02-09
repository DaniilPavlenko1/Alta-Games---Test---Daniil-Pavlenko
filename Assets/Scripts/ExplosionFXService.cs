using UnityEngine;

public class ExplosionFXService : MonoBehaviour
{
    public static ExplosionFXService I { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private ParticleSystem explosionVfxPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.9f;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    public void PlayExplosion(Vector3 position)
    {
        if (explosionVfxPrefab)
        {
            var vfx = Instantiate(explosionVfxPrefab, position, Quaternion.identity);
            vfx.Play();

            float life = 0.5f;
            Destroy(vfx.gameObject, Mathf.Max(0.2f, life));
        }

        if (explosionClip)
        {
            AudioSource.PlayClipAtPoint(explosionClip, position, volume);
        }
    }
}
