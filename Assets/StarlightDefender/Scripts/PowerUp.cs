using UnityEngine;

namespace StarlightDefender
{
    public enum PowerUpType { Rapid, Spread, Recover }

    public sealed class PowerUp : MonoBehaviour, IPoolable
    {
        [SerializeField] private PowerUpType type;
        [SerializeField] private float duration = 10f;
        [SerializeField] private float fallSpeed = 1.7f;
        private float phase;
        private Vector3 baseScale;

        public void Configure(PowerUpType powerUpType)
        {
            type = powerUpType;
        }

        private void Awake() => baseScale = transform.localScale;

        private void Update()
        {
            transform.Translate(Vector3.down * (fallSpeed * Time.deltaTime), Space.World);
            phase += Time.deltaTime * 5f;
            transform.localScale = baseScale * (1f + Mathf.Sin(phase) * 0.08f);
            if (Camera.main != null && transform.position.y < Camera.main.ViewportToWorldPoint(Vector3.zero).y - 1f)
                ObjectPool.Instance?.Despawn(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PowerUpManager manager)) return;
            manager.Apply(type, duration);
            GameBootstrap.Instance?.SpawnExplosion(transform.position, true);
            ObjectPool.Instance?.Despawn(gameObject);
        }

        public void OnPoolSpawned()
        {
            phase = 0f;
            if (baseScale == Vector3.zero) baseScale = transform.localScale;
            transform.localScale = baseScale;
        }
        public void OnPoolDespawned() { }
    }
}
