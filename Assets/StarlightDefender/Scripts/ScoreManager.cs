using UnityEngine;

namespace StarlightDefender
{
    public sealed class ScoreManager : MonoBehaviour
    {
        private const string HighScoreKey = "StarlightDefender.HighScore";
        public static ScoreManager Instance { get; private set; }
        public int Score { get; private set; }
        public int HighScore => PlayerPrefs.GetInt(HighScoreKey, 0);

        private void Awake()
        {
            Instance = this;
            Score = 0;
        }

        public void Add(int points)
        {
            if (points <= 0) return;
            Score += points;
            if (Score > HighScore)
            {
                PlayerPrefs.SetInt(HighScoreKey, Score);
                PlayerPrefs.Save();
            }
            UIManager.Instance?.RefreshHud();
        }

        public void SaveHighScore()
        {
            if (Score > HighScore) PlayerPrefs.SetInt(HighScoreKey, Score);
            PlayerPrefs.Save();
        }
    }
}
