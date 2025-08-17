using Define_Enums;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Lobby_UI_Manager : MonoBehaviour
{
    [Header("버튼 참조")]
    public Button _local_Game_Button;
    public Button _online_Game_Button;
    public Button _cancel_Button;
    [Header("컴포넌트")]
    [SerializeField] Network_Manager _network_Manager;

    private Room_UI_Manager _room_UI;
    public bool _is_button_Active = false;
    PhotonView _photonView;

    void Start()
    {
        _local_Game_Button.onClick.AddListener(OnClickLocalButton);
        _online_Game_Button.onClick.AddListener(OnClickOnlineButton);
        _cancel_Button.onClick.AddListener(OnClickCancelButton);
        _photonView = PhotonView.Get(this);
    }

    #region[버튼 클릭 이벤트]
    public void OnClickLocalButton()
    {
        if (_is_button_Active == false)
            return;
        _is_button_Active = false;
        ShowWarning("로컬은 준비중 입니다.");

        Invoke(nameof(EnableButton), 0.5f);
    }
    public void OnClickOnlineButton()
    {
        if (_is_button_Active == false)
            return;
        _is_button_Active = false;

        _network_Manager.JoinRoom();
    }
    public void OnClickCancelButton()
    {
        if (_is_button_Active == false)
            return;

        _is_button_Active = false;

        PhotonNetwork.Disconnect();
    }

    public void EnableButton()
    {
        _is_button_Active = true;
    }
    public void DisableButton()
    {
        _is_button_Active = false;
    }
    #endregion


    public void OpenRoomWindow()
    {
        GameObject go = GameObject.Find("Lobby_UI_Frame");
        GameObject prefab = Resources.Load("UIs/Room_UI_BG") as GameObject;
        GameObject instance = Instantiate(prefab, go.transform);
        _room_UI = instance.GetComponent<Room_UI_Manager>();

        if (PhotonNetwork.InRoom)
        {
            _room_UI.OpenUI();
        }
        else
        {
            ShowWarning("방에 들어와있지않습니다.");
            EnableButton();
        }
    }

    // 씬 넘기기
    public void NextScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Scenes_Controller_manager._instance.SetNowScene(ScenesKind.Setting_Scene);
            PhotonNetwork.LoadLevel((int)ScenesKind.Setting_Scene);
        }
    }
    #region[경고문구]
    void ShowWarning(string message)
    {
        GameObject go = GameObject.Find("Lobby_UI_Frame");
        GameObject prefab = Resources.Load<GameObject>("UIs/Warning_Text_Box_BG");
        GameObject instance = Instantiate(prefab, go.transform.position, Quaternion.identity, go.transform);
        Warning_Text_Box_Manager warning_Text_Box_Manager = instance.GetComponent<Warning_Text_Box_Manager>();

        warning_Text_Box_Manager.OpenBox(message);

        StartCoroutine(Close_MS(instance));
    }
    IEnumerator Close_MS(GameObject ms_Box)
    {
        yield return new WaitForSeconds(1f);
        Destroy(ms_Box);
    }
    #endregion
}
