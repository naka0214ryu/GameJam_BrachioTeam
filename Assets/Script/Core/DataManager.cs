using UnityEngine;

// ゲームの開始モードを定義する列挙型
public enum GameStartMode
{
    Normal,     // 通常開始（カウントダウン）
    Tutorial    // チュートリアルから開始
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // --- シーンをまたいで渡すための一時的なデータ ---
    public GameStartMode NextGameStartMode { get; set; } = GameStartMode.Normal;

    // --- ゲーム全体で永続化するデータ ---
    //
    // 概要:
    //     PlayerPrefs is a class that stores Player preferences between game sessions.
    //     It can store string, float and integer values into the user’s platform registry.
    public bool ShouldShowTutorial
    {
        get => PlayerPrefs.GetInt("ShouldShowTutorial", 1) == 1; // デフォルトは表示する(1)
        set => PlayerPrefs.SetInt("ShouldShowTutorial", value ? 1 : 0);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}