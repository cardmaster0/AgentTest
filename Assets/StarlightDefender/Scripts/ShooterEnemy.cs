using UnityEngine;

namespace StarlightDefender
{
    public sealed class ShooterEnemy : EnemyBase
    {
        [SerializeField] private GameObject enemyBulletPrefab;
        [SerializeField] private float stopViewportY = 0.72f;
        [SerializeField] private float stopDuration = 2.6f;
        private float stoppedTime;
        private float nextShot;
        private bool descending;

        public void ConfigureShooter(GameObject bullet) => enemyBulletPrefab = bullet;

        protected override void MoveEnemy()
        {
            if (MainCamera == null) return;
            float stopY = MainCamera.ViewportToWorldPoint(new Vector3(0.5f, stopViewportY)).y;
            if (!descending && transform.position.y > stopY)
            {
                transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime), Space.World);
                return;
            }
            if (!descending)
            {
                stoppedTime += Time.deltaTime;
                if (Time.time >= nextShot)
                {
                    FireAtPlayer();
                    nextShot = Time.time + shotInterval;
                }
                if (stoppedTime >= stopDuration) descending = true;
                return;
            }
            transform.Translate(Vector3.down * (moveSpeed * 1.25f * Time.deltaTime), Space.World);
        }

        private void FireAtPlayer()
        {
            if (enemyBulletPrefab == null || ObjectPool.Instance == null || GameManager.Instance?.Player == null) return;
            Vector2 direction = ((Vector2)GameManager.Instance.Player.transform.position - (Vector2)transform.position).normalized;
            GameObject bullet = ObjectPool.Instance.Spawn(enemyBulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>()?.Launch(direction);
        }

        public override void OnPoolSpawned()
        {
            base.OnPoolSpawned();
            stoppedTime = 0f;
            nextShot = Time.time + 0.4f;
            descending = false;
        }
    }
}
