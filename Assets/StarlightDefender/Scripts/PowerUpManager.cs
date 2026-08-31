using UnityEngine;

namespace StarlightDefender
{
    public sealed class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 10f;
        private float rapidRemaining;
        private float spreadRemaining;
        private PlayerHealth health;
        public bool RapidActive => rapidRemaining > 0f;
        public bool SpreadActive => spreadRemaining > 0f;
        public float RapidRemaining => rapidRemaining;
        public float SpreadRemaining => spreadRemaining;

        private void Awake() => health = GetComponent<PlayerHealth>();

        private void Update()
        {
            if (rapidRemaining > 0f) rapidRemaining = Mathf.Max(0f, rapidRemaining - Time.deltaTime);
            if (spreadRemaining > 0f) spreadRemaining = Mathf.Max(0f, spreadRemaining - Time.deltaTime);
            UIManager.Instance?.RefreshPowerUps(this);
        }

        public void Apply(PowerUpType type, float duration)
        {
            float time = duration > 0f ? duration : defaultDuration;
            switch (type)
            {
                case PowerUpType.Rapid: rapidRemaining += time; break;
                case PowerUpType.Spread: spreadRemaining += time; break;
                case PowerUpType.Recover: health?.Recover(1); break;
            }
            AudioManager.Instance?.Play("PowerUp");
            UIManager.Instance?.RefreshHud();
        }
    }
}
