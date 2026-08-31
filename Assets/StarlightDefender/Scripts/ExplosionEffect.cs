using UnityEngine;

namespace StarlightDefender
{
    public sealed class ExplosionEffect : MonoBehaviour, IPoolable
    {
        [SerializeField] private float duration = 0.42f;
        private float elapsed;
        private SpriteRenderer spriteRenderer;
        private Vector3 spawnScale;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spawnScale = transform.localScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = spawnScale * Mathf.Lerp(0.3f, 1.2f, t);
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }
            if (elapsed >= duration) ObjectPool.Instance?.Despawn(gameObject);
        }

        public void SetScale(float scale)
        {
            spawnScale = Vector3.one * scale;
            transform.localScale = spawnScale;
        }

        public void OnPoolSpawned()
        {
            elapsed = 0f;
            spawnScale = transform.localScale;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }

        public void OnPoolDespawned() { }
    }
}
