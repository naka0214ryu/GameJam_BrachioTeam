using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPoseList", menuName = "Pose Pattern/Pose List")]
public class PoseList : ScriptableObject
{
    // PoseDataのリストを保持する
    public List<PoseData> poses;
}