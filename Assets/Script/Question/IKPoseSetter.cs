using UnityEngine;
using System.Collections.Generic;

public class IKPoseSetter : MonoBehaviour
{
    [Header("ポーズを適用するスケルトンのルート")]
    public Transform skeletonRoot;

    private Dictionary<string, Transform> boneMap;

    void Awake()
    {
        boneMap = new Dictionary<string, Transform>();
        if (skeletonRoot != null)
        {
            foreach (Transform bone in skeletonRoot.GetComponentsInChildren<Transform>())
            {
                boneMap[bone.name] = bone;
            }
        }
    }

    public void LoadPose(IKPoseData poseToLoad)
    {
        if (poseToLoad == null || poseToLoad.bones == null)
        {
            Debug.LogError("ポーズデータが設定されていません！");
            return;
        }

        foreach (IKPoseData.BoneData boneData in poseToLoad.bones)
        {
            if (boneMap.TryGetValue(boneData.boneName, out Transform bone))
            {
                // ★ ワールド座標ではなくローカル座標を設定する
                bone.localPosition = boneData.localPosition;
                bone.localRotation = boneData.localRotation;
            }
        }
    }
}