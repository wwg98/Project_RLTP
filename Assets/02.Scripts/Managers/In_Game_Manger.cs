using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Define_Enums;
using Photon.Pun;
using Cinemachine;
using UnityEngine.UI;
using System;
using Photon.Realtime;
using System.Diagnostics.Tracing;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using ExitGames.Client.Photon.StructWrapping;
using System.Data;
using UnityEditor;
using System.Threading;


public class In_Game_Manger : MonoBehaviourPunCallbacks
{
    [Header("생성 위치")]
    public Transform fox_Spawn_rot;
    public Transform rabbit_Spawn_rot;
    [Header("버츄얼 카메라")]
    public CinemachineVirtualCamera _rabbitCam;
    public CinemachineVirtualCamera _foxCam;
    public CinemachineVirtualCamera _rabbit_WinCam;
    public CinemachineVirtualCamera _foxCam_WinCam;
    [Header("UI")]
    public Text _timer_Txt;
    public Text _in_Game_Time_Txt;
    public int _countDuration = 10;
    public int _inGameTime;
    public Text _turn_player_Txt;
    public Text _roun_txt;
    public UnityEngine.UI.Image _crosshair;
    public UnityEngine.UI.Button _card01;
    public UnityEngine.UI.Button _card02;
    public UnityEngine.UI.Button _card03;
    public UnityEngine.UI.Button _card04;
    [Header("오브젝트")]
    public GameObject _timer_Box;
    public GameObject _result_Box;
    public GameObject _exit_Gates;
    public Player_Interface_Manager _interface_manager;
    public Card_Selection_UI_Manager _card_Selection_UI_Manager;
    public PhotonView _my_photonView;
    public DoorMove DoorMove;
    double startTime;
    bool timerRunning;
    bool _forceStopped = false;
    public SampleCard01 SampleCard01;
    public SampleCard02 SampleCard02;



    Role role;
    RSP_Result RSP_Result;
    Round_Result Round_Result;
    Player_Controller player_Controller;
    public string winnerName { get; set; }
    public string loserName { get; set; }
    GameBattleProgress _now_Progress;
    int _round;
    int _turn;
    int _count;
    private int lastTurn = -1;
    bool _canInput = true;
    bool _choice_end = false;
    bool _is_cuser = false;
    private bool _randomCardSelected = false;
    private bool _isSlowMotionRunning = false;
    private CursorLockMode _cursorLock;

    GameObject[] _exitList;
    CardBase[] Cards;
    bool[] Card_highlights;


