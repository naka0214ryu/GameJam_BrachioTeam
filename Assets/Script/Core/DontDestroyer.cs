using UnityEngine;

// [Managers]オブジェクトをシーン間で永続化させるためだけのスクリプト
public class DontDestroyer : MonoBehaviour
{
    void Awake()
    {
        // このスクリプトがアタッチされているGameObject ([Managers]) を永続化する
        DontDestroyOnLoad(this.gameObject);
    }
}