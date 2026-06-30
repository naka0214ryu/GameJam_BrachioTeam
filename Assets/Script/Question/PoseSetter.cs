using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseSetter : MonoBehaviour
{
    [Header("動かす円")]
    public Transform leftArmCircle;
    public Transform rightArmCircle;
    public Transform leftLegCircle;
    public Transform rightLegCircle;

    [Header("読み込むポーズデータ")]
    public PoseData targetPose;

    /// <summary>
    /// 指定されたポーズデータに基づいて円を配置する
    /// </summary>
    public void LoadPose(PoseData poseToLoad)
    {
        if (poseToLoad == null)
        {
            Debug.LogError("ポーズデータが設定されていません！");
            return;
        }

        // ScriptableObjectからローカル座標を読み込み、それぞれの円のlocalPositionに設定する
        leftArmCircle.localPosition = poseToLoad.leftArmPosition;
        rightArmCircle.localPosition = poseToLoad.rightArmPosition;
        leftLegCircle.localPosition = poseToLoad.leftLegPosition;
        rightLegCircle.localPosition = poseToLoad.rightLegPosition;
    }
}
