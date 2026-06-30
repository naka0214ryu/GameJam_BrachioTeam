using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPopUp : MonoBehaviour
{
    [SerializeField]
    Button Button;

    public void Open()
    {
        Button.gameObject.SetActive(true);
    }

    public void Close()
    {
        Button.gameObject.SetActive(false);
    }
}
