using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewIKPoseList", menuName = "Pose Pattern/IK Pose List")]
public class IKPoseList : ScriptableObject
{
    // IKPoseDataのリストを保持する
    public List<IKPoseData> poses;
}