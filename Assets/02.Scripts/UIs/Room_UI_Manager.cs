using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class Room_UI_Manager : MonoBehaviour
{
    [Header("UI 참조")]
    public Button _start_button;
    public Button _exit_button;
    public Text _player01_Txt;
    public Text _player02_Txt;
    public Image MasterCrown;

    bool _is_button_Active = true;
    Lobby_UI_Manager _lobby_UI_Manager;
    PhotonView _photonView;

    public void Update()
    {
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (!p.IsLocal)
                {
                    _player02_Txt.text = p.NickName;
                    break;
                }
            }
        }
        else
        {
            _player02_Txt.text = "Player 02";
        }
    }

    public void OpenUI()
    {
        gameObject.SetActive(true);
        _lobby_UI_Manager = GameObject.Find("Lobby_UI_Frame").GetComponent<Lobby_UI_Manager>();
        _player01_Txt.text = PhotonNetwork.LocalPlayer.NickName;
        _start_button.onClick.AddListener(OnClickStartButton);
        _exit_button.onClick.AddListener(OnClickExitButton);

        GameObject go = GameObject.FindGameObjectWithTag("Network_Manager");
        _photonView = gameObject.GetComponent<PhotonView>();

        if(PhotonNetwork.IsMasterClient)
        {
            MasterCrown.gameObject.SetActive(true);
        }
        else
        {
            MasterCrown.gameObject.SetActive(false);
        }
    }
    public void CloseUI()
    {
        gameObject.SetActive(false);
        _lobby_UI_Manager.EnableButton();
        PhotonNetwork.LeaveRoom();
        Destroy(gameObject);
    }

    public void OnClickStartButton()
    {
        if (_is_button_Active == false || PhotonNetwork.PlayerList.Length != 2)
            return;
        _is_button_Active = false;

        if (PhotonNetwork.IsMasterClient)
        {
            GameObject go = this.gameObject;
            GameObject instance = PhotonNetwork.Instantiate("Text_Box_BG", go.transform.position, Quaternion.identity);

            PhotonView pv = instance.GetComponent<PhotonView>();
            PhotonTransformView transformView = instance.GetComponent<PhotonTransformView>();
            transformView.enabled = true;
            pv.ObservedComponents = new List<Component> { transformView };
            pv.RPC("OpenUI_RPC", RpcTarget.All);
        }
        else
        {
            ShowWarning("방장이시작할 수 있습니다.");
        }
    }

    public void OnClickExitButton()
    {
        if (_is_button_Active == false)
            return;
        _is_button_Active = false;

        CloseUI();
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
