using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Title_UI_Manager : MonoBehaviour
{
    [Header("UI 참조")]
    public InputField _userNameinput;
    public Button _check_button;
    public Button _play_buttons;
    public Button _option_buttons;
    public Button _exit_buttons;

    Option_UI_Manager _option_UI;

    // 진행상황 체크용
    bool _is_button_Active = true;

    void Start()
    {
        _userNameinput.characterLimit = 10;
        if (Game_Data_manager.Instance.PlayerName != null)
        {
            _userNameinput.text = Game_Data_manager.Instance.PlayerName;
        }
        else
            return;

        _check_button.onClick.AddListener(SaveUserName);
        _play_buttons.onClick.AddListener(OnClickPlayButton);
        _option_buttons.onClick.AddListener(OnClickOptionButton);
        _exit_buttons.onClick.AddListener(OnClickExitButton);
    }

    #region[버튼 클릭 이벤트]
    public void SaveUserName()
    {
        if (_is_button_Active == false)
            return;

        _is_button_Active = false;

        string name = _userNameinput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        Game_Data_manager.Instance.PlayerName = name;
        Invoke(nameof(EnableButton), 0.5f);
    }

    public void OnClickPlayButton()
    {
        if (_is_button_Active == false)
            return;

        _is_button_Active = false;

        if (string.IsNullOrWhiteSpace(Game_Data_manager.Instance.PlayerName))
        {
            ShowWarning();
        }
        else
        {
            Scenes_Controller_manager._instance.StartLobbyScene();
        }

        Invoke(nameof(EnableButton), 0.5f);
    }

    public void OnClickOptionButton()
    {
        if (_is_button_Active == false)
            return;

        _is_button_Active = false;
    
        GameObject go = GameObject.Find("Title_UI_Frame");
        GameObject prefab = Resources.Load("UIs/Option_UI_BG") as GameObject;
        GameObject instance = Instantiate(prefab, go.transform);
        _option_UI = instance.GetComponent<Option_UI_Manager>();
        _option_UI.OpenUI();
    }

    public void OnClickExitButton()
    {
        if (_is_button_Active == false)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
    Application.Quit(); // 빌드된 게임에서 종료
#endif
    }

    // 버튼 클릭 활성화
    public void EnableButton()
    {
        _is_button_Active = true;
    }
    #endregion

    #region[경고문구]
    void ShowWarning()
    {
        GameObject go = GameObject.Find("Title_UI_Frame");
        GameObject prefab = Resources.Load<GameObject>("UIs/Warning_Text_Box_BG");
        GameObject instance = Instantiate(prefab, go.transform.position, Quaternion.identity, go.transform);
        Warning_Text_Box_Manager warning_Text_Box_Manager = instance.GetComponent<Warning_Text_Box_Manager>();

        warning_Text_Box_Manager.OpenBox("이름을 설정해 주세요.");

        StartCoroutine(Close_MS(instance));
    }
    IEnumerator Close_MS(GameObject ms_Box)
    {
        yield return new WaitForSeconds(1f);
        Destroy(ms_Box);
    }
    #endregion
}
