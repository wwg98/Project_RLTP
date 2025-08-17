using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Warning_Text_Box_Manager : MonoBehaviour
{
    [SerializeField] Text _warning_txt;

    public void OpenBox(string message)
    {
        _warning_txt.text = message;

    }
}
