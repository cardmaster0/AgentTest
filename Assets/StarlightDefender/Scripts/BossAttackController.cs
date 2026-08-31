using UnityEngine;

namespace StarlightDefender
{
    public sealed class BossAttackController : MonoBehaviour
    {
        [SerializeField] private GameObject enemyBulletPrefab;
        [SerializeField] private float attackInterval = 1.65f;
        [SerializeField] private float enragedMultiplier = 0.72f;
        private BossController boss;
        private float nextAttack;
        private int patternIndex;
        private float movementPhase;
        private bool active;
        public int LastPattern { get; private set; } = -1;

        public void Configure(GameObject bullet) => enemyBulletPrefab = bullet;

        public void BeginCombat(BossController controller)
        {
            boss = controller;
            patternIndex = 0;
            LastPattern = -1;
            nextAttack = Time.time + 0.8f;
            active = true;
        }

        public void StopCombat() => active = false;

        private void Update()
        {
            if (!active || boss == null) return;
            movementPhase += Time.deltaTime;
            if (boss.HealthRatio <= 0.5f && Camera.main != null)
            {
                Vector3 p = transform.position;
                float halfWidth = Camera.main.orthographicSize * Camera.main.aspect - 1.7f;
                p.x = Mathf.Sin(movementPhase * 0.9f) * halfWidth;
                transform.position = p;
            }
            if (Time.time < nextAttack) return;
            int pattern = boss.HealthRatio <= 0.5f ? 2 : patternIndex % 2;
            LastPattern = pattern;
            if (pattern == 0) FireFan();
            else if (pattern == 1) FireAimedBurst();
            else FireRadial();
            patternIndex++;
            float multiplier = boss.HealthRatio <= 0.5f ? enragedMultiplier : 1f;
            nextAttack = Time.time + attackInterval * multiplier;
        }

        private void FireFan()
        {
            for (int i = -3; i <= 3; i++)
            {
                float angle = -90f + i * 13f;
                SpawnBullet(Direction(angle));
            }
        }

        private void FireAimedBurst()
        {
            Vector2 aimed = GameManager.Instance?.Player != null
                ? ((Vector2)GameManager.Instance.Player.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            float baseAngle = Mathf.Atan2(aimed.y, aimed.x) * Mathf.Rad2Deg;
            for (int i = -2; i <= 2; i++) SpawnBullet(Direction(baseAngle + i * 6f));
        }

        private void FireRadial()
        {
            for (int i = 0; i < 14; i++)
            {
                float angle = i * (360f / 14f) + movementPhase * 18f;
                SpawnBullet(Direction(angle));
            }
        }

        private void SpawnBullet(Vector2 direction)
        {
            if (enemyBulletPrefab == null || ObjectPool.Instance == null) return;
            GameObject bullet = ObjectPool.Instance.Spawn(enemyBulletPrefab, transform.position + Vector3.down * 0.65f, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>()?.Launch(direction);
        }

        private static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
