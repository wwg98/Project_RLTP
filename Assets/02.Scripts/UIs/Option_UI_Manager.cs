using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option_UI_Manager : MonoBehaviour
{
    [Header("UI ÂüÁ¶")]
    public Button _cancelButton;

    Title_UI_Manager _title_UI_Manager;

    bool _isButtonActive = true;
    public void OpenUI()
    {
        gameObject.SetActive(true);
        GameObject go = GameObject.Find("Title_UI_Frame") as GameObject;
        _title_UI_Manager = go.GetComponent<Title_UI_Manager>();

        _cancelButton.onClick.AddListener(CloseUI);
    }
    public void CloseUI()
    {
        if (_isButtonActive)
        {
            _title_UI_Manager.EnableButton();
            Destroy(gameObject);
        }
    }
}
