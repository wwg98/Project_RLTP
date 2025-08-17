using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Define_Enums;
using Photon.Pun.UtilityScripts;
using Unity.VisualScripting;
using System;

public class Network_Manager : MonoBehaviourPunCallbacks
{
    [Header("������Ʈ")]
    [SerializeField] Lobby_UI_Manager _lobby_Manager;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnected()
    {
        base.OnConnected();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.LocalPlayer.NickName = Game_Data_manager.Instance.PlayerName;
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        EnableButton();
    }

    public void JoinRoom()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        string roomName = "Room";
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        _lobby_Manager.OpenRoomWindow();
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowWarning("�濡 ���� ���߽��ϴ�.");
        Invoke(nameof(EnableButton), 1.0f);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowWarning("���� �������� ���߽��ϴ�.");
        Invoke(nameof(EnableButton), 1.0f);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Scenes_Controller_manager._instance.StartTitleScene();
    }

    #region
    private void EnableButton()
    {
        _lobby_Manager.EnableButton();
    }
    #endregion

    #region[�����]
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
