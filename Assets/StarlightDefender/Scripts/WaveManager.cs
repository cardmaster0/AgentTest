using System.Collections;
using UnityEngine;

namespace StarlightDefender
{
    public sealed class WaveManager : MonoBehaviour
    {
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject zigzagPrefab;
        [SerializeField] private GameObject shooterPrefab;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float bossSpawnTime = 90f;
        [SerializeField] private bool debugFastBoss;
        [SerializeField] private float debugBossSpawnTime = 12f;
        [SerializeField] private float baseSpawnInterval = 1.4f;
        private float elapsed;
        private float nextSpawn;
        private int waveIndex;
        private bool bossSequenceStarted;

        public void Configure(GameObject scout, GameObject zigzag, GameObject shooter, GameObject boss)
        {
            scoutPrefab = scout;
            zigzagPrefab = zigzag;
            shooterPrefab = shooter;
            bossPrefab = boss;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.IsFinished || bossSequenceStarted) return;
            elapsed += Time.deltaTime;
            float targetTime = debugFastBoss ? debugBossSpawnTime : bossSpawnTime;
            if (elapsed >= targetTime)
            {
                bossSequenceStarted = true;
                StartCoroutine(BossArrival());
                return;
            }
            if (Time.time < nextSpawn) return;
            SpawnWave();
            float intensity = Mathf.Clamp01(elapsed / 80f);
            nextSpawn = Time.time + Mathf.Lerp(baseSpawnInterval, 0.78f, intensity);
        }

        private void SpawnWave()
        {
            waveIndex++;
            GameObject chosen = scoutPrefab;
            if (elapsed >= 40f && waveIndex % 4 == 0) chosen = shooterPrefab;
            else if (elapsed >= 20f && waveIndex % 3 == 0) chosen = zigzagPrefab;

            int pattern = waveIndex % 4;
            if (pattern == 0) SpawnRow(chosen, Mathf.Min(5, 2 + waveIndex / 12));
            else if (pattern == 1) SpawnV(chosen, 5);
            else if (pattern == 2) SpawnAlternating(chosen, 4);
            else SpawnOne(chosen, Random.Range(0.12f, 0.88f), 1.08f);
        }

        private void SpawnRow(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++) SpawnOne(prefab, (i + 1f) / (count + 1f), 1.05f);
        }

        private void SpawnV(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float centerDistance = Mathf.Abs(i - (count - 1) * 0.5f);
                SpawnOne(prefab, Mathf.Lerp(0.18f, 0.82f, i / (float)(count - 1)), 1.02f + centerDistance * 0.07f);
            }
        }

        private void SpawnAlternating(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++) SpawnOne(prefab, i % 2 == 0 ? 0.22f : 0.78f, 1.04f + i * 0.1f);
        }

        private static void SpawnOne(GameObject prefab, float viewportX, float viewportY)
        {
            if (prefab == null || Camera.main == null || ObjectPool.Instance == null) return;
            Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(viewportX, viewportY, 10f));
            position.z = 0f;
            ObjectPool.Instance.Spawn(prefab, position, Quaternion.identity);
        }

        private IEnumerator BossArrival()
        {
            UIManager.Instance?.ShowWarning("WARNING\nBOSS APPROACHING", 2.8f);
            AudioManager.Instance?.Play("BossWarning");
            yield return new WaitForSeconds(3f);
            if (bossPrefab != null && Camera.main != null && ObjectPool.Instance != null)
            {
                Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.22f, 10f));
                position.z = 0f;
                ObjectPool.Instance.Spawn(bossPrefab, position, Quaternion.identity);
            }
        }

        public void StopWavesForAutomatedTest() => bossSequenceStarted = true;
    }
}
