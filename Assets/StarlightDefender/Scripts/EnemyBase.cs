using UnityEngine;

namespace StarlightDefender
{
    public abstract class EnemyBase : MonoBehaviour, IPoolable
    {
        [Header("Enemy Stats")]
        [SerializeField] protected int maxHp = 2;
        [SerializeField] protected float moveSpeed = 2.2f;
        [SerializeField] protected int defeatScore = 100;
        [SerializeField] protected int contactDamage = 1;
        [SerializeField] protected float shotInterval = 1.5f;
        [Range(0f, 1f)] [SerializeField] protected float dropChance = 0.12f;
        [SerializeField] private GameObject explosionPrefab;
        protected int hp;
        protected Camera MainCamera;

        public void ConfigureStats(int health, float speed, int score, int contact, float fireInterval, float drop, GameObject explosion)
        {
            maxHp = health;
            moveSpeed = speed;
            defeatScore = score;
            contactDamage = contact;
            shotInterval = fireInterval;
            dropChance = drop;
            explosionPrefab = explosion;
        }

        protected virtual void Awake() => MainCamera = Camera.main;

        protected virtual void Update()
        {
            MoveEnemy();
            if (MainCamera != null && transform.position.y < MainCamera.ViewportToWorldPoint(Vector3.zero).y - 1.5f)
                ObjectPool.Instance?.Despawn(gameObject);
        }

        protected abstract void MoveEnemy();

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || hp <= 0) return;
            hp -= amount;
            if (hp <= 0) Die();
        }

        protected virtual void Die()
        {
            ScoreManager.Instance?.Add(defeatScore);
            GameBootstrap.Instance?.SpawnExplosion(transform.position, false);
            GameBootstrap.Instance?.TrySpawnPowerUp(transform.position, dropChance);
            AudioManager.Instance?.Play("EnemyExplosion", 0.75f);
            ObjectPool.Instance?.Despawn(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerHealth player)) return;
            player.TakeDamage(contactDamage);
            ObjectPool.Instance?.Despawn(gameObject);
        }

        public virtual void OnPoolSpawned()
        {
            hp = maxHp;
            MainCamera = Camera.main;
        }

        public virtual void OnPoolDespawned() { }
    }
}
