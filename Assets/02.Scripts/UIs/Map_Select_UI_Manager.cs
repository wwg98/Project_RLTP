using System.Collections;
using System.Collections.Generic;
using Define_Enums;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using DG.Tweening;

public class Map_Select_UI_Manager : MonoBehaviourPunCallbacks
{
    [Header("UI 참조")]
    public Text _message;
    public Button _small_Button;
    public Button _large_Button;
    public GameObject _image;
    public Role_Select_UI_Manager _Role_Select_manager;
    [Header("트랜스폼")]
    public GameObject Set_Image_Pos;
    public RectTransform _door03;
    public RectTransform _door04;
    public RectTransform _right2_Destination_Pos;
    public RectTransform _left2_Destination_Pos;


    private RSP_Result _gameResult;
    private RectTransform _imageRect;
    private RectTransform _endImageRect;
    private RectTransform _smallbuttonRect;
    private RectTransform _largebuttonRect;

    PhotonView _my_photon;
    bool _is_map_Select = false;
    bool _move_end = false;
    bool _sceneLoadingStarted = false;

    double startTime;
    bool timerRunning;
    bool _forceStopped = false;
    float _duration = 0.8f;
    int slerect_Map;

    private void Update()
    {
        if(_move_end)
        {
            counting();
        }
    }

    public void OpenUI()
    {
        gameObject.SetActive(true);
        _my_photon = GetComponent<PhotonView>();
        _gameResult = Game_Data_manager.Instance.RSPResult;


        if (_gameResult == RSP_Result.draw)
        {
            _message.text = "맵이 자동으로 선택될것입니다.";
            if (PhotonNetwork.IsMasterClient)
            {
                RandomMap();
            }
            _forceStopped = true;
        }
        else
        {
            _message.text = "승자는 맵을 선택해 주세요.";
            StartSharedTimer();
        }
        _small_Button.onClick.AddListener(OnClickSmallMap);
        _large_Button.onClick.AddListener(OnclickLargeMap);

        UISetting();
    }

    #region[맵선택]
    void RandomMap()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            int mapNum = Random.Range(1, 3);
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["SelectedMap"] = mapNum;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

    }

    void OnClickSmallMap()
    {
        if (_is_map_Select == false)
        {
            if(_gameResult == RSP_Result.Win)
            {
                _is_map_Select = true;
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props["SelectedMap"] = 1;
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                _forceStopped = true;
                CancelTimer();
            }

        }
    }

    void OnclickLargeMap()
    {
        if (_is_map_Select == false)
        {
            if (_gameResult == RSP_Result.Win)
            {
                _is_map_Select = true;
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props["SelectedMap"] = 2;
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                _forceStopped = true;
                CancelTimer();
            }
        }
    }
    #endregion

    void UISetting()
    {
        _imageRect = _image.gameObject.GetComponent<RectTransform>();
        _endImageRect = Set_Image_Pos.GetComponent<RectTransform>();
        _smallbuttonRect = _small_Button.GetComponent<RectTransform>();
        _largebuttonRect = _large_Button.GetComponent<RectTransform>();
        _smallbuttonRect.localScale = Vector3.zero;
        _largebuttonRect.localScale = Vector3.zero;

        DG.Tweening.Sequence MapSeq = DOTween.Sequence();

        MapSeq.Join(_imageRect.DOAnchorPos(_endImageRect.anchoredPosition, _duration));

        MapSeq.OnComplete(() =>
        {
            DG.Tweening.Sequence scaleSeq = DOTween.Sequence();
            scaleSeq.Join(_smallbuttonRect.DOScale(new Vector3(1, 1, 1), _duration));
            scaleSeq.Join(_largebuttonRect.DOScale(new Vector3(1, 1, 1), _duration));

            scaleSeq.OnComplete(() =>
            {
                _move_end = true;
            });

        });
    }

    void CloseDoor()
    {
        StartCoroutine(WaitBeforeClosingDoor(5f));
    }

    IEnumerator WaitBeforeClosingDoor(float delay)
    {
        yield return new WaitForSeconds(delay);

        DG.Tweening.Sequence doorCloseSequence2 = DOTween.Sequence();

        doorCloseSequence2.Join(_door03.DOAnchorPos(_right2_Destination_Pos.anchoredPosition, _duration));
        doorCloseSequence2.Join(_door04.DOAnchorPos(_left2_Destination_Pos.anchoredPosition, _duration));

        doorCloseSequence2.AppendInterval(1f);

        doorCloseSequence2.OnComplete(() =>
        {
            StartCoroutine(NextSceneAfterDelay(1f));
        });
    }


    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("SelectedMap"))
        {
            slerect_Map = (int)propertiesThatChanged["SelectedMap"];
            if(slerect_Map == 1)
            {
                _message.text = "작은 맵을 선택하셨습니다.";
            }
            else
            {
                _message.text = "큰 맵을 선택하셨습니다.";
            }

            CloseDoor();
        }
    }

    IEnumerator NextSceneAfterDelay(float delay)
    {
        if (_sceneLoadingStarted) yield break;
        _sceneLoadingStarted = true; 
        yield return new WaitForSeconds(delay);
        if (slerect_Map == 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Scenes_Controller_manager._instance.SetNowScene(ScenesKind.Small_Map_Scene);
                PhotonNetwork.LoadLevel((int)ScenesKind.Small_Map_Scene);
            }
        }
        else if (slerect_Map == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Scenes_Controller_manager._instance.SetNowScene(ScenesKind.Large_Map_Scene);
                PhotonNetwork.LoadLevel((int)ScenesKind.Large_Map_Scene);
            }
        }
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

        if (elapsed >= 10)
        {
            timerRunning = false;
            if (!_forceStopped)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    RandomMap();
                }
            }
        }
    }

    [PunRPC]
    public void RPC_CancelTimer()
    {
        timerRunning = false;
    }

    public void CancelTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _forceStopped = true;
            _my_photon.RPC("RPC_CancelTimer", RpcTarget.All);
        }
    }

    #endregion
}
