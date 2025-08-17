using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Define_Enums;
using Photon.Pun;

public class Text_Box_Manager : MonoBehaviourPun
{
    [SerializeField] private Text _time_text;
    [SerializeField] private double _count = 3;

    double startTime;
    bool timerRunning;
    PhotonView _photonView;
    Lobby_UI_Manager Lobby_UI_Manager;

    private void Update()
    {
        if (!timerRunning) return;

        double elapsed = PhotonNetwork.Time - startTime;

        if (elapsed >= _count)
        {
            timerRunning = false;
            _time_text.text = "게임 시작!";
            StartCoroutine(WaitMessage());
        }
        _time_text.text = $"{Mathf.Max(0, (float)(_count - (PhotonNetwork.Time - startTime))):F0}초 후에 게임 시작";
    }


    // UI 오픈
    [PunRPC]
    public void OpenUI_RPC()
    {
        gameObject.SetActive(true);
        GameObject go = GameObject.Find("Room_UI_BG(Clone)");
        Transform newParent = go.transform;
        transform.SetParent(newParent, false);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        Lobby_UI_Manager =  GameObject.Find("Lobby_UI_Frame").GetComponent<Lobby_UI_Manager>();

        _photonView = PhotonView.Get(this);
        StartSharedTimer();
    }



    IEnumerator WaitMessage()
    {
        yield return new WaitForSeconds(0.5f);
        Lobby_UI_Manager.NextScene();
    }

    #region [타이머]
    [PunRPC]
    public void StartTimer_RPC(double startTime)
    {
        this.startTime = startTime;
        timerRunning = true;
    }

    public void StartSharedTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            double startTime = PhotonNetwork.Time;
            photonView.RPC("StartTimer_RPC", RpcTarget.All, startTime);
        }
    }
    #endregion

}
