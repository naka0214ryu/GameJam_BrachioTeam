using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuVisualController : MonoBehaviour
{
    private RectTransform skateImage;  //スケートの画像
    private Button startButton;  //スタートボタン
    //private bool skateMoveFin = false;  //スケート画像が動き終わったか判定 

    private void Start()
    {
        skateImage = GameObject.Find("SkatePanel").GetComponent<RectTransform>();
        startButton = GameObject.Find("StartGameButton").GetComponent<Button>();

        skateImage.localPosition = new Vector3(-1892f, 724f, 0f);

        //始まりと同時にスケートのイラストが滑ってくる
        StartCoroutine(SkateImageMove());
    }

    private void Update(){

    }

    private IEnumerator SkateImageMove()
    {
        float duration = 1f; //アニメーションの時間
        float elapsed = 0f; //経過時間
        Vector3 startPos = skateImage.localPosition;
        Vector3 endPos = startPos + new Vector3(1300f, 0f, 0f);
        
        while (elapsed < duration) //duration未満の間ループを続ける
        {
            //経過時間（elapsed）を正規化（0〜1の範囲）してLerpに渡す
            float t = elapsed / duration;
            
            //動きをだんだん遅くする
            t = 1 - (1 - t) * (1 - t);

            skateImage.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        //最終位置にしっかり配置
        skateImage.localPosition = endPos;
    }
}
