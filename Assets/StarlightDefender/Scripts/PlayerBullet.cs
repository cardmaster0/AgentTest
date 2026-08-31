using UnityEngine;

namespace StarlightDefender
{
    public sealed class PlayerBullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 3f;
        private float remaining;

        public void SetDamage(int value) => damage = Mathf.Max(1, value);

        private void Update()
        {
            transform.Translate(Vector3.up * (speed * Time.deltaTime), Space.Self);
            remaining -= Time.deltaTime;
            if (remaining <= 0f) ObjectPool.Instance?.Despawn(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out BossController boss))
            {
                boss.TakeDamage(damage);
                ObjectPool.Instance?.Despawn(gameObject);
            }
            else if (other.TryGetComponent(out EnemyBase enemy))
            {
                enemy.TakeDamage(damage);
                ObjectPool.Instance?.Despawn(gameObject);
            }
        }

        public void OnPoolSpawned() => remaining = lifetime;
        public void OnPoolDespawned() { }
    }
}
