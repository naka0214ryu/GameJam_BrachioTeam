using UnityEngine;

// どの体の部位かを定義する
public enum BodyPart { LeftHand, RightHand, LeftFoot, RightFoot, Head }

public class TargetPoint : MonoBehaviour
{
    [Tooltip("各ポイントがどの部位に対応するか設定")]
    public BodyPart partType;
}