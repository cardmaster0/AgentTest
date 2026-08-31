using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StarlightDefender;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace StarlightDefenderEditor
{
    public static class StarlightDefenderGenerator
    {
        private const string Root = "Assets/StarlightDefender";
        private const string ArtRoot = "Assets/Art/Generated";
        private const string PrefabRoot = Root + "/Prefabs";
        private const string SceneRoot = Root + "/Scenes";
        private const string AudioRoot = "Assets/Resources/StarlightDefender/Audio";

        private static readonly Dictionary<string, int> LayerIds = new();

        [MenuItem("Tools/Starlight Defender/Generate Game")]
        public static void GenerateGame()
        {
            try
            {
                EnsureFolders();
                GenerateArtwork();
                GenerateAudio();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ConfigureSpriteImporters();
                ConfigureLayers();
                Dictionary<string, GameObject> prefabs = CreatePrefabs();
                CreateScenes(prefabs);
                ConfigureBuildSettings();
                PlayerSettings.productName = "STARLIGHT DEFENDER";
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(SceneRoot + "/SD_Title.unity", OpenSceneMode.Single);
                Debug.Log("[STARLIGHT DEFENDER] Generation completed: art, audio, prefabs, scenes and build settings are ready.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        [MenuItem("Tools/Starlight Defender/Run Automated Playtest")]
        public static void RunAutomatedPlaytest()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            PlayerPrefs.SetInt(AutomatedPlaytest.PreviousHighScoreKey, PlayerPrefs.GetInt("StarlightDefender.HighScore", 0));
            PlayerPrefs.DeleteKey(AutomatedPlaytest.FailureKey);
            PlayerPrefs.SetInt(AutomatedPlaytest.TestKey, 1);
            PlayerPrefs.Save();
            EditorSceneManager.OpenScene(SceneRoot + "/SD_Title.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Starlight Defender/Clear Local Save Data")]
        public static void ClearLocalSaveData()
        {
            PlayerPrefs.DeleteKey("StarlightDefender.HighScore");
            PlayerPrefs.DeleteKey(AutomatedPlaytest.TestKey);
            PlayerPrefs.DeleteKey(AutomatedPlaytest.PreviousHighScoreKey);
            PlayerPrefs.DeleteKey(AutomatedPlaytest.FailureKey);
            PlayerPrefs.Save();
            Debug.Log("[STARLIGHT DEFENDER] Local high score and automated-test state cleared.");
        }

        private static void EnsureFolders()
        {
            string[] folders = { ArtRoot, PrefabRoot, SceneRoot, AudioRoot };
            foreach (string folder in folders) Directory.CreateDirectory(folder);
        }

        private static void ConfigureLayers()
        {
            string[] names = { "Player", "PlayerBullet", "Enemy", "EnemyBullet", "PowerUp" };
            foreach (string name in names) LayerIds[name] = EnsureLayer(name);

            int player = LayerIds["Player"];
            int playerBullet = LayerIds["PlayerBullet"];
            int enemy = LayerIds["Enemy"];
            int enemyBullet = LayerIds["EnemyBullet"];
            int powerUp = LayerIds["PowerUp"];
            Physics2D.IgnoreLayerCollision(player, playerBullet, true);
            Physics2D.IgnoreLayerCollision(player, powerUp, false);
            Physics2D.IgnoreLayerCollision(player, enemy, false);
            Physics2D.IgnoreLayerCollision(player, enemyBullet, false);
            Physics2D.IgnoreLayerCollision(playerBullet, playerBullet, true);
            Physics2D.IgnoreLayerCollision(playerBullet, enemyBullet, true);
            Physics2D.IgnoreLayerCollision(playerBullet, powerUp, true);
            Physics2D.IgnoreLayerCollision(enemy, enemy, true);
            Physics2D.IgnoreLayerCollision(enemy, enemyBullet, true);
            Physics2D.IgnoreLayerCollision(enemy, powerUp, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, enemyBullet, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, powerUp, true);
            Physics2D.IgnoreLayerCollision(powerUp, powerUp, true);
        }

        private static int EnsureLayer(string name)
        {
            int existing = LayerMask.NameToLayer(name);
            if (existing >= 0) return existing;
            UnityEngine.Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject serialized = new(tagManager);
            SerializedProperty layers = serialized.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue)) continue;
                slot.stringValue = name;
                serialized.ApplyModifiedProperties();
                return i;
            }
            throw new InvalidOperationException("No free User Layer is available for " + name);
        }

        private static Dictionary<string, GameObject> CreatePrefabs()
        {
            Sprite playerSprite = LoadSprite("Player");
            Sprite scoutSprite = LoadSprite("Scout");
            Sprite zigzagSprite = LoadSprite("Zigzag");
            Sprite shooterSprite = LoadSprite("Shooter");
            Sprite bossSprite = LoadSprite("Boss");
            Sprite playerBulletSprite = LoadSprite("PlayerBullet");
            Sprite enemyBulletSprite = LoadSprite("EnemyBullet");
            Sprite explosionSprite = LoadSprite("Explosion");

            Dictionary<string, GameObject> result = new();
            result["PlayerBullet"] = CreateProjectilePrefab("PlayerBullet", playerBulletSprite, true);
            result["EnemyBullet"] = CreateProjectilePrefab("EnemyBullet", enemyBulletSprite, false);
            result["Explosion"] = CreateExplosionPrefab(explosionSprite);

            result["Scout"] = CreateEnemyPrefab<ScoutEnemy>("Scout", scoutSprite, 2, 2.4f, 100, 1, 0f, 0.11f, result["Explosion"]);
            result["Zigzag"] = CreateEnemyPrefab<ZigzagEnemy>("Zigzag", zigzagSprite, 4, 1.85f, 220, 1, 0f, 0.16f, result["Explosion"]);
            result["Shooter"] = CreateShooterPrefab(shooterSprite, result["EnemyBullet"], result["Explosion"]);
            result["Boss"] = CreateBossPrefab(bossSprite, result["EnemyBullet"]);
            result["Rapid"] = CreatePowerUpPrefab("Rapid", LoadSprite("PowerRapid"), PowerUpType.Rapid);
            result["Spread"] = CreatePowerUpPrefab("Spread", LoadSprite("PowerSpread"), PowerUpType.Spread);
            result["Recover"] = CreatePowerUpPrefab("Recover", LoadSprite("PowerRecover"), PowerUpType.Recover);
            result["Player"] = CreatePlayerPrefab(playerSprite, result["PlayerBullet"], result["Explosion"]);
            AssetDatabase.SaveAssets();
            return result;
        }

        private static GameObject CreateProjectilePrefab(string name, Sprite sprite, bool player)
        {
            GameObject go = CreateSpriteObject(name, sprite, player ? "PlayerBullet" : "EnemyBullet", player ? 12 : 8);
            AddPhysics(go, new Vector2(0.16f, 0.42f));
            if (player) go.AddComponent<PlayerBullet>();
            else go.AddComponent<EnemyBullet>();
            return SavePrefab(go, name);
        }

        private static GameObject CreateExplosionPrefab(Sprite sprite)
        {
            GameObject go = CreateSpriteObject("Explosion", sprite, "EnemyBullet", 20);
            go.layer = 0;
            go.AddComponent<ExplosionEffect>();
            return SavePrefab(go, "Explosion");
        }

        private static GameObject CreatePlayerPrefab(Sprite sprite, GameObject bullet, GameObject flash)
        {
            GameObject go = CreateSpriteObject("Player", sprite, "Player", 10);
            AddPhysics(go, new Vector2(0.72f, 1.05f));
            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerHealth>();
            go.AddComponent<PowerUpManager>();
            PlayerShooter shooter = go.AddComponent<PlayerShooter>();
            shooter.Configure(bullet, flash);
            return SavePrefab(go, "Player");
        }

        private static GameObject CreateEnemyPrefab<T>(string name, Sprite sprite, int hp, float speed, int score,
            int contact, float shotInterval, float drop, GameObject explosion) where T : EnemyBase
        {
            GameObject go = CreateSpriteObject(name, sprite, "Enemy", 6);
            AddPhysics(go, new Vector2(0.78f, 0.78f));
            T enemy = go.AddComponent<T>();
            enemy.ConfigureStats(hp, speed, score, contact, shotInterval, drop, explosion);
            return SavePrefab(go, name);
        }

        private static GameObject CreateShooterPrefab(Sprite sprite, GameObject bullet, GameObject explosion)
        {
            GameObject go = CreateSpriteObject("Shooter", sprite, "Enemy", 6);
            AddPhysics(go, new Vector2(0.9f, 0.9f));
            ShooterEnemy enemy = go.AddComponent<ShooterEnemy>();
            enemy.ConfigureStats(6, 1.5f, 360, 1, 0.95f, 0.2f, explosion);
            enemy.ConfigureShooter(bullet);
            return SavePrefab(go, "Shooter");
        }

        private static GameObject CreateBossPrefab(Sprite sprite, GameObject bullet)
        {
            GameObject go = CreateSpriteObject("Starlight Dreadnought", sprite, "Enemy", 7);
            AddPhysics(go, new Vector2(3.2f, 1.25f));
            BossController boss = go.AddComponent<BossController>();
            BossAttackController attacks = go.AddComponent<BossAttackController>();
            attacks.Configure(bullet);
            return SavePrefab(go, "Boss");
        }

        private static GameObject CreatePowerUpPrefab(string name, Sprite sprite, PowerUpType type)
        {
            GameObject go = CreateSpriteObject(name, sprite, "PowerUp", 9);
            AddPhysics(go, new Vector2(0.6f, 0.6f));
            PowerUp power = go.AddComponent<PowerUp>();
            power.Configure(type);
            return SavePrefab(go, name);
        }

        private static GameObject CreateSpriteObject(string name, Sprite sprite, string layer, int sortingOrder)
        {
            GameObject go = new(name);
            go.layer = LayerIds.TryGetValue(layer, out int id) ? id : 0;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return go;
        }

        private static void AddPhysics(GameObject go, Vector2 colliderSize)
        {
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = colliderSize;
        }

        private static GameObject SavePrefab(GameObject source, string name)
        {
            string path = PrefabRoot + "/" + name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            UnityEngine.Object.DestroyImmediate(source);
            return prefab;
        }

        private static void CreateScenes(Dictionary<string, GameObject> prefabs)
        {
            Sprite star = LoadSprite("Star");
            Scene titleScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            GameObject titleRoot = new("Title Screen");
            titleRoot.AddComponent<TitleController>().Configure(star);
            EditorSceneManager.SaveScene(titleScene, SceneRoot + "/SD_Title.unity");

            Scene gameScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            GameObject systems = new("Game Systems");
            systems.AddComponent<ObjectPool>();
            systems.AddComponent<AudioManager>();
            systems.AddComponent<ScoreManager>();
            systems.AddComponent<GameManager>();
            systems.AddComponent<UIManager>();
            systems.AddComponent<AutomatedPlaytest>();
            BackgroundScroller background = systems.AddComponent<BackgroundScroller>();
            background.Configure(star);
            WaveManager waves = systems.AddComponent<WaveManager>();
            waves.Configure(prefabs["Scout"], prefabs["Zigzag"], prefabs["Shooter"], prefabs["Boss"]);
            GameBootstrap bootstrap = systems.AddComponent<GameBootstrap>();
            bootstrap.Configure(prefabs["Player"], prefabs["Scout"], prefabs["Zigzag"], prefabs["Shooter"], prefabs["Boss"],
                prefabs["Rapid"], prefabs["Spread"], prefabs["Recover"], prefabs["Explosion"]);
            EditorSceneManager.SaveScene(gameScene, SceneRoot + "/Game.unity");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.006f, 0.01f, 0.045f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
            UniversalAdditionalCameraData data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderType = CameraRenderType.Base;
        }

        private static void ConfigureBuildSettings()
        {
            string title = SceneRoot + "/SD_Title.unity";
            string game = SceneRoot + "/Game.unity";
            List<EditorBuildSettingsScene> scenes = new()
            {
                new EditorBuildSettingsScene(title, true),
                new EditorBuildSettingsScene(game, true)
            };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path == title || existing.path == game) continue;
                scenes.Add(new EditorBuildSettingsScene(existing.path, false));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Sprite LoadSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "/" + name + ".png");

        private static void ConfigureSpriteImporters()
        {
            foreach (string path in Directory.GetFiles(ArtRoot, "*.png", SearchOption.TopDirectoryOnly))
            {
                string assetPath = path.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 64f;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void GenerateAudio()
        {
            WriteTone("PlayerShot", 760f, 0.075f, 0.3f, 0.28f);
            WriteTone("EnemyExplosion", 150f, 0.2f, 0.7f, 0.45f);
            WriteTone("PlayerHit", 95f, 0.28f, 0.6f, 0.5f);
            WriteTone("PowerUp", 520f, 0.26f, 1.7f, 0.38f);
            WriteTone("BossWarning", 210f, 0.65f, 0.25f, 0.5f);
            WriteTone("BossExplosion", 70f, 0.8f, 0.5f, 0.58f);
        }

        private static void WriteTone(string name, float frequency, float duration, float sweep, float volume)
        {
            const int sampleRate = 44100;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            string path = AudioRoot + "/" + name + ".wav";
            using FileStream stream = new(path, FileMode.Create);
            using BinaryWriter writer = new(stream);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + samples * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(samples * 2);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float normalized = i / (float)samples;
                float envelope = Mathf.Sin(Mathf.PI * normalized) * (1f - normalized * 0.35f);
                float phase = 2f * Mathf.PI * frequency * t * (1f + sweep * normalized);
                float noise = Mathf.Sin(i * 12.9898f) * 0.12f;
                float value = (Mathf.Sin(phase) + noise * (1f - normalized)) * envelope * volume;
                writer.Write((short)(Mathf.Clamp(value, -1f, 1f) * short.MaxValue));
            }
        }

        private static void GenerateArtwork()
        {
            SaveTexture("Player", DrawPlayer());
            SaveTexture("Scout", DrawScout());
            SaveTexture("Zigzag", DrawZigzag());
            SaveTexture("Shooter", DrawShooter());
            SaveTexture("Boss", DrawBoss());
            SaveTexture("PlayerBullet", DrawBullet(true));
            SaveTexture("EnemyBullet", DrawBullet(false));
            SaveTexture("PowerRapid", DrawPowerUp(new Color(0.2f, 0.85f, 1f), 0));
            SaveTexture("PowerSpread", DrawPowerUp(new Color(0.8f, 0.3f, 1f), 1));
            SaveTexture("PowerRecover", DrawPowerUp(new Color(0.25f, 1f, 0.45f), 2));
            SaveTexture("Explosion", DrawExplosion());
            SaveTexture("Star", DrawStar());
            SaveTexture("LifeIcon", DrawPlayer());
        }

        private static void SaveTexture(string name, Texture2D texture)
        {
            texture.Apply();
            File.WriteAllBytes(ArtRoot + "/" + name + ".png", texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static Texture2D NewTexture(int width, int height)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            texture.name = "Generated Starlight Defender Art";
            Color32[] pixels = new Color32[width * height];
            texture.SetPixels32(pixels);
            return texture;
        }

        private static Texture2D DrawPlayer()
        {
            Texture2D t = NewTexture(96, 96);
            Glow(t, 48, 18, 16, new Color(0.15f, 0.85f, 1f, 0.5f));
            Polygon(t, new[] { P(48, 88), P(61, 55), P(82, 23), P(58, 32), P(48, 16), P(38, 32), P(14, 23), P(35, 55) }, new Color(0.12f, 0.45f, 0.85f));
            Polygon(t, new[] { P(48, 84), P(57, 52), P(48, 29), P(39, 52) }, new Color(0.86f, 0.97f, 1f));
            Polygon(t, new[] { P(44, 50), P(48, 70), P(52, 50), P(48, 38) }, new Color(0.15f, 0.9f, 1f));
            Line(t, 25, 31, 38, 50, 3, new Color(0.45f, 0.85f, 1f));
            Line(t, 71, 31, 58, 50, 3, new Color(0.45f, 0.85f, 1f));
            return t;
        }

        private static Texture2D DrawScout()
        {
            Texture2D t = NewTexture(80, 80);
            Glow(t, 40, 57, 10, new Color(1f, 0.12f, 0.05f, 0.4f));
            Polygon(t, new[] { P(40, 8), P(51, 30), P(72, 52), P(51, 47), P(40, 70), P(29, 47), P(8, 52), P(29, 30) }, new Color(0.82f, 0.08f, 0.08f));
            Polygon(t, new[] { P(40, 15), P(47, 34), P(40, 52), P(33, 34) }, new Color(1f, 0.64f, 0.5f));
            return t;
        }

        private static Texture2D DrawZigzag()
        {
            Texture2D t = NewTexture(96, 80);
            Polygon(t, new[] { P(48, 9), P(59, 31), P(91, 22), P(72, 52), P(55, 45), P(48, 70), P(41, 45), P(24, 52), P(5, 22), P(37, 31) }, new Color(0.48f, 0.1f, 0.72f));
            Polygon(t, new[] { P(48, 15), P(56, 36), P(48, 55), P(40, 36) }, new Color(0.93f, 0.45f, 1f));
            Line(t, 13, 30, 37, 37, 3, new Color(0.8f, 0.32f, 1f));
            Line(t, 83, 30, 59, 37, 3, new Color(0.8f, 0.32f, 1f));
            return t;
        }

        private static Texture2D DrawShooter()
        {
            Texture2D t = NewTexture(88, 88);
            Polygon(t, new[] { P(44, 8), P(61, 20), P(77, 43), P(63, 70), P(25, 70), P(11, 43), P(27, 20) }, new Color(0.9f, 0.34f, 0.05f));
            Circle(t, 44, 42, 18, new Color(0.25f, 0.12f, 0.08f));
            Circle(t, 44, 42, 11, new Color(1f, 0.72f, 0.15f));
            Rect(t, 39, 7, 49, 38, new Color(0.35f, 0.17f, 0.09f));
            Circle(t, 44, 15, 5, new Color(1f, 0.86f, 0.35f));
            return t;
        }

        private static Texture2D DrawBoss()
        {
            Texture2D t = NewTexture(240, 128);
            Glow(t, 120, 30, 25, new Color(1f, 0.08f, 0.04f, 0.28f));
            Polygon(t, new[] { P(120, 118), P(151, 93), P(219, 83), P(234, 39), P(176, 51), P(154, 19), P(86, 19), P(64, 51), P(6, 39), P(21, 83), P(89, 93) }, new Color(0.06f, 0.07f, 0.1f));
            Polygon(t, new[] { P(120, 108), P(146, 84), P(173, 62), P(150, 30), P(90, 30), P(67, 62), P(94, 84) }, new Color(0.2f, 0.22f, 0.26f));
            Polygon(t, new[] { P(120, 103), P(133, 78), P(120, 51), P(107, 78) }, new Color(0.82f, 0.06f, 0.08f));
            for (int x = 35; x <= 205; x += 34) Circle(t, x, 56, 6, new Color(1f, 0.12f, 0.06f));
            Rect(t, 42, 72, 74, 80, new Color(0.55f, 0.05f, 0.07f));
            Rect(t, 166, 72, 198, 80, new Color(0.55f, 0.05f, 0.07f));
            return t;
        }

        private static Texture2D DrawBullet(bool player)
        {
            Texture2D t = NewTexture(24, 40);
            Color glow = player ? new Color(0.2f, 0.9f, 1f, 0.35f) : new Color(1f, 0.12f, 0.06f, 0.35f);
            Color core = player ? new Color(0.82f, 1f, 1f) : new Color(1f, 0.72f, 0.15f);
            Glow(t, 12, 19, 10, glow);
            Polygon(t, new[] { P(12, 38), P(19, 18), P(15, 3), P(9, 3), P(5, 18) }, core);
            return t;
        }

        private static Texture2D DrawPowerUp(Color color, int symbol)
        {
            Texture2D t = NewTexture(64, 64);
            Glow(t, 32, 32, 28, new Color(color.r, color.g, color.b, 0.25f));
            Circle(t, 32, 32, 24, new Color(0.025f, 0.04f, 0.12f, 0.95f));
            Ring(t, 32, 32, 24, 4, color);
            if (symbol == 0)
                Polygon(t, new[] { P(35, 53), P(20, 32), P(30, 33), P(26, 11), P(45, 37), P(34, 36) }, color);
            else if (symbol == 1)
            {
                Line(t, 32, 15, 32, 49, 4, color);
                Line(t, 31, 29, 16, 45, 4, color);
                Line(t, 33, 29, 48, 45, 4, color);
            }
            else
            {
                Rect(t, 27, 15, 37, 49, color);
                Rect(t, 15, 27, 49, 37, color);
            }
            return t;
        }

        private static Texture2D DrawExplosion()
        {
            Texture2D t = NewTexture(96, 96);
            for (int r = 42; r >= 5; r--)
            {
                float k = r / 42f;
                Color c = Color.Lerp(new Color(1f, 1f, 0.8f, 1f), new Color(1f, 0.08f, 0f, 0.05f), k);
                Circle(t, 48, 48, r, c);
            }
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                Line(t, 48, 48, 48 + Mathf.RoundToInt(Mathf.Cos(a) * 45f), 48 + Mathf.RoundToInt(Mathf.Sin(a) * 45f), 2, new Color(1f, 0.5f, 0.08f, 0.7f));
            }
            return t;
        }

        private static Texture2D DrawStar()
        {
            Texture2D t = NewTexture(32, 32);
            Glow(t, 16, 16, 13, new Color(0.45f, 0.75f, 1f, 0.25f));
            Polygon(t, new[] { P(16, 31), P(19, 19), P(31, 16), P(19, 13), P(16, 1), P(13, 13), P(1, 16), P(13, 19) }, Color.white);
            return t;
        }

        private static Vector2Int P(int x, int y) => new(x, y);

        private static void Polygon(Texture2D texture, Vector2Int[] points, Color color)
        {
            int minX = Mathf.Max(0, points.Min(p => p.x));
            int maxX = Mathf.Min(texture.width - 1, points.Max(p => p.x));
            int minY = Mathf.Max(0, points.Min(p => p.y));
            int maxY = Mathf.Min(texture.height - 1, points.Max(p => p.y));
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                bool inside = false;
                for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                {
                    Vector2Int a = points[i];
                    Vector2Int b = points[j];
                    if ((a.y > y) == (b.y > y)) continue;
                    if (x < (b.x - a.x) * (y - a.y) / (float)(b.y - a.y) + a.x) inside = !inside;
                }
                if (inside) Blend(texture, x, y, color);
            }
        }

        private static void Rect(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Blend(texture, x, y, color);
        }

        private static void Circle(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            int rr = radius * radius;
            for (int y = cy - radius; y <= cy + radius; y++)
            for (int x = cx - radius; x <= cx + radius; x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= rr) Blend(texture, x, y, color);
        }

        private static void Ring(Texture2D texture, int cx, int cy, int radius, int thickness, Color color)
        {
            int outer = radius * radius;
            int inner = (radius - thickness) * (radius - thickness);
            for (int y = cy - radius; y <= cy + radius; y++)
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                int d = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (d <= outer && d >= inner) Blend(texture, x, y, color);
            }
        }

        private static void Glow(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int r = radius; r >= 1; r--)
            {
                float alpha = color.a * (1f - r / (float)radius) * 0.22f;
                Circle(texture, cx, cy, r, new Color(color.r, color.g, color.b, alpha));
            }
        }

        private static void Line(Texture2D texture, int x0, int y0, int x1, int y1, int thickness, Color color)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : i / (float)steps;
                Circle(texture, Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), thickness, color);
            }
        }

        private static void Blend(Texture2D texture, int x, int y, Color source)
        {
            if (x < 0 || y < 0 || x >= texture.width || y >= texture.height) return;
            Color destination = texture.GetPixel(x, y);
            float alpha = source.a + destination.a * (1f - source.a);
            if (alpha <= 0f) return;
            Color result = new(
                (source.r * source.a + destination.r * destination.a * (1f - source.a)) / alpha,
                (source.g * source.a + destination.g * destination.a * (1f - source.a)) / alpha,
                (source.b * source.a + destination.b * destination.a * (1f - source.a)) / alpha,
                alpha);
            texture.SetPixel(x, y, result);
        }
    }
}
