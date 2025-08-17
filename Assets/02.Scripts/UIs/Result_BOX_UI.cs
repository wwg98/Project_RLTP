using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Define_Enums;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Result_BOX_UI : MonoBehaviour
{
    [SerializeField]
    public Text _result_text;
    public GameObject _buttons;
    public Button _lobby_button;
    public Button _fix_Role_ReStart;
    public Button _Reset_Role_ReStart;
    public In_Game_Manger _in_Game_Manager;
    public PhotonView _photonView;

    bool _button_ON = true;
    public void OpenUI(Role winnerRole)
    {
        _button_ON = true;
        Role role = Game_Data_manager.Instance.Role;
        _buttons.SetActive(false);
        _result_text.text = winnerRole == role ? "V i c t o r y !" : "D e f e a t . . .";

        _lobby_button.onClick.AddListener(() => HandleButtonClick("RPC_Click_Lobby"));
        _fix_Role_ReStart.onClick.AddListener(() => HandleButtonClick("RPC_Click_fix_Role"));
        _Reset_Role_ReStart.onClick.AddListener(() => HandleButtonClick("RPC_Click_Reset"));

        StartCoroutine(ActiveButtons());
    }

    IEnumerator ActiveButtons()
    {
        yield return new WaitForSeconds(5f);
        _buttons.SetActive(true);
    }

    #region[버튼 상호작용]
    private void HandleButtonClick(string rpcMethod)
    {
        if (!_button_ON) return;

        _button_ON = false;

        if (!PhotonNetwork.IsMasterClient)
        {
            ShowWarning();
            _button_ON = true;
            return;
        }

        _photonView.RPC(rpcMethod, RpcTarget.All);
    }

    [PunRPC]
    public void RPC_Click_Lobby() => StartCoroutine(DisconnectAndLoadScene());

    IEnumerator DisconnectAndLoadScene()
    {
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        else if (PhotonNetwork.InLobby) PhotonNetwork.LeaveLobby();

        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
            yield return null;

        Scenes_Controller_manager._instance.StartLobbyScene();
    }

    [PunRPC]
    public void RPC_Click_fix_Role() => Scenes_Controller_manager._instance.ResetCurrentScene();
    [PunRPC]
    public void RPC_Click_Reset() => Scenes_Controller_manager._instance.StartSettingScene();

    private void ShowWarning()
    {
        GameObject prefab = Resources.Load<GameObject>("UIs/Warning_MasterClient_MS");
        GameObject instance = Instantiate(prefab, _buttons.transform.position, Quaternion.identity, _buttons.transform);
        StartCoroutine(CloseWarning(instance));
    }

    IEnumerator CloseWarning(GameObject warningBox)
    {
        yield return new WaitForSeconds(1f);
        Destroy(warningBox);
    }
    #endregion
}
