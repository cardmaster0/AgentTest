using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarlightDefender
{
    public sealed class TitleController : MonoBehaviour
    {
        [SerializeField] private Sprite starSprite;
        private Font font;

        public void Configure(Sprite star) => starSprite = star;

        private void Start()
        {
            Time.timeScale = 1f;
            Camera.main.backgroundColor = new Color(0.006f, 0.01f, 0.045f);
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (starSprite != null)
            {
                BackgroundScroller background = gameObject.AddComponent<BackgroundScroller>();
                background.Configure(starSprite);
            }
            BuildUI();
            int automatedStage = PlayerPrefs.GetInt(AutomatedPlaytest.TestKey, 0);
            if (automatedStage == 1) StartCoroutine(AutomatedStartCheck());
            else if (automatedStage == 4)
            {
                Debug.Log("[SD TEST] PASS: TITLE button route returned to SD_Title");
                if (PlayerPrefs.GetInt(AutomatedPlaytest.FailureKey, 0) == 0)
                    Debug.Log("[SD TEST] COMPLETE: automated Play Mode smoke test passed");
                else
                    Debug.LogError("[SD TEST] COMPLETE WITH FAILURES: inspect earlier test output");
                int previousHigh = PlayerPrefs.GetInt(AutomatedPlaytest.PreviousHighScoreKey, 0);
                if (previousHigh > 0) PlayerPrefs.SetInt("StarlightDefender.HighScore", previousHigh);
                else PlayerPrefs.DeleteKey("StarlightDefender.HighScore");
                PlayerPrefs.DeleteKey(AutomatedPlaytest.TestKey);
                PlayerPrefs.DeleteKey(AutomatedPlaytest.PreviousHighScoreKey);
                PlayerPrefs.DeleteKey(AutomatedPlaytest.FailureKey);
                PlayerPrefs.Save();
            }
        }

        private IEnumerator AutomatedStartCheck()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            Debug.Log("[SD TEST] PASS: GAME START route opened Game from SD_Title");
            StartGame();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) StartGame();
        }

        private void BuildUI()
        {
            EnsureEventSystem();
            GameObject canvasObject = new("Title UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Text title = CreateText(canvas.transform, "STARLIGHT\nDEFENDER", 96, new Color(0.45f, 0.92f, 1f));
            title.fontStyle = FontStyle.Bold;
            SetAnchors(title.rectTransform, new Vector2(0.06f, 0.6f), new Vector2(0.94f, 0.86f));
            Text subtitle = CreateText(canvas.transform, "— 2D SPACE DEFENSE MISSION —", 30, new Color(0.65f, 0.72f, 1f));
            SetAnchors(subtitle.rectTransform, new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.63f));
            Button start = CreateButton(canvas.transform, "GAME START", new Color(0.08f, 0.42f, 0.7f));
            SetAnchors(start.GetComponent<RectTransform>(), new Vector2(0.2f, 0.41f), new Vector2(0.8f, 0.5f));
            start.onClick.AddListener(StartGame);
            Text high = CreateText(canvas.transform, $"HIGH SCORE  {PlayerPrefs.GetInt("StarlightDefender.HighScore", 0):00000000}", 34, new Color(1f, 0.84f, 0.25f));
            SetAnchors(high.rectTransform, new Vector2(0.1f, 0.31f), new Vector2(0.9f, 0.38f));
            Text help = CreateText(canvas.transform,
                "MOVE   WASD / ARROW KEYS\nFIRE   SPACE / Z\nPAUSE  ESC\n\nRAPID  faster fire   •   SPREAD  triple shot   •   RECOVER  +1 life\n\nPRESS ENTER OR SPACE", 27, new Color(0.75f, 0.84f, 0.96f));
            SetAnchors(help.rectTransform, new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.29f));
        }

        private void StartGame() => SceneManager.LoadScene(GameManager.GameScene);

        private Text CreateText(Transform parent, string value, int size, Color color)
        {
            GameObject go = new("Text");
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(14, size / 2);
            text.resizeTextMaxSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Color color)
        {
            GameObject go = new(label);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText(go.transform, label, 44, Color.white);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject go = new("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
    }
}
