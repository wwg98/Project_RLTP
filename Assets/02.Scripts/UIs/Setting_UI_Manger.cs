using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Define_Enums;
using Photon.Realtime;
using Photon.Pun.Demo.Asteroids;
using System.Net.Mail;
using System.Security.Cryptography;

public class Setting_UI_Manger : MonoBehaviourPunCallbacks
{
    [Header("Select_UIs 참조")]
    public float _count = 15;
    public Text _timer_Text;
    public Text _player01_Text;
    public Text _player02_Text;
    public Text _player02_State_Text;
    public Text _center_Text;
    public Button _rockButton;
    public Button _scissorsButton;
    public Button _paperButton;
    [Header("Result_UIs 참조")]
    public Text _player01_Result_Text;
    public Text _player02_Result_Text;
    [Header("컴포넌트 참조")]
    public Map_Select_UI_Manager _map_Select_UI_Manager;
    public Role_Select_UI_Manager _role_Manager;
    [Header("게임 오브젝트")]
    public GameObject _select_UIs;
    public GameObject _result_UIs;
    public GameObject _role_select_UIs;
    public GameObject _map_select_UIs;

    double startTime;
    bool timerRunning;
    bool _buttonOn = false;
    bool _all__Player_Ready = false;
    bool _RSP_end = false;
    bool _forceStopped = false;
    bool _choice = false;
    Player otherPlayer;
    PhotonView _myPhotonView;

    RSPKind _mySelect;
    RSPKind rSPKind;
    Setting_progress _now_progress;

