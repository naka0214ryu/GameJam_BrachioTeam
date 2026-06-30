using UnityEngine;

public class testMainMenuProcessr : MonoBehaviour
{
    public void OnGameStartClicked()
    {
        GameAppManager.Instance.LoadScene("GameScene");
    }
}