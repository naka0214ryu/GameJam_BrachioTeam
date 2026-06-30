using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // UnityEventを使用するために追加

// PanelRouterクラスが存在することが前提です
[RequireComponent(typeof(PanelRouter))]
public class TutorialManager : MonoBehaviour
{
    /// <summary>
    /// チュートリアルの各ページの設定をまとめるクラス
    /// </summary>
    [System.Serializable]
    public class TutorialPage
    {
        [Tooltip("PanelRouterに設定したページのキー")]
        public string key;
        [Tooltip("このページの「次へ」ボタン（任意）")]
        public Button nextButton;
        [Tooltip("このページの「戻る」ボタン（任意）")]
        public Button backButton;
        [Tooltip("このページの「閉じる」ボタン（任意）")]
        public Button closeButton;
    }

    // --- インスペクターで設定する項目 ---

    [Header("チュートリアルページ設定")]
    [Tooltip("チュートリアルの各ページと、そのページに属するボタンを設定します")]
    [SerializeField]
    private List<TutorialPage> tutorialPages = new List<TutorialPage>();

    [Header("イベント")]
    [Tooltip("チュートリアルが閉じられた（完了した）ときに呼び出されます")]
    public UnityEvent onTutorialClosed;


    // --- 内部で利用する変数 ---

    private PanelRouter panelRouter;
    private int currentPageIndex = 0;

    private void Awake()
    {
        // PanelRouterコンポーネントを取得
        panelRouter = GetComponent<PanelRouter>();

        // 各ページのボタンにクリックイベントを登録
        foreach (var page in tutorialPages)
        {
            page.nextButton?.onClick.AddListener(NextPage);
            page.backButton?.onClick.AddListener(BackPage);
            page.closeButton?.onClick.AddListener(CloseTutorial);
        }

        // UnityEventがnullの場合に初期化
        if (onTutorialClosed == null)
        {
            onTutorialClosed = new UnityEvent();
        }
    }

    /// <summary>
    /// チュートリアルを開始します
    /// </summary>
    public void StartTutorial()
    {
        if (tutorialPages.Count == 0)
        {
            Debug.LogWarning("チュートリアルのページが設定されていません。");
            return;
        }

        currentPageIndex = 0;
        UpdateUI();
    }

    /// <summary>
    /// チュートリアルを終了し、完了イベントを呼び出します
    /// </summary>
    public void CloseTutorial()
    {
        // チュートリアル完了イベントを呼び出す
        // GameManager側でゲーム開始処理などを紐づける
        onTutorialClosed.Invoke();
    }

    /// <summary>
    /// 次のページへ進みます
    /// </summary>
    private void NextPage()
    {
        // 最後のページでなければインデックスを進める
        if (currentPageIndex < tutorialPages.Count - 1)
        {
            currentPageIndex++;
            UpdateUI();
        }
    }

    /// <summary>
    /// 前のページへ戻ります
    /// </summary>
    private void BackPage()
    {
        // 最初のページでなければインデックスを戻す
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateUI();
        }
    }

    /// <summary>
    /// 現在のページインデックスに基づいてUIの状態を更新します
    /// </summary>
    private void UpdateUI()
    {
        // 現在のページを表示
        string key = tutorialPages[currentPageIndex].key;
        panelRouter.ShowExclusive(key);

        // ボタンの表示/非表示制御は不要になります。
        // なぜなら、各ボタンは対応するパネルの子オブジェクトになっているため、
        // PanelRouterによるページの表示/非表示に追従するためです。
    }
}
