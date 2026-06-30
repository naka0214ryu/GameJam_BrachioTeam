using UnityEngine;
using UnityEngine.EventSystems; // UIのイベントを取得するために必要
using UnityEngine.UI; // Buttonコンポーネントにアクセスするために必要

// Buttonがアタッチされていることを必須にする
[RequireComponent(typeof(Button))]
public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("拡縮効果")]
    [SerializeField] private float hoverScale = 1.1f; // ホバーした時の拡大率
    private Vector3 originalScale; // 元のスケールを保存

    [Header("効果音")]
    [SerializeField] private AudioClip hoverSound;    // ホバーした時の音
    [SerializeField] private AudioClip clickSound;    // クリックした時の音
    
    // AudioSourceはGameAppManagerから取得するため、SerializeFieldは不要
    private AudioSource audioSource; 

    void Awake()
    {
        // 最初に元のスケールを記憶しておく
        originalScale = transform.localScale;

        // ★★★ 変更点 ★★★
        // GameAppManagerのインスタンスからUI効果音用のAudioSourceを取得する
        if (GameAppManager.Instance != null)
        {
            audioSource = GameAppManager.Instance.uiSoundSource;
        }
        else
        {
            Debug.LogError("GameAppManagerが見つかりません！ Bootシーンから開始しているか確認してください。");
        }
    }

    // マウスカーソルがボタンに乗った時に呼ばれる
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
        AudioManager.Instance.PlaySE("Hover");
        //PlaySound(hoverSound);
    }

    // マウスカーソルがボタンから離れた時に呼ばれる
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    // ボタンがクリックされた時に呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySE("Click");
        //PlaySound(clickSound);
    }

    // 効果音を再生する共通の処理
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // PlayOneShotを使うと、他の音を中断せずに再生できる
            audioSource.PlayOneShot(clip);
        }
    }
}
