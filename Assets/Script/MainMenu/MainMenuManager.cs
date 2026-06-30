using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenuシーンのUIと全体的なフローを管理します。
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels via PanelRouter")]
    [SerializeField] private PanelRouter panelRouter;
    [SerializeField] private string mainPanelKey = "MainPanel";
    [SerializeField] private string configPanelKey = "ConfigPanel";
    [SerializeField] private string rankingPanelKey = "RankingPanel";

    [Header("Buttons")]
    [SerializeField] private Button normalStartButton;
    [SerializeField] private Button tutorialStartButton;
    [SerializeField] private Button openConfigButton;
    [SerializeField] private Button openRankingButton;
    [SerializeField] private Button closeConfigButton; // 設定画面を閉じるボタン
    [SerializeField] private Button closeRankingButton; // ランキング画面を閉じるボタン

    [Header("Tutorial Settings")]
    [SerializeField] private Toggle shouldShowTutorialToggle; // チュートリアルを表示するかのトグル

    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    void Start()
    {
        // --- ボタンのイベントリスナーを登録 ---
        normalStartButton.onClick.AddListener(OnNormalStart);
        // tutorialStartButton?.onClick.AddListener(OnTutorialStart);
        openConfigButton.onClick.AddListener(OpenConfigPanel);
        openRankingButton.onClick.AddListener(OpenRankingPanel);
        closeConfigButton.onClick.AddListener(OpenMainPanel);
        closeRankingButton.onClick.AddListener(OpenMainPanel);

        // --- チュートリアルトグルの設定 ---
        // DataManagerから現在の設定を読み込み、トグルの状態に反映
        shouldShowTutorialToggle.isOn = DataManager.Instance.ShouldShowTutorial;
        // トグルの値が変更されたら、DataManagerの設定を更新するようリスナーを登録
        shouldShowTutorialToggle?.onValueChanged.AddListener(OnTutorialToggleChanged);

        // 起動時にメインパネルを表示
        panelRouter.ShowExclusive(mainPanelKey);

        // --- AudioMixerの初期化 ---
        bgmSlider.value = AudioManager.Instance.GetBGMVolume();
        seSlider.value = AudioManager.Instance.GetSEVolume();
        
        bgmSlider.onValueChanged.AddListener(value =>
        {
            AudioManager.Instance.SetBGMVolume(value);
        });        
        
        seSlider.onValueChanged.AddListener(value =>
        {
            AudioManager.Instance.SetSEVolume(value);
        });
    }

    /// <summary>
    /// 通常モードでゲームを開始します。
    /// </summary>
    public void OnNormalStart()
    {
        DataManager.Instance.NextGameStartMode = GameStartMode.Normal;
        GameAppManager.Instance.LoadScene("GameScene");
    }

    /// <summary>
    /// チュートリアルモードでゲームを開始します。
    /// </summary>
    public void OnTutorialStart()
    {
        DataManager.Instance.NextGameStartMode = GameStartMode.Tutorial;
        GameAppManager.Instance.LoadScene("GameScene");
    }

    /// <summary>
    /// チュートリアル表示設定のトグルが変更されたときに呼ばれます。
    /// </summary>
    /// <param name="show">トグルの新しい値</param>
    private void OnTutorialToggleChanged(bool show)
    {
        DataManager.Instance.ShouldShowTutorial = show;
    }

    // --- パネル制御 ---
    private void OpenConfigPanel() => panelRouter.ShowExclusive(configPanelKey);
    private void OpenRankingPanel() => panelRouter.ShowExclusive(rankingPanelKey);
    private void OpenMainPanel() => panelRouter.ShowExclusive(mainPanelKey);
}
