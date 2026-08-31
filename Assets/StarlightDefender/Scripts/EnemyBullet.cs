using UnityEngine;

namespace StarlightDefender
{
    public sealed class EnemyBullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 4.6f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 7f;
        private Vector2 direction = Vector2.down;
        private float remaining;

        public void Launch(Vector2 newDirection)
        {
            direction = newDirection.sqrMagnitude > 0.01f ? newDirection.normalized : Vector2.down;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
            remaining -= Time.deltaTime;
            if (remaining <= 0f) ObjectPool.Instance?.Despawn(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerHealth player)) return;
            player.TakeDamage(damage);
            ObjectPool.Instance?.Despawn(gameObject);
        }

        public void OnPoolSpawned()
        {
            remaining = lifetime;
            direction = Vector2.down;
        }
        public void OnPoolDespawned() { }
    }
}
