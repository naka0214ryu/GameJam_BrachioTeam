using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAppManager : MonoBehaviour
{
    public static GameAppManager Instance { get; private set; }

    // シーン全体で共有するAudioSourceへの参照
    public AudioSource clearSoundEffect;
    
    // ★★★ 追加 ★★★
    // UIの効果音を再生するためのAudioSource
    public AudioSource uiSoundSource;

    void Awake()
    {
        // シングルトンパターンの設定のみ行う
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // このロジックは本来通らないはずだが、念のため残しておく
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        AudioManager.Instance.PlayBGM(sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
