using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    [Header("出題するIKポーズのリスト")]
    public IKPoseList ikPoseList;

    [Header("ポーズを配置するスクリプト")]
    public IKPoseSetter ikPoseSetter;

    private int currentPoseIndex = 0;

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.Return))//エンター押したらポーズ切り替え 
        {
            //GoToNextPose();
        }
    }

    /// <summary>
    /// 次のポーズへ進める処理
    /// （ポーズ作成成功後などに使って）
    /// </summary>
    public void GoToNextPose()
    {
        // ポーズリストがない、またはポーズが1つ以下の場合は処理をしない
        if (ikPoseList == null || ikPoseList.poses.Count <= 1)
        {
            return;
        }

        int nextPoseIndex=currentPoseIndex;

        // 現在のポーズと異なるインデックスが選ばれるまでランダムに選択を繰り返す
        while (nextPoseIndex == currentPoseIndex)
        {
            nextPoseIndex = Random.Range(0, ikPoseList.poses.Count);
        }

        // 新しいポーズのインデックスを保存し、表示する
        currentPoseIndex = nextPoseIndex;

        ShowPose(currentPoseIndex);
    }


    private void ShowPose(int index)
    {
        IKPoseData nextPose = ikPoseList.poses[index];
        Debug.Log($"ポーズ {index + 1} ({nextPose.name}) を出題");
        ikPoseSetter.LoadPose(nextPose);
    }
}
