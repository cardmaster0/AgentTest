using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightDefender
{
    public sealed class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [Min(1)] [SerializeField] private int attackPower = 1;
        [SerializeField] private float fireInterval = 0.18f;
        [SerializeField] private float rapidMultiplier = 0.45f;
        private float nextFireTime;
        private PowerUpManager powerUps;

        private void Awake() => powerUps = GetComponent<PowerUpManager>();

        public void Configure(GameObject bullet, GameObject flash)
        {
            bulletPrefab = bullet;
            muzzleFlashPrefab = flash;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || GameManager.Instance == null || GameManager.Instance.IsFinished) return;
            bool firing = keyboard.spaceKey.isPressed || keyboard.zKey.isPressed;
            float interval = powerUps != null && powerUps.RapidActive ? fireInterval * rapidMultiplier : fireInterval;
            if (firing && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + interval;
            }
        }

        private void Fire()
        {
            if (bulletPrefab == null || ObjectPool.Instance == null) return;
            Vector3 origin = transform.position + Vector3.up * 0.68f;
            if (powerUps != null && powerUps.SpreadActive)
            {
                SpawnBullet(origin, 0f);
                SpawnBullet(origin + Vector3.left * 0.18f, 18f);
                SpawnBullet(origin + Vector3.right * 0.18f, -18f);
            }
            else SpawnBullet(origin, 0f);
            if (muzzleFlashPrefab != null)
            {
                GameObject flash = ObjectPool.Instance.Spawn(muzzleFlashPrefab, origin, Quaternion.identity);
                flash.GetComponent<ExplosionEffect>()?.SetScale(0.32f);
            }
            AudioManager.Instance?.Play("PlayerShot", 0.55f);
        }

        public void FireForAutomatedTest() => Fire();

        private void SpawnBullet(Vector3 origin, float zAngle)
        {
            GameObject bullet = ObjectPool.Instance.Spawn(bulletPrefab, origin, Quaternion.Euler(0f, 0f, zAngle));
            bullet.GetComponent<PlayerBullet>()?.SetDamage(attackPower);
        }
    }
}