    void Start()
    {
        ResetPlayerProperties();
        _result_UIs.SetActive(false);
        _role_select_UIs.SetActive(false);
        _myPhotonView = PhotonView.Get(this);

        _center_Text.text = "역할 선택을 위해 가위, 바위 보를 진행하세요.";

        _player01_Text.text = PhotonNetwork.LocalPlayer.NickName;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.IsLocal)
            {
                _player02_Text.text = p.NickName;
                otherPlayer = p;
                break;
            }
        }

        _now_progress = Setting_progress.RSP;

        StartCoroutine(EnableButtonsAfterDelay(1f));

    }

    void Update()
    {
        switch (_now_progress)
        {
            case Setting_progress.RSP:
                if (_buttonOn)
                {
                    _buttonOn = false;
                    Play_RSP();
                }
                counting();
                break;

            case Setting_progress.Role:
                break;
        }
    }


    #region[RSP]
    public void Play_RSP()
    {
        _rockButton.onClick.AddListener(OnclickRock);
        _scissorsButton.onClick.AddListener(OnclickScissors);
        _paperButton.onClick.AddListener(OnclickPaper);
    }

    public void OnclickRock()
    {
        if(!_choice)
        {
            _choice = true;
            _mySelect = RSPKind.Rock;
            SetPlayerProperties(true, _mySelect);
        }

    }
    public void OnclickScissors()
    {
        if (!_choice)
        {
            _choice = true;
            _mySelect = RSPKind.Scissors;
            SetPlayerProperties(true, _mySelect);
        }
    }
    public void OnclickPaper()
    {
        if (!_choice)
        {
            _choice = true;
            _mySelect = RSPKind.Paper;
            SetPlayerProperties(true, _mySelect);
        }
    }

    public void RPC_SetResult()
    {
        if (otherPlayer == null)
        {
            Debug.LogWarning("상대 플레이어가 없습니다.");
            return;
        }

        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("RSP", out object local_RSPValue))
        {
            Debug.LogWarning("내 RSP 값이 없습니다.");
            return;
        }

        if (!otherPlayer.CustomProperties.TryGetValue("RSP", out object other_RSPValue))
        {
            Debug.LogWarning("상대의 RSP 값이 없습니다.");
            return;
        }

        Get_RSP_Result((RSPKind)local_RSPValue, (RSPKind)other_RSPValue);
    }

    public void Get_RSP_Result(RSPKind my, RSPKind opponent)
    {
        if (my == opponent)
        {
            _center_Text.text = "비겼습니다.\n자동으로 역할이 정해질 것입니다.";
            Game_Data_manager.Instance.RSPResult = RSP_Result.draw;
            SetPlayerResultProperties(RSP_Result.draw);
        }
        else if ((my == RSPKind.Rock && opponent == RSPKind.Scissors) ||
            (my == RSPKind.Scissors && opponent == RSPKind.Paper) ||
            (my == RSPKind.Paper && opponent == RSPKind.Rock))
        {
            _center_Text.text = "이겼습니다.\n역할을 먼저 선택할 수 있습니다.";
            Game_Data_manager.Instance.RSPResult = RSP_Result.Win;
            SetPlayerResultProperties(RSP_Result.Win);
        }
        else
        {
            _center_Text.text = "졌습니다.\n상대가 역할을 선택하는 것을 기다려주세요.";
            Game_Data_manager.Instance.RSPResult = RSP_Result.Defeat;
            SetPlayerResultProperties(RSP_Result.Defeat);
        }
    }

    public void RandomRSP()
    {
        int randomRSP = Random.Range(0, 3);
        switch (randomRSP)
        {
            case 0:
                rSPKind = RSPKind.Rock;
                break;
            case 1:
                rSPKind = RSPKind.Scissors;
                break;
            case 2:
                rSPKind = RSPKind.Paper;
                break;
        }
        SetPlayerProperties(true, rSPKind);
    }
    #endregion

    #region[프로퍼티]

    // 프로퍼티 콜백
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Ready"))
        {
            bool isReady = (bool)changedProps["Ready"];

            CheckAllPlayersReady();

            if (targetPlayer != PhotonNetwork.LocalPlayer)
            {
                if (isReady == false)
                {
                    _player02_State_Text.text = "Waiting...";
                }
                else
                {
                    _player02_State_Text.text = "Ready";
                }
            }

        }

        if (changedProps.ContainsKey("RSP"))
        {
            RSPKind RSP = (RSPKind)changedProps["RSP"];

            if(targetPlayer == PhotonNetwork.LocalPlayer)
            {
                _player01_Result_Text.text = RSP.ToString();

            }
            else if(targetPlayer != PhotonNetwork.LocalPlayer)
            {
                _player02_Result_Text.text = RSP.ToString();
            }

        }

        if (changedProps.ContainsKey("RSP_Result"))
        {
            RSP_Result isResult = (RSP_Result)changedProps["RSP_Result"];

            CheckAllPlayersRsult();
        }

    }

    // 플레이어 프로퍼티 저장
    public void SetPlayerProperties(bool isReady, RSPKind RSP)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps["Ready"] = isReady;
        playerProps["RSP"] = RSP;

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }
    public void SetPlayerResultProperties(RSP_Result result)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps["RSP_Result"] = result;

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    /// <summary>
    /// 프로퍼티 초기화
    /// </summary>
    void ResetPlayerProperties()
    {
        SetPlayerProperties(false, RSPKind.NoChoise);
        SetPlayerResultProperties(RSP_Result.None);
    }

    // 모두 레디 체크
    public void CheckAllPlayersReady()
    {
        if (_all__Player_Ready ) return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("Ready", out object isReadyObj) || !(bool)isReadyObj)
            {
                return;
            }
        }

        _all__Player_Ready = true;

        CancelTimer();
        StartCoroutine(DelayResultCalculation(1f));
        _select_UIs.SetActive(false);
        _result_UIs.SetActive(true);
    }

    IEnumerator DelayResultCalculation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        RPC_SetResult();
    }

    public void CheckAllPlayersRsult()
    {
        if (_RSP_end) return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("RSP_Result", out object resultObj))
            {
                return;
            }

            RSP_Result result = (RSP_Result)resultObj;

            if (result == RSP_Result.None)
            {
                return;
            }
        }

        _RSP_end = true;

        NextProgress(Setting_progress.Role);
        StartCoroutine(ResultUIOpenAfterDelay(5f));
    }

    #endregion

    void NextProgress(Setting_progress progress)
    {
        _now_progress = progress;
    }

    IEnumerator EnableButtonsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartSharedTimer();
        _buttonOn = true;
    }

    IEnumerator ResultUIOpenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _role_select_UIs.SetActive(true);
        _role_Manager.OpenUI();
        _map_select_UIs.SetActive(false);
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

    public void counting()
    {
        if (!timerRunning) return;

        double elapsed = PhotonNetwork.Time - startTime;

        if (elapsed >= _count)
        {
            timerRunning = false;
            _timer_Text.text = "가위, 바위, 보 지정 제한시간\n0";
            if (!_forceStopped)
            {
                RandomRSP();
            }
        }
        _timer_Text.text = $"가위, 바위, 보 지정 제한시간\n{Mathf.Max(0, (float)(_count - (PhotonNetwork.Time - startTime))):F0}";
    }

    [PunRPC]
    public void RPC_CancelTimer()
    {
        timerRunning = false;
        _timer_Text.text = "가위, 바위, 보 지정 제한시간\n0";
    }

    public void CancelTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _forceStopped = true;
            _myPhotonView.RPC("RPC_CancelTimer", RpcTarget.All);
        }
    }

    #endregion
}
