using UnityEngine;

public class LimbChecker : MonoBehaviour
{
    [Tooltip("このColliderがどの部位に対応するか設定します")]
    public BodyPart partType;

    // 外部から読み取り可能なプロパティ
    public bool IsInTargetArea { get; private set; }

    void Start()
    {
        IsInTargetArea = false;
    }

    // 判定開始前にGameManagerから呼ばれる
    public void ResetCheck()
    {
        IsInTargetArea = false;
    }

    // ★★★ OnTriggerEnter2D から OnTriggerStay2D に変更 ★★★
    // Trigger内のColliderと接触している間、毎フレーム呼ばれる
    private void OnTriggerStay2D(Collider2D other)
    {
        // 接触相手がTargetPointかチェック
        if (other.TryGetComponent<TargetPoint>(out var targetPoint))
        {
            // 自分と同じ部位のターゲットに触れていたらフラグを立てる
            if (targetPoint.partType == this.partType)
            {
                // Debug.Logはログが多くなりすぎる可能性があるのでコメントアウトしてもOK
                // Debug.Log($"LimbChecker: {partType} is in contact with TargetPoint: {targetPoint.partType}");
                IsInTargetArea = true;
            }
        }
    }
}
