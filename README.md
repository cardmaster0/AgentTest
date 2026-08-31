# STARLIGHT DEFENDER

Unity 6 / URP 2Dで制作した、完成状態までプレイできる2D縦スクロールシューティングです。通常敵のWaveを突破し、約90秒後に出現する大型戦艦を撃破するとミッションクリアになります。既存の `Assets/Scenes/SampleScene.unity` は変更していません。

## 操作方法

- 移動: `WASD` または矢印キー
- 射撃: `Space` または `Z`（押し続けて連射）
- ポーズ/再開: `Escape`
- リザルト: `R` でリトライ、`T` でタイトル
- タイトル: `Enter` / `Space` または `GAME START` ボタン

新Input System（プロジェクトの `activeInputHandler: 1`）を使用しています。

## ゲーム内容

- ライフ3、被弾後1.5秒の無敵と点滅
- Scout、Zigzag、Shooterの3敵種
- 横一列、V字、左右交互を含む時間進行Wave
- RAPID、SPREAD、RECOVERの3パワーアップ
- HP割合で攻撃速度と攻撃パターンが変わるボス
- 扇状弾、プレイヤー狙いの連射、移動しながらの全周弾
- スコア、PlayerPrefsによるハイスコア保存
- 2層パララックスの星背景
- 発射、撃破、被弾、取得、ボス警告、ボス撃破の自動生成効果音
- 警告、爆発、発光、被弾点滅、ボス連続爆発
- 弾、敵、パワーアップ、爆発の共通オブジェクトプール

## シーン構成

- `Assets/StarlightDefender/Scenes/SD_Title.unity`: タイトル、操作説明、ハイスコア
- `Assets/StarlightDefender/Scenes/Game.unity`: ゲーム、ポーズ、GAME OVER、MISSION COMPLETE
- `Assets/Scenes/SampleScene.unity`: 元から存在するシーン（未変更、Build Settingsでは無効）

Build Settingsは `SD_Title`、`Game` の順で登録されています。リザルトはゲームシーン内の全画面UIとして表示されます。

## スクリプト構成

- プレイヤー: `PlayerController`、`PlayerShooter`、`PlayerHealth`、`PlayerBullet`
- 敵: `EnemyBase`、`ScoutEnemy`、`ZigzagEnemy`、`ShooterEnemy`、`EnemyBullet`
- Wave/ボス: `WaveManager`、`BossController`、`BossAttackController`
- パワーアップ: `PowerUp`、`PowerUpManager`
- ゲーム管理: `GameBootstrap`、`GameManager`、`ScoreManager`、`UIManager`、`AudioManager`
- 表現/最適化: `BackgroundScroller`、`ExplosionEffect`、`ObjectPool`
- 検証: `AutomatedPlaytest`
- 自動生成: `Assets/StarlightDefender/Editor/StarlightDefenderGenerator.cs`

ランタイムで毎フレーム `Find` を行う構成は避け、各管理クラスの参照と初期化時の登録で接続しています。

## 自動セットアップ

Unityメニューから次を実行できます。

`Tools > Starlight Defender > Generate Game`

以下を同じ名前で再生成するため、複数回実行しても無制限に重複しません。

- `Assets/Art/Generated` のオリジナルPNG 13点
- `Assets/Resources/StarlightDefender/Audio` のWAV 6点
- Player、弾、敵3種、ボス、パワーアップ3種、爆発のプレハブ
- タイトル/ゲームシーンとUI、カメラ、管理オブジェクト
- Player、PlayerBullet、Enemy、EnemyBullet、PowerUpレイヤーと衝突設定
- Build Settings

`Tools > Starlight Defender > Run Automated Playtest` で主要機能のPlay Modeスモークテストを実行できます。テスト前のハイスコアは終了時に復元されます。

## 生成画像

外部画像は使用していません。Editorスクリプトが透明Texture2Dへポリゴン、円、線、グローを描き、PNGとして保存します。Player、Scout、Zigzag、Shooter、Boss、両陣営の弾、3パワーアップ、Explosion、Star、LifeIconを生成します。

各PNGはSprite、Alpha Is Transparency有効、Bilinear、MipMap無効、Clamp、非圧縮、64 Pixels Per Unitとして自動インポートされます。

## 調整可能な主なパラメータ

- `WaveManager`: `bossSpawnTime`（通常90秒）、`debugFastBoss`、`debugBossSpawnTime`（12秒）、出現間隔
- `PlayerController`: 移動速度、画面端マージン
- `PlayerShooter`: 攻撃力、連射間隔、RAPID倍率
- `PlayerHealth`: 最大ライフ、無敵時間
- `PlayerBullet` / `EnemyBullet`: 速度、攻撃力、寿命
- 各 `EnemyBase`: 最大HP、速度、スコア、接触ダメージ、射撃間隔、ドロップ率
- `BossController`: 最大HP、登場速度、撃破ボーナス
- `BossAttackController`: 攻撃間隔、HP50%以下の速度倍率
- `PowerUpManager` / `PowerUp`: 効果時間、落下速度

## 使用しているUnity機能

- Unity 6000.3.21f1
- Universal Render Pipeline 17.3.0 / 2D Renderer
- Input System 1.20.0
- uGUI、CanvasScaler、2D Collider、Kinematic Rigidbody2D
- PlayerPrefs、SpriteRenderer、AudioSource、EditorSceneManager、PrefabUtility

## テスト結果

2026-08-02にUnity MCPから生成、コンパイル、Play Mode起動を実施しました。

- コンパイル: PASS（`recompile_status: completed / failed: false / errors: []`）
- タイトル/ゲームのPlay Mode起動とUI・背景・プレイヤー生成: PASS
- プレイヤー射撃、弾とScoutの衝突、撃破スコア: PASS
- Scout、Zigzag、Shooter生成: PASS
- RAPID、SPREAD、RECOVER: PASS
- 被弾と1.5秒無敵: PASS
- ポーズ/再開: PASS
- ボス登場、HP UI、弾幕、撃破ボーナス、MISSION COMPLETE: PASS
- GAME OVER、RETRY、TITLE遷移: PASS
- PlayerPrefsハイスコア保存: PASS
- Play Mode中の新規 `NullReferenceException` / `MissingReferenceException`: なし
- 自動Play Modeスモークテスト: 21/21 PASS（ボス3パターンの切替と高攻撃力時のHPゲージ同期を含む）

未解決の既知問題はありません。
