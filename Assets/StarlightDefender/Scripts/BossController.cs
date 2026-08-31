using System.Collections;
using UnityEngine;

namespace StarlightDefender
{
    public sealed class BossController : MonoBehaviour, IPoolable
    {
        [SerializeField] private int maxHp = 180;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private int defeatBonus = 10000;
        [SerializeField] private float entrySpeed = 1.4f;
        [SerializeField] private float targetViewportY = 0.78f;
        private int hp;
        private bool fighting;
        private bool defeated;
        private BossAttackController attacks;
        public float HealthRatio => maxHp <= 0 ? 0f : Mathf.Clamp01((float)hp / maxHp);
        public bool IsFighting => fighting;

        private void Awake() => attacks = GetComponent<BossAttackController>();

        private void Update()
        {
            if (defeated || fighting || Camera.main == null) return;
            float targetY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, targetViewportY)).y;
            transform.Translate(Vector3.down * (entrySpeed * Time.deltaTime), Space.World);
            if (transform.position.y <= targetY)
            {
                Vector3 p = transform.position;
                p.y = targetY;
                transform.position = p;
                fighting = true;
                attacks?.BeginCombat(this);
                UIManager.Instance?.ShowBoss(this);
            }
        }

        public void TakeDamage(int amount)
        {
            if (!fighting || defeated || amount <= 0) return;
            hp = Mathf.Max(0, hp - amount);
            UIManager.Instance?.RefreshBoss(HealthRatio);
            if (hp <= 0) StartCoroutine(DefeatRoutine());
        }

        private IEnumerator DefeatRoutine()
        {
            defeated = true;
            fighting = false;
            hp = 0;
            UIManager.Instance?.RefreshBoss(0f);
            attacks?.StopCombat();
            GetComponent<Collider2D>().enabled = false;
            ScoreManager.Instance?.Add(defeatBonus);
            AudioManager.Instance?.Play("BossExplosion");
            for (int i = 0; i < 9; i++)
            {
                Vector3 offset = new(Random.Range(-1.5f, 1.5f), Random.Range(-0.55f, 0.55f), 0f);
                GameBootstrap.Instance?.SpawnExplosion(transform.position + offset, false, 1.5f);
                yield return new WaitForSeconds(0.16f);
            }
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance?.MissionComplete();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerHealth player)) player.TakeDamage(contactDamage);
        }

        public void OnPoolSpawned()
        {
            hp = maxHp;
            fighting = false;
            defeated = false;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = true;
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = true;
        }
        public void OnPoolDespawned() { }
    }
}
