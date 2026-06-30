using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IKPoseData", menuName = "IK/PoseData")]
public class IKPoseData : ScriptableObject
{
    [Serializable]
    public class BoneData 
    {
        public string boneName; 
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    public BoneData[] bones;
}
