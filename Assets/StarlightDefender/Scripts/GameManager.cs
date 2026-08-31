using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace StarlightDefender
{
    public sealed class GameManager : MonoBehaviour
    {
        public const string TitleScene = "SD_Title";
        public const string GameScene = "Game";
        public static GameManager Instance { get; private set; }
        public PlayerHealth Player { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsPaused { get; private set; }

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (!IsFinished && keyboard.escapeKey.wasPressedThisFrame) TogglePause();
            if (IsFinished && keyboard.rKey.wasPressedThisFrame) Retry();
            if (IsFinished && keyboard.tKey.wasPressedThisFrame) ReturnToTitle();
        }

        public void RegisterPlayer(PlayerHealth player)
        {
            Player = player;
            UIManager.Instance?.RefreshHud();
        }

        public void TogglePause()
        {
            if (IsFinished) return;
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            UIManager.Instance?.ShowPause(IsPaused);
        }

        public void GameOver() => Finish(false);
        public void MissionComplete() => Finish(true);

        private void Finish(bool completed)
        {
            if (IsFinished) return;
            IsFinished = true;
            IsPaused = false;
            ScoreManager.Instance?.SaveHighScore();
            UIManager.Instance?.ShowResult(completed);
            Time.timeScale = 0f;
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameScene);
        }

        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(TitleScene);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }
    }
}
