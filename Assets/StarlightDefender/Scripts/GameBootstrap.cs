using UnityEngine;

namespace StarlightDefender
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject zigzagPrefab;
        [SerializeField] private GameObject shooterPrefab;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private GameObject rapidPrefab;
        [SerializeField] private GameObject spreadPrefab;
        [SerializeField] private GameObject recoverPrefab;
        [SerializeField] private GameObject explosionPrefab;

        public void Configure(GameObject player, GameObject scout, GameObject zigzag, GameObject shooter, GameObject boss,
            GameObject rapid, GameObject spread, GameObject recover, GameObject explosion)
        {
            playerPrefab = player;
            scoutPrefab = scout;
            zigzagPrefab = zigzag;
            shooterPrefab = shooter;
            bossPrefab = boss;
            rapidPrefab = rapid;
            spreadPrefab = spread;
            recoverPrefab = recover;
            explosionPrefab = explosion;
        }

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            if (playerPrefab == null || Camera.main == null) return;
            Vector3 spawn = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.16f, 10f));
            spawn.z = 0f;
            Instantiate(playerPrefab, spawn, Quaternion.identity);
            WaveManager waves = GetComponent<WaveManager>();
            waves?.Configure(scoutPrefab, zigzagPrefab, shooterPrefab, bossPrefab);
            UIManager.Instance?.RefreshHud();
        }

        public void TrySpawnPowerUp(Vector3 position, float chance)
        {
            if (Random.value > chance || ObjectPool.Instance == null) return;
            int choice = Random.Range(0, 3);
            GameObject prefab = choice == 0 ? rapidPrefab : choice == 1 ? spreadPrefab : recoverPrefab;
            if (prefab != null) ObjectPool.Instance.Spawn(prefab, position, Quaternion.identity);
        }

        public void SpawnExplosion(Vector3 position, bool pickup, float scale = 1f)
        {
            if (explosionPrefab == null || ObjectPool.Instance == null) return;
            GameObject effect = ObjectPool.Instance.Spawn(explosionPrefab, position, Quaternion.identity);
            effect.GetComponent<ExplosionEffect>()?.SetScale(scale * (pickup ? 0.55f : 1f));
            SpriteRenderer renderer = effect.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = pickup ? new Color(0.35f, 1f, 0.85f, 1f) : Color.white;
        }

        public EnemyBase SpawnEnemyForAutomatedTest(int kind, Vector3 position)
        {
            GameObject prefab = kind == 0 ? scoutPrefab : kind == 1 ? zigzagPrefab : shooterPrefab;
            return ObjectPool.Instance?.Spawn(prefab, position, Quaternion.identity)?.GetComponent<EnemyBase>();
        }

        public BossController SpawnBossForAutomatedTest()
        {
            if (bossPrefab == null || Camera.main == null || ObjectPool.Instance == null) return null;
            Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.78f, 10f));
            position.z = 0f;
            return ObjectPool.Instance.Spawn(bossPrefab, position, Quaternion.identity)?.GetComponent<BossController>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
