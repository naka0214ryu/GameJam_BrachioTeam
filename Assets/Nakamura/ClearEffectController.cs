using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClearEffectController : MonoBehaviour
{
    [Header("Effect Components")]
    [SerializeField] private GameObject clapHands;
    [SerializeField] private TextMeshProUGUI clearText; 

    //private AudioSource clearSoundEffect; // ローカルで参照を保持
    private bool isEffectRunning = false; // エフェクトが実行中かどうか

    void Start()
    {
        // コンポーネントが設定されていない場合、Findで探すフォールバック
        if (clapHands == null) clapHands = GameObject.Find("ClapHands");
        if (clearText == null) clearText = GameObject.Find("ClearText").GetComponent<TextMeshProUGUI>();
        
        // 初期化
        if (clearText != null) clearText.enabled = false;

        if (clapHands != null)
        {
            Vector3 initialPos = clapHands.transform.position;
            initialPos.y = -100f; 
            clapHands.transform.position = initialPos;
        }
    }

    /// <summary>
    /// GameManagerから呼び出されるクリアエフェクト再生用のコルーチン
    /// </summary>
    public IEnumerator PlayClearEffect()
    {
        if (!isEffectRunning)
        {
            // エフェクトの各要素を再生
            //if (clearSoundEffect != null) clearSoundEffect.Play();
            AudioManager.Instance.PlaySE("Clap");
            if (clearText != null) clearText.enabled = true;
            //手の動き
            if (clapHands != null)
            {
                yield return StartCoroutine(ClapHandsMove());
            }
            else
            {
                StartCoroutine(ClapHandsMove());
            }
        }
    }

    private IEnumerator ClapHandsMove()
    {
        isEffectRunning = true;
        Vector3 startPos = clapHands.transform.position;      // 拍手イラストの初期位置
        Vector3 middlePos = new Vector3(startPos.x, 350f, startPos.z);  // 上下の動きの下
        Vector3 upPos = middlePos + new Vector3(0, 111f, 0); // 上下の動きの上
        float duration = 0.1f; // 動きの所要時間

        yield return MoveToPosition(startPos, middlePos, duration);

        for(int i = 0; i < 2; i++)
        {
            yield return MoveToPosition(middlePos, upPos, 0.2f);
            yield return MoveToPosition(upPos, middlePos, 0.2f);
        }
        
        yield return new WaitForSeconds(0.4f);

        yield return MoveToPosition(middlePos, startPos, duration);

        isEffectRunning = false;
        if (clearText != null) clearText.enabled = false;
    }
    
    // 位置を補間しながら移動するヘルパーメソッド
    private IEnumerator MoveToPosition(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            clapHands.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        clapHands.transform.position = end;
    }
}
