using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLegCircleController : MonoBehaviour
{
    private Vector3 offset;            //円をクリックしたときの位置
    private Vector3 circlePos;         //ドラッグしてる円の位置
    [SerializeField]private Transform moveAreaCenter;  //円の移動範囲の中心（オブジェクト）
    private const float radius = 2.0f; //円の移動範囲の半径

    void Start()
    {

    }

    void OnMouseDown()
    {
        //クリック位置を保存
        offset = gameObject.transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        circlePos = GetMouseWorldPos() + offset;
        Vector3 direction = circlePos - moveAreaCenter.position;//範囲の中心と操作中の円の差
        
        //範囲から外れてるか判定
        if (direction.magnitude > radius)
        {
            direction = direction.normalized * radius;
        }

        transform.position = moveAreaCenter.position + direction;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}