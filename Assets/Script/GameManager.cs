using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// GameSceneのゲームフロー、スコア、UI、そしてプレイヤーの動きを管理します。
/// </summary>
public class GameManager : MonoBehaviour
{
    // ゲームの状態を定義する列挙型
    private enum GameState
    {
        Ready,      // 開始準備（チュートリアル or カウントダウン）
        Playing,    // プレイ中
        GameOver,   // ゲームオーバー演出中
        Result      // リザルト表示中
    }
    private GameState currentState;

    [Header("UI Panels via PanelRouter")]
    [SerializeField] private PanelRouter panelRouter;
    [SerializeField] private string tutorialPanelKey = "TutorialPanel";
    [SerializeField] private string countdownPanelKey = "CountdownPanel";
    [SerializeField] private string gameHUDPanelKey = "InGameHUD";
    [SerializeField] private string resultPanelKey = "ResultPanel";

    [Header("Tutorial UI Elements")]
    [SerializeField] private Button closeTutorialButton;
    [SerializeField] private Toggle dontShowTutorialToggle;

    [Header("CountDown UI Elements")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Game HUD Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI currentScoreText;

    [Header("Result UI Elements")]
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Game Settings")]
    [SerializeField] private float countdownDuration = 3.0f;
    [Tooltip("何問クリアするごとに難易度（速度）が上がるか指定します")]
    [SerializeField] private int levelUpInterval = 10;

    [Header("QuestionManager")]
    [SerializeField] private QuestionManager questionManager;

    [Header("Pictogram References")]
    [SerializeField] private Transform playerPictogram;
    [SerializeField] private Transform templatePictogram;
    [SerializeField] private LimbChecker[] limbCheckers;

    [Header("Effects")]
    [SerializeField] private ClearEffectController clearEffectController;
    [SerializeField] private TextMeshProUGUI failedText;
    [SerializeField] private TextMeshProUGUI levelUpText;

    [Header("Sound Effects")]
    [Tooltip("ポーズ入力中のカチカチ音を再生するAudioSource")]
    [SerializeField] private AudioSource tickingSoundSource;
    [Tooltip("カチカチ音の最小ピッチ")]
    [SerializeField] private float minPitch = 1.0f;
    [Tooltip("カチカチ音の最大ピッチ")]
    [SerializeField] private float maxPitch = 2.5f;


    // --- ゲームロジック用変数 ---
    private List<bool> judgeLimbs = new List<bool> { false, false, false, false, false };
    private int score = 0;
    private bool isJudging = false;

    // --- プレイヤーの動きに関する変数 ---
    [Header("Player Movement Settings")]
    [Tooltip("8の字運動の中心からの横方向の半径")]
    [SerializeField] private float moveRadiusX = 4.0f;
    [Tooltip("8の字運動の中心からの縦方向の半径")]
    [SerializeField] private float moveRadiusY = 2.0f;
    [Tooltip("軌道の前半（135度分）を移動する時間")] // 180度のうちの最初の3/4
    [SerializeField] private float fastMoveDuration = 1.0f;
    [Tooltip("軌道の後半（45度分）を移動する時間（ポーズ入力時間）")] // 180度のうちの最後の1/4
    [SerializeField] private float slowMoveDuration = 10.0f;
    [Tooltip("ゲームオーバー時の演出時間")]
    [SerializeField] private float gameOverAnimationDuration = 1.5f;

    private Vector3 playerMoveCenter;
    private Vector3 initialPlayerPosition;
    private float poseCycleTimer = 0.0f;
    private int currentCycle = 0; // 8の字の前半(0)か後半(1)かを管理

    // slowMoveDurationの初期値を保持するための変数
    private float slowMoveDurationOriginal;

    //BGM切り替え
    bool isFastMovePlaying = false;
    void Start()
    {
        closeTutorialButton.onClick.AddListener(OnCloseTutorial);
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnGoToMainMenu);

        dontShowTutorialToggle.isOn = !DataManager.Instance.ShouldShowTutorial;
        dontShowTutorialToggle.onValueChanged.AddListener(OnTutorialToggleChanged);

        initialPlayerPosition = playerPictogram.position;
        playerMoveCenter = initialPlayerPosition;

        // ゲーム開始時にInspectorで設定されたslowMoveDurationの初期値を保存
        slowMoveDurationOriginal = slowMoveDuration;

        if (levelUpText != null) levelUpText.enabled = false;//レベルアップテキストを非表示で初期化

        currentState = GameState.Ready;
        var startMode = DataManager.Instance.NextGameStartMode;

        if (startMode == GameStartMode.Tutorial || DataManager.Instance.ShouldShowTutorial)
        {
            panelRouter.ShowExclusive(tutorialPanelKey);
        }
        else
        {
            StartCoroutine(StartCountdown());
        }
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdatePlaying();
            UpdatePlayerMovement();
        }
    }

    public void OnCloseTutorial()
    {
        DataManager.Instance.ShouldShowTutorial = !dontShowTutorialToggle.isOn;
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        panelRouter.ShowExclusive(countdownPanelKey);
        float timer = countdownDuration;
        int lastSecond = -1; // 前回表示した秒数を記録する変数

        while (timer > 0)
        {
            int currentSecond = Mathf.CeilToInt(timer);
            countdownText.text = currentSecond.ToString();

            // 秒数が切り替わった瞬間に音を鳴らす
            if (currentSecond != lastSecond)
            {
                AudioManager.Instance.PlaySE("countdown_tick");
                lastSecond = currentSecond;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        countdownText.text = "GO!";
        AudioManager.Instance.PlaySE("countdown_go"); // ★★★ 追加: GO!の音を鳴らす
        yield return new WaitForSeconds(0.5f);
        SetupGame();
    }

    private void SetupGame()
    {
        score = 0;
        // ゲーム開始時やリトライ時に、保存しておいた初期値にリセットする
        slowMoveDuration = slowMoveDurationOriginal;
        currentCycle = 0;
        ResetPose();
        if (currentScoreText != null) currentScoreText.text = (score + 1).ToString() + "個目のポーズ！";
        panelRouter.ShowExclusive(gameHUDPanelKey);
        currentState = GameState.Playing;
    }

    private void UpdatePlaying()
    {
        if (isJudging) return;

        poseCycleTimer += Time.deltaTime;
        float totalCycleTime = fastMoveDuration + slowMoveDuration;

        if (poseCycleTimer > fastMoveDuration && poseCycleTimer < totalCycleTime)
        {
            float remainingSlowTime = totalCycleTime - poseCycleTimer;
            timerText.text = remainingSlowTime.ToString("F1");
        }
        else
        {
            timerText.text = "";
        }

        if (poseCycleTimer >= totalCycleTime)
        {
            StartCoroutine(JudgePoseCoroutine());
        }
    }

    private void UpdatePlayerMovement()
    {
        if (isJudging) return;

        float moveProgress = 0.0f;
        float totalCycleTime = fastMoveDuration + slowMoveDuration;
        float clampedTimer = Mathf.Min(poseCycleTimer, totalCycleTime);

        if (clampedTimer < fastMoveDuration)
        {
            if (!isFastMovePlaying)
            {
                AudioManager.Instance.PlayLoop("FastMove");
                isFastMovePlaying = true;
            }
            float phaseProgress = clampedTimer / fastMoveDuration;
            moveProgress = phaseProgress * 0.75f;

        }
        else
        {
            if (isFastMovePlaying)
            {
                AudioManager.Instance.StopAllSE();
                isFastMovePlaying = false;
                
                // slowフェーズ開始時にカチカチ音を再生開始
                if (tickingSoundSource != null && !tickingSoundSource.isPlaying)
                {
                    tickingSoundSource.pitch = minPitch; // ピッチを初期値に
                    tickingSoundSource.Play();
                }
            }
            float timeInSlowPhase = clampedTimer - fastMoveDuration;
            float phaseProgress = timeInSlowPhase / slowMoveDuration;
            moveProgress = 0.75f + (phaseProgress * 0.25f);

            // 時間経過でピッチを上げる
            if (tickingSoundSource != null && tickingSoundSource.isPlaying)
            {
                tickingSoundSource.pitch = Mathf.Lerp(minPitch, maxPitch, phaseProgress);
            }
        }

        MovePlayerOnPath(moveProgress);
    }

    private void MovePlayerOnPath(float progress)
    {
        float angleThisCycle = progress * Mathf.PI;
        float totalAngle = (currentCycle * Mathf.PI) + angleThisCycle;

        float x = playerMoveCenter.x + Mathf.Sin(totalAngle) * moveRadiusX;
        float y = playerMoveCenter.y + Mathf.Sin(totalAngle * 2.0f) * moveRadiusY;
        playerPictogram.position = new Vector3(x, y, playerMoveCenter.z);
    }


    private void ShowResult()
    {
        currentState = GameState.Result;
        resultScoreText.text = score.ToString() + "個のポーズ\nを決めた！";
        panelRouter.ShowExclusive(resultPanelKey);
    }


    private void ResetPose()
    {
        for (int i = 0; i < judgeLimbs.Count; i++)
        {
            judgeLimbs[i] = false;
        }
        poseCycleTimer = 0.0f;
        playerPictogram.position = initialPlayerPosition;
    }

    private void OnTutorialToggleChanged(bool show)
    {
        DataManager.Instance.ShouldShowTutorial = !show;
    }

    private void OnRetry()
    {
        DataManager.Instance.NextGameStartMode = GameStartMode.Normal;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGoToMainMenu()
    {
        GameAppManager.Instance.LoadScene("MainMenu");
    }

    private IEnumerator JudgePoseCoroutine()
    {
        // 判定が始まったらカチカチ音を止める
        if (tickingSoundSource != null) tickingSoundSource.Stop();

        isJudging = true;

        foreach (var checker in limbCheckers)
        {
            checker.ResetCheck();
        }

        yield return new WaitForFixedUpdate();

        judgeLimbs[0] = limbCheckers.FirstOrDefault(c => c.partType == BodyPart.LeftHand)?.IsInTargetArea ?? false;
        judgeLimbs[1] = limbCheckers.FirstOrDefault(c => c.partType == BodyPart.RightHand)?.IsInTargetArea ?? false;
        judgeLimbs[2] = limbCheckers.FirstOrDefault(c => c.partType == BodyPart.LeftFoot)?.IsInTargetArea ?? false;
        judgeLimbs[3] = limbCheckers.FirstOrDefault(c => c.partType == BodyPart.RightFoot)?.IsInTargetArea ?? false;
        judgeLimbs[4] = limbCheckers.FirstOrDefault(c => c.partType == BodyPart.Head)?.IsInTargetArea ?? false;

        string debugMessage = $"判定結果: 左手({judgeLimbs[0]}), 右手({judgeLimbs[1]}), 左足({judgeLimbs[2]}), 右足({judgeLimbs[3]}), 頭({judgeLimbs[4]})";
        Debug.Log(debugMessage);

        yield return new WaitForSeconds(0.5f);

        if (judgeLimbs.All(limb => limb))
        {
            Debug.Log("成功！");
            score++;

            bool isLevelUp = false;
            if (levelUpInterval > 0 && score > 0 && score % levelUpInterval == 0)
            {
                UpdateDifficulty();
                isLevelUp = true;
            }

            // クリアエフェクトを再生し、終了するまで待機する
            if (clearEffectController != null)
            {
                yield return StartCoroutine(clearEffectController.PlayClearEffect());
            }

            ResetPose();
            if (currentScoreText != null) currentScoreText.text = (score + 1).ToString() + "個目のポーズ！";
            currentCycle = (currentCycle + 1) % 2;
            questionManager.GoToNextPose();
            isJudging = false;

            if (isLevelUp)
            {
                StartCoroutine(ShowLevelUpEffect());
            }
        }
        else
        {
            Debug.Log("失敗！");
            StartCoroutine(GameOverCoroutine());
        }
    }

    /// <summary>
    /// スコアに基づいて難易度を調整します。
    /// slowMoveDurationを初期値とfastMoveDurationの間で指数関数的に変化させます。
    /// </summary>
    private void UpdateDifficulty()
    {
        // 100問解いたときに変化量が約半分になるような減衰定数を計算
        // k = ln(2) / 100
        const float decayConstant = 0.00693147f; // Mathf.Log(2f) / 100f

        // 減少の進行度を計算（スコアが上がるほど0に近づく）
        float progress = Mathf.Exp(-decayConstant * score);

        // slowMoveDurationを更新
        slowMoveDuration = fastMoveDuration + (slowMoveDurationOriginal - fastMoveDuration) * progress;

        // ★★★ 変更点 ★★★
        // レベルアップしたことがわかるようにログを出力
        Debug.Log($"★★★ LEVEL UP! (Score: {score}) | New slowMoveDuration: {slowMoveDuration} ★★★");
    }


    private IEnumerator GameOverCoroutine()
    {
        // ゲームオーバー時もカチカチ音を止める
        if (tickingSoundSource != null) tickingSoundSource.Stop();
        
        currentState = GameState.GameOver;
        FailedEffect();
        float timer = 0f;
        float startProgress = 0.0f;
        float endProgress = 0.5f;
        int nextCycle = (currentCycle + 1) % 2;

        while (timer < gameOverAnimationDuration)
        {
            timer += Time.deltaTime;
            float currentProgress = Mathf.Lerp(startProgress, endProgress, timer / gameOverAnimationDuration);

            float angleThisCycle = currentProgress * Mathf.PI;
            float totalAngle = (nextCycle * Mathf.PI) + angleThisCycle;
            float x = playerMoveCenter.x + Mathf.Sin(totalAngle) * moveRadiusX;
            float y = playerMoveCenter.y + Mathf.Sin(totalAngle * 2.0f) * moveRadiusY;
            playerPictogram.position = new Vector3(x, y, playerMoveCenter.z);

            yield return null;
        }

        float finalAngleThisCycle = endProgress * Mathf.PI;
        float finalTotalAngle = (nextCycle * Mathf.PI) + finalAngleThisCycle;
        float finalX = playerMoveCenter.x + Mathf.Sin(finalTotalAngle) * moveRadiusX;
        float finalY = playerMoveCenter.y + Mathf.Sin(finalTotalAngle * 2.0f) * moveRadiusY;
        playerPictogram.position = new Vector3(finalX, finalY, playerMoveCenter.z);

        ShowResult();
    }

    private void FailedEffect()
    {
        AudioManager.Instance.PlaySE("Failed");
        if (failedText != null) failedText.text = "Failed ...";
    }
    
    // レベルアップ時のテキストアニメーション用コルーチン
    private IEnumerator ShowLevelUpEffect()
    {
        if (levelUpText == null) yield break;

        levelUpText.enabled = true;
        levelUpText.text = "SPEED UP!";

        float duration = fastMoveDuration*2; // アニメーション全体の時間
        float halfDuration = duration / 2f;
        Vector3 initialScale = Vector3.one * 0.5f;

        // 初期状態を設定（小さく、透明に）
        levelUpText.transform.localScale = initialScale;
        levelUpText.color = new Color(levelUpText.color.r, levelUpText.color.g, levelUpText.color.b, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 前半で拡大・フェードイン、後半で縮小・フェードアウト
            float progress = (elapsed < halfDuration) 
                ? (elapsed / halfDuration) 
                : (1 - (elapsed - halfDuration) / halfDuration);

            levelUpText.transform.localScale = Vector3.Lerp(initialScale, Vector3.one, progress);
            levelUpText.color = new Color(levelUpText.color.r, levelUpText.color.g, levelUpText.color.b, progress);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // アニメーション終了後に非表示にする
        levelUpText.enabled = false;
    }
}
