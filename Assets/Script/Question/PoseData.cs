using UnityEngine;

[CreateAssetMenu(fileName = "NewPoseData", menuName = "Pose Pattern/Pose Data")]
public class PoseData : ScriptableObject 
{
    public Vector2 leftArmPosition;
    public Vector2 rightArmPosition;
    public Vector2 leftLegPosition;
    public Vector2 rightLegPosition;
}
