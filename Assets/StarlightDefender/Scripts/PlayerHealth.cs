using System.Collections;
using UnityEngine;

namespace StarlightDefender
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxLife = 3;
        [SerializeField] private float invulnerableDuration = 1.5f;
        private SpriteRenderer spriteRenderer;
        private bool invulnerable;
        public int Life { get; private set; }
        public int MaxLife => maxLife;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Life = maxLife;
        }

        private void Start() => GameManager.Instance?.RegisterPlayer(this);

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || invulnerable || Life <= 0 || GameManager.Instance == null || GameManager.Instance.IsFinished) return;
            Life = Mathf.Max(0, Life - damage);
            AudioManager.Instance?.Play("PlayerHit");
            UIManager.Instance?.RefreshHud();
            if (Life == 0)
            {
                GameManager.Instance.GameOver();
                return;
            }
            StartCoroutine(InvulnerabilityRoutine());
        }

        public void Recover(int amount)
        {
            Life = Mathf.Min(maxLife, Life + Mathf.Max(0, amount));
            UIManager.Instance?.RefreshHud();
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            invulnerable = true;
            float elapsed = 0f;
            while (elapsed < invulnerableDuration)
            {
                elapsed += 0.1f;
                if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
            }
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            invulnerable = false;
        }
    }
}
