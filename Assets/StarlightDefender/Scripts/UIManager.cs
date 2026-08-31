using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarlightDefender
{
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        private Font font;
        private Text scoreText;
        private Text lifeText;
        private Text powerText;
        private Text warningText;
        private GameObject bossBarRoot;
        private Image bossFill;
        private GameObject pausePanel;
        private GameObject resultPanel;
        private Text resultTitle;
        private Text resultScore;
        public float DisplayedBossRatio { get; private set; }

        private void Awake()
        {
            Instance = this;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildUI();
        }

        private void BuildUI()
        {
            EnsureEventSystem();
            GameObject canvasObject = new("Game UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            scoreText = CreateText(canvas.transform, "Score", "SCORE 00000000", 38, TextAnchor.MiddleLeft);
            SetRect(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -38f), new Vector2(500f, 64f), new Vector2(0f, 1f));
            lifeText = CreateText(canvas.transform, "Life", "LIFE ◆ ◆ ◆", 38, TextAnchor.MiddleRight);
            SetRect(lifeText.rectTransform, Vector2.one, Vector2.one, new Vector2(-38f, -38f), new Vector2(420f, 64f), Vector2.one);
            powerText = CreateText(canvas.transform, "Power", string.Empty, 30, TextAnchor.MiddleLeft);
            powerText.color = new Color(0.45f, 0.95f, 1f);
            SetRect(powerText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -96f), new Vector2(700f, 52f), new Vector2(0f, 1f));

            bossBarRoot = CreatePanel(canvas.transform, "Boss HP", new Color(0.02f, 0.02f, 0.08f, 0.88f));
            SetRect(bossBarRoot.GetComponent<RectTransform>(), new Vector2(0.15f, 0.91f), new Vector2(0.85f, 0.91f), Vector2.zero, new Vector2(0f, 28f), new Vector2(0.5f, 0.5f));
            GameObject fill = CreatePanel(bossBarRoot.transform, "Fill", new Color(0.92f, 0.12f, 0.2f));
            bossFill = fill.GetComponent<Image>();
            bossFill.type = Image.Type.Simple;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.01f, 0.18f);
            fillRect.anchorMax = new Vector2(0.99f, 0.82f);
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            bossBarRoot.SetActive(false);

            warningText = CreateText(canvas.transform, "Warning", string.Empty, 64, TextAnchor.MiddleCenter);
            warningText.fontStyle = FontStyle.Bold;
            warningText.color = new Color(1f, 0.18f, 0.12f);
            warningText.gameObject.SetActive(false);
            Stretch(warningText.rectTransform);

            pausePanel = CreatePanel(canvas.transform, "Pause", new Color(0f, 0f, 0.08f, 0.78f));
            Stretch(pausePanel.GetComponent<RectTransform>());
            Text pause = CreateText(pausePanel.transform, "Label", "PAUSED\n\nESC : RESUME", 52, TextAnchor.MiddleCenter);
            Stretch(pause.rectTransform);
            pausePanel.SetActive(false);

            BuildResult(canvas.transform);
        }

        private void BuildResult(Transform parent)
        {
            resultPanel = CreatePanel(parent, "Result", new Color(0.005f, 0.01f, 0.04f, 0.94f));
            Stretch(resultPanel.GetComponent<RectTransform>());
            resultTitle = CreateText(resultPanel.transform, "Title", "GAME OVER", 76, TextAnchor.MiddleCenter);
            resultTitle.fontStyle = FontStyle.Bold;
            SetRect(resultTitle.rectTransform, new Vector2(0.1f, 0.61f), new Vector2(0.9f, 0.78f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            resultScore = CreateText(resultPanel.transform, "Score", string.Empty, 40, TextAnchor.MiddleCenter);
            SetRect(resultScore.rectTransform, new Vector2(0.1f, 0.43f), new Vector2(0.9f, 0.62f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            Button retry = CreateButton(resultPanel.transform, "RETRY  [R]", new Color(0.08f, 0.4f, 0.68f));
            SetRect(retry.GetComponent<RectTransform>(), new Vector2(0.2f, 0.29f), new Vector2(0.8f, 0.38f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            retry.onClick.AddListener(() => GameManager.Instance?.Retry());
            Button title = CreateButton(resultPanel.transform, "TITLE  [T]", new Color(0.24f, 0.19f, 0.42f));
            SetRect(title.GetComponent<RectTransform>(), new Vector2(0.2f, 0.17f), new Vector2(0.8f, 0.26f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            title.onClick.AddListener(() => GameManager.Instance?.ReturnToTitle());
            resultPanel.SetActive(false);
        }

        public void RefreshHud()
        {
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
            scoreText.text = $"SCORE {score:00000000}";
            int life = GameManager.Instance?.Player != null ? GameManager.Instance.Player.Life : 3;
            lifeText.text = "LIFE " + new string('◆', Mathf.Max(0, life));
        }

        public void RefreshPowerUps(PowerUpManager powers)
        {
            if (powerText == null || powers == null) return;
            string rapid = powers.RapidActive ? $"RAPID {powers.RapidRemaining:0.0}s" : string.Empty;
            string spread = powers.SpreadActive ? $"SPREAD {powers.SpreadRemaining:0.0}s" : string.Empty;
            powerText.text = string.IsNullOrEmpty(rapid) ? spread : string.IsNullOrEmpty(spread) ? rapid : rapid + "   " + spread;
        }

        public void ShowBoss(BossController boss)
        {
            bossBarRoot.SetActive(true);
            RefreshBoss(boss != null ? boss.HealthRatio : 1f);
        }

        public void RefreshBoss(float ratio)
        {
            if (bossFill == null) return;
            float clamped = Mathf.Clamp01(ratio);
            DisplayedBossRatio = clamped;
            RectTransform rect = bossFill.rectTransform;
            Vector2 anchorMax = rect.anchorMax;
            anchorMax.x = Mathf.Lerp(0.01f, 0.99f, clamped);
            rect.anchorMax = anchorMax;
            bossFill.enabled = clamped > 0.0001f;
        }

        public void ShowPause(bool show) => pausePanel?.SetActive(show);

        public void ShowResult(bool completed)
        {
            bossBarRoot?.SetActive(false);
            resultPanel.SetActive(true);
            resultTitle.text = completed ? "MISSION COMPLETE" : "GAME OVER";
            resultTitle.color = completed ? new Color(0.35f, 1f, 0.88f) : new Color(1f, 0.22f, 0.22f);
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
            int high = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : PlayerPrefs.GetInt("StarlightDefender.HighScore", 0);
            resultScore.text = $"SCORE  {score:00000000}\nHIGH SCORE  {high:00000000}";
        }

        public void ShowWarning(string message, float seconds) => StartCoroutine(WarningRoutine(message, seconds));

        private IEnumerator WarningRoutine(string message, float seconds)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            float remaining = seconds;
            while (remaining > 0f)
            {
                warningText.enabled = Mathf.Repeat(remaining, 0.5f) > 0.12f;
                remaining -= Time.deltaTime;
                yield return null;
            }
            warningText.enabled = true;
            warningText.gameObject.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, string value, int size, TextAnchor alignment)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, size / 2);
            text.resizeTextMaxSize = size;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Color color)
        {
            GameObject go = CreatePanel(parent, label, color);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            Text text = CreateText(go.transform, "Label", label, 38, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject go = new("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
