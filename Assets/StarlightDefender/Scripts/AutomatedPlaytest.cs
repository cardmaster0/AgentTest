using System.Collections;
using UnityEngine;

namespace StarlightDefender
{
    public sealed class AutomatedPlaytest : MonoBehaviour
    {
        public const string TestKey = "StarlightDefender.InternalAutomatedPlaytestStage";
        public const string PreviousHighScoreKey = "StarlightDefender.InternalPreviousHighScore";
        public const string FailureKey = "StarlightDefender.InternalAutomatedPlaytestFailed";

        private void Start()
        {
            int stage = PlayerPrefs.GetInt(TestKey, 0);
            if (stage > 0) StartCoroutine(Run(stage));
        }

        private IEnumerator Run(int stage)
        {
            yield return new WaitForSecondsRealtime(0.8f);
            if (stage == 1) yield return RunGameplayChecks();
            else if (stage == 2) yield return RunGameOverAndRetryCheck();
            else if (stage == 3) yield return RunTitleRouteCheck();
        }

        private IEnumerator RunGameplayChecks()
        {
            PlayerHealth player = GameManager.Instance?.Player;
            Check(player != null, "player spawned with health component");
            Check(UIManager.Instance != null && ScoreManager.Instance != null && ObjectPool.Instance != null, "core managers and UI initialized");
            if (player == null) yield break;

            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            Vector2 originalPosition = playerBody.position;
            playerBody.position = new Vector2(999f, 999f);
            player.GetComponent<PlayerController>().ClampImmediatelyForAutomatedTest();
            Vector3 clampedViewport = Camera.main.WorldToViewportPoint(playerBody.position);
            Check(clampedViewport.x <= 0.96f && clampedViewport.y <= 0.94f, "player movement remains inside camera bounds");
            playerBody.position = originalPosition;
            player.transform.position = originalPosition;
            Physics2D.SyncTransforms();

            PlayerShooter shooter = player.GetComponent<PlayerShooter>();
            shooter.FireForAutomatedTest();
            yield return new WaitForSecondsRealtime(0.12f);
            Check(FindObjectsByType<PlayerBullet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0, "player firing creates a pooled bullet");

            Vector3 target = player.transform.position + Vector3.up * 2.2f;
            EnemyBase scout = GameBootstrap.Instance.SpawnEnemyForAutomatedTest(0, target);
            shooter.FireForAutomatedTest();
            yield return new WaitForSecondsRealtime(0.08f);
            shooter.FireForAutomatedTest();
            yield return new WaitForSecondsRealtime(0.45f);
            Check(ScoreManager.Instance.Score >= 100 || scout == null || !scout.gameObject.activeSelf, "player bullet damages and defeats Scout, adding score");

            Vector3 topLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.2f, 0.9f, 10f));
            Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(0.8f, 0.9f, 10f));
            topLeft.z = topRight.z = 0f;
            GameBootstrap.Instance.SpawnEnemyForAutomatedTest(1, topLeft);
            GameBootstrap.Instance.SpawnEnemyForAutomatedTest(2, topRight);
            Check(FindObjectsByType<ZigzagEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0 &&
                  FindObjectsByType<ShooterEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0, "Zigzag and Shooter enemy types spawn");

            PowerUpManager powers = player.GetComponent<PowerUpManager>();
            powers.Apply(PowerUpType.Rapid, 2f);
            powers.Apply(PowerUpType.Spread, 2f);
            Check(powers.RapidActive && powers.SpreadActive, "RAPID and SPREAD power-up timers activate and stack");
            int beforeDamage = player.Life;
            player.TakeDamage(1);
            int afterFirstHit = player.Life;
            player.TakeDamage(1);
            Check(afterFirstHit == beforeDamage - 1 && player.Life == afterFirstHit, "damage and 1.5 second invulnerability prevent duplicate hits");
            powers.Apply(PowerUpType.Recover, 0f);
            Check(player.Life == beforeDamage, "RECOVER restores one life without exceeding maximum");

            GameManager.Instance.TogglePause();
            Check(GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "ESC pause state stops game time");
            GameManager.Instance.TogglePause();
            Check(!GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 1f), "pause resumes game time");

            GetComponent<WaveManager>()?.StopWavesForAutomatedTest();
            BossController boss = GameBootstrap.Instance.SpawnBossForAutomatedTest();
            yield return new WaitForSecondsRealtime(0.25f);
            Check(boss != null && boss.IsFighting, "boss enters and starts combat with HP UI");
            yield return new WaitForSecondsRealtime(2.1f);
            BossAttackController bossAttacks = boss?.GetComponent<BossAttackController>();
            bool patternOne = bossAttacks != null && bossAttacks.LastPattern == 0 &&
                              FindObjectsByType<EnemyBullet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0;
            yield return new WaitForSecondsRealtime(0.45f);
            bool patternTwo = bossAttacks != null && bossAttacks.LastPattern == 1;
            boss?.TakeDamage(90);
            yield return new WaitForSecondsRealtime(1.55f);
            bool patternThree = bossAttacks != null && bossAttacks.LastPattern == 2;
            Check(patternOne && patternTwo && patternThree, "boss switches attack patterns 1, 2 and the HP-50% pattern 3");
            boss?.TakeDamage(9999);
            Check(boss != null && boss.HealthRatio == 0f && UIManager.Instance.DisplayedBossRatio == 0f,
                "lethal high attack power clamps boss HP and its gauge to zero");
            yield return new WaitForSecondsRealtime(2.5f);
            Check(GameManager.Instance.IsFinished && ScoreManager.Instance.Score >= 10000, "boss defeat awards bonus and opens MISSION COMPLETE result");
            Check(ScoreManager.Instance.HighScore >= ScoreManager.Instance.Score, "high score persisted to PlayerPrefs");

            Time.timeScale = 1f;
            PlayerPrefs.SetInt(TestKey, 2);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameManager.GameScene);
        }

        private IEnumerator RunGameOverAndRetryCheck()
        {
            Check(GameManager.Instance?.Player != null, "retry stage loaded a fresh player");
            GameManager.Instance.GameOver();
            yield return new WaitForSecondsRealtime(0.15f);
            Check(GameManager.Instance.IsFinished, "GAME OVER result state opens");
            Time.timeScale = 1f;
            PlayerPrefs.SetInt(TestKey, 3);
            PlayerPrefs.Save();
            GameManager.Instance.Retry();
        }

        private IEnumerator RunTitleRouteCheck()
        {
            Check(GameManager.Instance?.Player != null, "RETRY route reloaded Game");
            PlayerPrefs.SetInt(TestKey, 4);
            PlayerPrefs.Save();
            GameManager.Instance.ReturnToTitle();
            yield break;
        }

        private static void Check(bool condition, string message)
        {
            if (condition) Debug.Log("[SD TEST] PASS: " + message);
            else
            {
                PlayerPrefs.SetInt(FailureKey, 1);
                PlayerPrefs.Save();
                Debug.LogError("[SD TEST] FAIL: " + message);
            }
        }
    }
}