    private void Start()
    {
        role = Game_Data_manager.Instance.Role;
        RSP_Result = Game_Data_manager.Instance.RSPResult;
        Cards = _card_Selection_UI_Manager.GetCards();
        Card_highlights = new bool[Cards.Length];
        for (int i = 0; i < Cards.Length; i++)
        {
            Card_highlights[i] = false;
            OutlineOff(Cards[i].gameObject);
        }

        _interface_manager.ColorReset();

        _exitList = new GameObject[_exit_Gates.transform.childCount];
        for (int i = 0; i < _exitList.Length; i++)
        {
            _exitList[i] = _exit_Gates.transform.GetChild(i).gameObject;
            CinemachineVirtualCamera gateCam = _exitList[i].transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
            gateCam.gameObject.SetActive(false);
            _exitList[i].SetActive(false);
        }


        Player winner = null;
        Player loser = null;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("RSP_Result", out object resultObj))
            {
                if ((RSP_Result)resultObj == RSP_Result.Win)
                {
                    winner = player;
                }
                else
                {
                    loser = player;
                }
            }
        }

        if (winner != null) winnerName = winner.NickName;
        if (loser != null) loserName = loser.NickName;
        _turn_player_Txt.text = "";

        if (PhotonNetwork.IsMasterClient)
        {
            ResetGameProperties();
        }

        Player_Spawn();

        _roun_txt.text = _round.ToString();

        if (PhotonNetwork.IsMasterClient)
        {
            _my_photonView.RPC("OpenUI_RPC", RpcTarget.All);
        }

        _my_photonView.RPC("RPC_SetCursor", RpcTarget.All, false);
        _card01.interactable = false;
        _card02.interactable = false;
        _card03.interactable = false;
        _card04.interactable = false;

        changecard(winnerName);
    }

    private void Update()
    {
        if(!_is_cuser)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _is_cuser = true;
                playercuser(true);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _is_cuser = false;
                playercuser(false);
            }
        }

        switch (_now_Progress)
        {
            case GameBattleProgress.BUFF_CHOICE:
                if (RSP_Result != RSP_Result.draw)
                {
                    counting();
                }
                switch (_round)
                {
                    case 1:
                        if (RSP_Result != RSP_Result.draw)
                        {
                            switch (_turn)
                            {
                                case 1:
                                    _turn_player_Txt.text = winnerName;
                                    if (RSP_Result == RSP_Result.Win)
                                    {
                                        ChoiceCard();
                                    }
                                    break;
                                case 2:
                                case 3:
                                    _turn_player_Txt.text = loserName;
                                    if (RSP_Result == RSP_Result.Defeat)
                                    {
                                        ChoiceCard();
                                    }
                                    break;
                                case 4:
                                    if (!_choice_end)
                                    {
                                        _choice_end = true;
                                        if (PhotonNetwork.IsMasterClient)
                                        {
                                            _my_photonView.RPC("UIUP_RPC", RpcTarget.All);
                                        }
                                    }
                                    break;
                            }
                        }
                        else if (RSP_Result == RSP_Result.draw)
                        {
                            switch (_turn)
                            {
                                case 1:
                                    _turn_player_Txt.text = loserName;
                                    if (PhotonNetwork.IsMasterClient && !_randomCardSelected && _turn != lastTurn)
                                    {
                                        _randomCardSelected = true;
                                        lastTurn = _turn;
                                        int randomCardIndex = UnityEngine.Random.Range(0, Cards.Length);
                                        _my_photonView.RPC("RPC_selectCard", RpcTarget.All, randomCardIndex, true);
                                    }
                                    break;

                                case 2:
                                    _turn_player_Txt.text = winnerName;
                                    if (PhotonNetwork.IsMasterClient && !_randomCardSelected && _turn != lastTurn)
                                    {
                                        _randomCardSelected = true;
                                        lastTurn = _turn;
                                        int randomCardIndex = UnityEngine.Random.Range(0, Cards.Length);
                                        _my_photonView.RPC("RPC_selectCard", RpcTarget.All, randomCardIndex, true);
                                    }
                                    break;

                                case 3:
                                    if (!_choice_end)
                                    {
                                        _choice_end = true;
                                        if (PhotonNetwork.IsMasterClient)
                                        {
                                            _my_photonView.RPC("UIUP_RPC", RpcTarget.All);
                                        }
                                    }
                                    break;
                            }
                        }

                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        switch (_turn)
                        {
                            case 1:
                                _turn_player_Txt.text = loserName;
                                if (Round_Result == Round_Result.defeat)
                                {
                                    ChoiceCard();
                                }
                                break;
                            case 2:
                                _turn_player_Txt.text = winnerName;
                                if (Round_Result == Round_Result.Win)
                                {
                                    ChoiceCard();
                                }
                                break;
                            case 3:
                                if (!_choice_end)
                                {
                                    _choice_end = true;
                                    if (PhotonNetwork.IsMasterClient)
                                    {
                                        _my_photonView.RPC("UIUP_RPC", RpcTarget.All);
                                    }
                                }
                                break;
                        }
                        break;
                }
                break;
            case GameBattleProgress.INGAME:
                counting();
                _roun_txt.text = _round.ToString();
                break;
            case GameBattleProgress.VICTORY:
                break;
        }
    }


    #region[플레이어 스폰]
    void Player_Spawn()
    {
        GameObject instance;
        CinemachineVirtualCamera vcam = null;

        if (role == Role.Rabbit)
        {
            instance = PhotonNetwork.Instantiate("Characters/Rabbit_player", rabbit_Spawn_rot.position, rabbit_Spawn_rot.rotation);
            if (instance.GetComponent<PhotonView>().IsMine)
                player_Controller = instance.GetComponent<Rabbit_Controller>();
            vcam = _rabbitCam;
        }
        else if (role == Role.Fox)
        {
            instance = PhotonNetwork.Instantiate("Characters/Fox_player", fox_Spawn_rot.position, fox_Spawn_rot.rotation);
            if (instance.GetComponent<PhotonView>().IsMine)
                player_Controller = instance.GetComponent<Fox_Controller>();
            vcam = _foxCam;
        }

        if (vcam != null)
        {
            if (role == Role.Rabbit)
            {
                vcam.Follow = GameObject.FindGameObjectWithTag("Rabbit_Head").transform;
                vcam.LookAt = GameObject.FindGameObjectWithTag("Rabbit_Head").transform;
            }
            else if (role == Role.Fox)
            {
                vcam.Follow = GameObject.FindGameObjectWithTag("Fox_Head").transform;
                vcam.LookAt = GameObject.FindGameObjectWithTag("Fox_Head").transform;
            }
            vcam.gameObject.SetActive(true);
            foreach (var cam in FindObjectsOfType<CinemachineVirtualCamera>())
            {
                if (cam != vcam)
                    cam.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("카메라가 null이 아님");
        }
    }

    public void RespawnPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_RespawnPlayer", RpcTarget.All);
        }
    }
    [PunRPC]
    public void RPC_RespawnPlayer()
    {
        if (player_Controller != null && player_Controller.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(player_Controller.gameObject);    
            player_Controller = null;
        }

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(2f);

        Player_Spawn();
    }
    #endregion

    #region[카드 선택]
    [PunRPC]
    void RPC_selectCard(int num, bool force)
    {
        selectCard(num, force);
    }
    void selectCard(int num, bool force)
    {
        if (!_canInput) return;

        RectTransform selectedCardRect = Cards[num].gameObject.GetComponent<RectTransform>();

        for (int i = 0; i < Cards.Length; i++)
        {
            if (i != num)
            {
                OutlineOff(Cards[i].gameObject);
                Card_highlights[i] = false;
            }
        }

        if (!Card_highlights[num])
        {
            OutlineOn(Cards[num].gameObject);
            Card_highlights[num] = true;

            if (force)
            {
                _canInput = false;
                OutlineOff(Cards[num].gameObject);
                _card_Selection_UI_Manager.CardSelect(selectedCardRect);
            }
        }
        else
        {
            _canInput = false;
            OutlineOff(Cards[num].gameObject);
            CancelTimer();
            CardBase cardBase = Cards[num].gameObject.GetComponent<CardBase>();
            _card_Selection_UI_Manager.CardSelect(selectedCardRect);
        }
    }

    [PunRPC]
    public void RPC_cardchoice(int cardNum)
    {
        player_Controller.GetCardBuff(cardNum);
    }

    void OutlineOn(GameObject Card)
    {
        Outline cardOutline = Card.transform.GetChild(0).gameObject.GetComponent<Outline>();
        cardOutline.enabled = true;
    }
    void OutlineOff(GameObject Card)
    {
        Outline cardOutline = Card.transform.GetChild(0).gameObject.GetComponent<Outline>();
        cardOutline.enabled = false;
    }
    void ChoiceCard()
    {
        if (_canInput)
        {
            _card01.interactable = true;
            _card02.interactable = true;
            _card03.interactable = true;
            _card04.interactable = true;

            // 기존 리스너 제거 (중복 방지)
            _card01.onClick.RemoveAllListeners();
            _card02.onClick.RemoveAllListeners();
            _card03.onClick.RemoveAllListeners();
            _card04.onClick.RemoveAllListeners();

            // 새 리스너 연결
            _card01.onClick.AddListener(() => OnclickCard(0));
            _card02.onClick.AddListener(() => OnclickCard(1));
            _card03.onClick.AddListener(() => OnclickCard(2));
            _card04.onClick.AddListener(() => OnclickCard(3));
        }
    }
    public void SelectRandomCard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int randomCardIndex = UnityEngine.Random.Range(0, Cards.Length);

            if (randomCardIndex < 0 || randomCardIndex >= Cards.Length)
            {
                Debug.LogError("랜덤 카드 인덱스가 범위를 벗어났습니다!");
                return;
            }
            _canInput = false;
            _my_photonView.RPC("RPC_SelectCardAndSync", RpcTarget.All, randomCardIndex);
        }
    }

    [PunRPC]
    void RPC_SelectCardAndSync(int num)
    {
        selectCard(num, true);
    }

    #endregion

    #region[다음턴]
    public void NextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Updateturn();
            StartSharedTimer();
        }
        _canInput = true;
        _randomCardSelected = false;

        for (int i = 0; i < Cards.Length; i++)
        {
            Card_highlights[i] = false;
            OutlineOff(Cards[i].gameObject);
        }
    }
    public void  NextRound()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateRound();
            RespawnPlayer();
            ResetGameProperties();
        }
        changecard(loserName);
        _canInput = true;
        _randomCardSelected = false;
        _choice_end = false;

        for (int i = 0; i < Cards.Length; i++)
        {
            Card_highlights[i] = false;
            OutlineOff(Cards[i].gameObject);
        }
    }
    #endregion

    #region
    [PunRPC]
    void OpenUI_RPC()
    {
        _card_Selection_UI_Manager.OpenUI();
    }

    [PunRPC]
    void UIUP_RPC()
    {
        _card_Selection_UI_Manager.Card_Gather();
    }
    #endregion

    #region[프로퍼티]
    public void ResetGameProperties()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        if (_now_Progress == GameBattleProgress.OPEN)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                {
                    { "Round", 1},
                    { "Turn", 1 },
                    { "RabbitScore", 0 },
                    { "FoxScore", 0 },
                    { "FoxRoundScore" , 0},
                    { "RabbitRoundScore" , 0}
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                {
                    { "Turn", 1 },
                    { "RabbitScore", 0 },
                    { "FoxScore", 0 }
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

        if (propertiesThatChanged.ContainsKey("GameProgress"))
        {
            _now_Progress = (GameBattleProgress)propertiesThatChanged["GameProgress"];
            print(_now_Progress);
            if (_now_Progress == GameBattleProgress.BUFF_CHOICE)
            {
                _my_photonView.RPC("RPC_SetCursor", RpcTarget.All, true);
                if (PhotonNetwork.IsMasterClient)
                {
                    StartSharedTimer();
                }
                if (DoorMove != null)
                {
                    DoorMove.StopDoorRoutine();
                }
            }
            else if (_now_Progress == GameBattleProgress.INGAME)
            {
                _my_photonView.RPC("RPC_SetCursor", RpcTarget.All, false);
                StartSharedTimer();
                player_Controller.MoveControlle(true);
                for (int i = 0; i < _exitList.Length; i++)
                {
                    CinemachineVirtualCamera gateCam = _exitList[i].transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
                    gateCam.gameObject.SetActive(false);
                    _exitList[i].SetActive(false);
                }
                if (DoorMove != null)
                {
                    DoorMove.PlayDoors();
                }
            }
        }

        if (propertiesThatChanged.ContainsKey("Round"))
        {
            _round = (int)propertiesThatChanged["Round"];
            print("라운드" + _round);
        }

        if (propertiesThatChanged.ContainsKey("Turn"))
        {
            _turn = (int)propertiesThatChanged["Turn"];
            print("턴" + _turn);
        }

        if (propertiesThatChanged.ContainsKey("FoxScore") || propertiesThatChanged.ContainsKey("RabbitScore"))
        {
            int foxScore = propertiesThatChanged.ContainsKey("FoxScore") ? (int)propertiesThatChanged["FoxScore"] : 0;
            int rabbitScore = propertiesThatChanged.ContainsKey("RabbitScore") ? (int)propertiesThatChanged["RabbitScore"] : 0;

            print($"여우 : " + foxScore + "토끼 : " + rabbitScore);

            if (foxScore == 2 || rabbitScore == 1)
            {
                Role currentRole = Game_Data_manager.Instance.Role;
                string playerName = Game_Data_manager.Instance.PlayerName;

                Player fox = null;
                Player rabbit = null;
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    if (player.CustomProperties.TryGetValue("Role", out object roleObj))
                    {
                        Role playerRole = (Role)roleObj;

                        if (playerRole == Role.Fox)
                        {
                            fox = player;
                        }
                        else if (playerRole == Role.Rabbit)
                        {
                            rabbit = player;
                        }
                    }
                }

                if (foxScore == 2)
                {
                    if (currentRole == Role.Fox)
                    {
                        Round_Result = Round_Result.Win;
                        winnerName = fox?.NickName;
                        loserName = rabbit?.NickName;
                    }
                    else if (currentRole == Role.Rabbit)
                    {
                        Round_Result = Round_Result.defeat;
                        winnerName = fox?.NickName;
                        loserName = rabbit?.NickName;
                    }
                    if(PhotonNetwork.IsMasterClient)
                    {
                        UpdateFoxRoundScore();
                    }
                }
                else if (rabbitScore == 1)
                {
                    if (currentRole == Role.Rabbit)
                    {
                        Round_Result = Round_Result.Win;
                        winnerName = rabbit?.NickName;
                        loserName = fox?.NickName;
                    }
                    else if (currentRole == Role.Fox)
                    {
                        Round_Result = Round_Result.defeat;
                        winnerName = rabbit?.NickName;
                        loserName = fox?.NickName;
                    }
                    if (PhotonNetwork.IsMasterClient)
                    {
                        UpdateRabbitRoundScore();
                    }
                }

                player_Controller.MoveControlle(false);
                CancelTimer();
            }
            else if (foxScore == 1)
            {
                CancelTimer();
                player_Controller.MoveControlle(false);
                RespawnPlayer();
                _card_Selection_UI_Manager.ScoreCheck();
            }
        }

        if (propertiesThatChanged.ContainsKey("FoxRoundScore") || propertiesThatChanged.ContainsKey("RabbitRoundScore"))
        {
            int foxRoundScore = (int)PhotonNetwork.CurrentRoom.CustomProperties["FoxRoundScore"];
            int rabbitRoundScore = (int)PhotonNetwork.CurrentRoom.CustomProperties["RabbitRoundScore"];

            print($"라운드 여우 :" + foxRoundScore + "라운드 토끼 : " + rabbitRoundScore);

            if (foxRoundScore == 2 || rabbitRoundScore == 2)
            {
                if (foxRoundScore == 2)
                {
                    Game_Data_manager.Instance.WinnerRole = Role.Fox;
                }
                else if (rabbitRoundScore == 2)
                {
                    Game_Data_manager.Instance.WinnerRole = Role.Rabbit;
                }
                _card_Selection_UI_Manager.RsultOn();
            }
             else if(foxRoundScore != 0 || rabbitRoundScore != 0)
            {
                _card_Selection_UI_Manager.NextRoundchoice();
                if (foxRoundScore != 0)
                {
                    _interface_manager.GetFoxScore();
                }
                else if(rabbitRoundScore != 0)
                {
                    _interface_manager.GetRabbitScore();
                }
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("ChoiceCard"))
        {
            int cardNum = (int)changedProps["ChoiceCard"];
            _my_photonView.RPC("RPC_cardchoice", RpcTarget.OthersBuffered, cardNum);
        }
    }

    public void UpdateRound()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        int currentScore = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Round") ?
                            (int)PhotonNetwork.CurrentRoom.CustomProperties["Round"] : 0;

        props["Round"] = currentScore + 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public void Updateprogress(GameBattleProgress progress)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["GameProgress"] = progress;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public void Updateturn()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Turn"))
        {
            int currentTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties["Turn"];
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["Turn"] = currentTurn + 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        else
        {
            Debug.LogWarning("턴 값이 존재하지 않습니다. 기본값(1)으로 설정합니다.");
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["Turn"] = 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }
    public void UpdateRabbitScore()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        int currentScore = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RabbitScore") ?
                            (int)PhotonNetwork.CurrentRoom.CustomProperties["RabbitScore"] : 0;

        props["RabbitScore"] = currentScore + 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public void UpdateFoxScore(bool TimeOut)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        int currentScore = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("FoxScore") ?
                            (int)PhotonNetwork.CurrentRoom.CustomProperties["FoxScore"] : 0;

        props["FoxScore"] = TimeOut ? currentScore + 2 : currentScore + 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public void UpdateRabbitRoundScore()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        int currentScore = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RabbitRoundScore") ?
                            (int)PhotonNetwork.CurrentRoom.CustomProperties["RabbitRoundScore"] : 0;

        props["RabbitRoundScore"] = currentScore + 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public void UpdateFoxRoundScore()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        int currentScore = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("FoxRoundScore") ?
                            (int)PhotonNetwork.CurrentRoom.CustomProperties["FoxRoundScore"] : 0;

        props["FoxRoundScore"] = currentScore + 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
    public void UpdateTuneplayer(string name)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("TurnPlayerName"))
        {
            string currentTurnName = (string)PhotonNetwork.CurrentRoom.CustomProperties["TurnPlayerName"];
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["TurnPlayerName"] = name;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    public void UpdatedhoiceCard(int cardnum)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["ChoiceCard"] = cardnum;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    #endregion

    #region [타이머]
    [PunRPC]
    public void StartTimer_RPC(double startTime, int duration)
    {
        this.startTime = startTime;
        this._count = duration;
        timerRunning = true;
        _forceStopped = false;
    }

    public void StartSharedTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int duration = 0;

            double startTime = PhotonNetwork.Time;
            if (_now_Progress == GameBattleProgress.OPEN || _now_Progress == GameBattleProgress.BUFF_CHOICE)
            {
                duration = _countDuration;
            }
            else if (_now_Progress == GameBattleProgress.INGAME)
            {
                duration = _inGameTime;
            }

            photonView.RPC("StartTimer_RPC", RpcTarget.All, startTime, duration);
        }
    }

    public void counting()
    {
        if (!timerRunning) return;
        double elapsed = PhotonNetwork.Time - startTime;

        if (elapsed >= _count)
        {
            timerRunning = false;
            _timer_Txt.text = "0";
            _in_Game_Time_Txt.text = "0";

            if (!_forceStopped)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (_now_Progress == GameBattleProgress.BUFF_CHOICE)
                    {
                        if (PhotonNetwork.IsMasterClient && !_randomCardSelected && _turn != lastTurn)
                        {
                            _randomCardSelected = true;
                            lastTurn = _turn;
                            int randomCardIndex = UnityEngine.Random.Range(0, Cards.Length);
                            _my_photonView.RPC("RPC_selectCard", RpcTarget.All, randomCardIndex, true);
                        }
                    }
                    if (_now_Progress == GameBattleProgress.INGAME)
                    {
                        UpdateFoxScore(true);
                        print("타임 아웃");
                    }
                }
            }
        }
        _timer_Txt.text = $"{Mathf.Max(0, (float)(_count - (PhotonNetwork.Time - startTime))):F0}";
        _in_Game_Time_Txt.text = $"{Mathf.Max(0, (float)(_count - (PhotonNetwork.Time - startTime))):F0}";
    }

    [PunRPC]
    public void RPC_CancelTimer()
    {
        timerRunning = false;
        _timer_Txt.text = "0";
        _in_Game_Time_Txt.text = "0";
    }

    public void CancelTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _forceStopped = true;
            _my_photonView.RPC("RPC_CancelTimer", RpcTarget.All);
        }
    }
    #endregion

    #region[탈출구 오픈]
    [PunRPC]
    public void RPC_ExitOpen(int gatenum)
    {
        player_Controller.MoveControlle(false);
        _exitList[gatenum].gameObject.SetActive(true);
        CinemachineVirtualCamera gateCam = _exitList[gatenum].transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
        gateCam.gameObject.SetActive(true);

        StartCoroutine(ExitgateOpenEnd(gateCam));
    }

    private IEnumerator ExitgateOpenEnd(CinemachineVirtualCamera gateCam)
    {
        yield return new WaitForSecondsRealtime(2f);

        gateCam.gameObject.SetActive(false);
        player_Controller.MoveControlle(true);
    }

    public void ExitOpen()
    {
            int randomGate = UnityEngine.Random.Range(0, _exitList.Length);
            _my_photonView.RPC("RPC_ExitOpen", RpcTarget.All, randomGate);
    }
    #endregion

    public void playermoveOn()
    {
        player_Controller.MoveControlle(true);
    }

    [PunRPC]
    void RPC_SetCursor(bool visible)
    {
        playercuser(visible);
    }
    public void playercuser(bool visible)
    {
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            player_Controller.CursorLock(true);
            _crosshair.gameObject.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            player_Controller.CursorLock(false);
            _crosshair.gameObject.SetActive(true);
        }
    }
    public void changecard(string name)
    {
        SampleCard01.Chagecard(name);
        SampleCard02.Chagecard(name);
    }

    public void OnclickCard(int index)
    {
        _my_photonView.RPC("RPC_selectCard", RpcTarget.All, index, false);

        // 선택 후 버튼 비활성화
        _card01.interactable = false;
        _card02.interactable = false;
        _card03.interactable = false;
        _card04.interactable = false;
    }
    #region[슬로우 모션]
    [PunRPC]
    private void RPC_StartSlowMotion()
    {
        if (!_isSlowMotionRunning)
        {
            StartCoroutine(SlowMotionCoroutine());
        }
    }
    public void StartSlowMotion()
    {
        if (_isSlowMotionRunning) return;

        if (PhotonNetwork.IsMasterClient)
        {
            _my_photonView.RPC("RPC_StartSlowMotion", RpcTarget.All);
        }
    }
    private IEnumerator SlowMotionCoroutine()
    {
        _isSlowMotionRunning = true;

        float baseTimeScale = Time.timeScale;
        float baseTimeFix = Time.fixedDeltaTime;

        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = baseTimeScale;
        Time.fixedDeltaTime = baseTimeFix;
        _isSlowMotionRunning = false;

    }
    #endregion
}
