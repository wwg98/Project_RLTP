using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Define_Enums;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using System.Linq;
using DG.Tweening;
using Photon.Pun.Demo.Cockpit.Forms;
using System.Data;

public class Role_Select_UI_Manager : MonoBehaviourPunCallbacks
{
    [Header("UI 참조")]
    public RectTransform _door01;
    public RectTransform _door02;
    public RectTransform _rabbitimage;
    public RectTransform _foximage;
    public RectTransform _rabbitKey;
    public RectTransform _foxKey;
    public RectTransform _timerBox;
    public RectTransform Rabbit_Crown;
    public RectTransform Fox_Crown;
    public RectTransform _door03;
    public RectTransform _door04;
    public Text _txt_Message;
    public Text _timer_txt;
    public Text Player01_name_txt;
    public Text Player02_name_txt;
    public float role_Select_cont = 10f;
    [Header("목표 포지션")]
    public RectTransform _right_Destination_Pos;
    public RectTransform _left_Destination_Pos;
    public RectTransform _right2_Destination_Pos;
    public RectTransform _left2_Destination_Pos;
    public RectTransform _rabbit_crown_Pos;
    public RectTransform _fox_crown_Pos;
    [Header("이동속도")]
    public float duration = 1.0f;
    [Header("스케일 속도")]
    public float duration_S = 1.0f;
    [Header("컴포넌트, 오브젝트")]
    public Map_Select_UI_Manager Map_Select_UI_Manager;
    public GameObject _center_Object;
    public Setting_UI_Manger _setting_UI_manager;
    public GameObject _name_Obj;
    public GameObject _name_Obj2;
    public GameObject _charimage;
    public GameObject _charimage2;
    public RectTransform _rabbit_tag;
    public RectTransform _fox_tag;

    double startTime;
    bool timerRunning;
    bool _is_input = false;
    bool _role_end = false;
    bool _is_all_select_Role = false;
    bool _forceStopped = false;
    PhotonView _photonView;
    Player otherPlayer;
    float _duration = 0.8f;
    RectTransform target;
    RectTransform target_crown_pos;
    Role rRole;
    Role Winer_Role;


    private void Update()
    {
        counting();

        if (_is_input == true)
        {
            RoleSelrect();
        }
    }

    public void OpenUI()
    {
        _photonView = PhotonView.Get(this);

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.IsLocal)
            {
                otherPlayer = p;
                break;
            }
        }

        Player01_name_txt.gameObject.SetActive(false);
        Player02_name_txt.gameObject.SetActive(false);
        Fox_Crown.gameObject.SetActive(false);
        Rabbit_Crown.gameObject.SetActive(false);

        _timer_txt.text = "역활 선택 시간";
        MoveDoor();
    }

    void MoveDoor()
    {
        DG.Tweening.Sequence doorCloseSequence = DOTween.Sequence();

        doorCloseSequence.Join(_door01.DOAnchorPos(_right_Destination_Pos.anchoredPosition, _duration));
        doorCloseSequence.Join(_door02.DOAnchorPos(_left_Destination_Pos.anchoredPosition, _duration));

        doorCloseSequence.AppendInterval(1f);

        doorCloseSequence.OnComplete(() =>
        {
            _rabbitimage.pivot = new Vector2(0.5f, 0.5f);
            _foximage.pivot = new Vector2(0.5f, 0.5f);
            _rabbitKey.pivot = new Vector2(0.5f, 0.5f);
            _foxKey.pivot = new Vector2(0.5f, 0.5f);
            _timerBox.pivot = new Vector2(0.5f, 0.5f);


            DG.Tweening.Sequence ScaleUpSequence = DOTween.Sequence();

            ScaleUpSequence.Join(_rabbitimage.DOScale(new Vector3(1f, 1f, 1f), duration_S)
                     .SetEase(Ease.OutBack));
            ScaleUpSequence.Join(_foximage.DOScale(new Vector3(1f, 1f, 1f), duration_S)
               .SetEase(Ease.OutBack));
            ScaleUpSequence.Join(_rabbitKey.DOScale(new Vector3(1f, 1f, 1f), duration_S)
           .SetEase(Ease.OutBack));
            ScaleUpSequence.Join(_foxKey.DOScale(new Vector3(1f, 1f, 1f), duration_S)
               .SetEase(Ease.OutBack));
            ScaleUpSequence.Join(_timerBox.DOScale(new Vector3(1f, 1f, 1f), duration_S)
            .SetEase(Ease.OutBack));

            ScaleUpSequence.OnComplete(() =>
            {
                StartSharedTimer();
                if (Game_Data_manager.Instance.RSPResult == RSP_Result.Win)
                {
                    _is_input = true;
                }
                else if (Game_Data_manager.Instance.RSPResult == RSP_Result.draw)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        RandomRole();
                    }
                }
            });
        });
    }
    [PunRPC]
    public void MoveCrown()
    {
        Player01_name_txt.gameObject.SetActive(true);
        Player02_name_txt.gameObject.SetActive(true);

        if (Winer_Role == Role.Rabbit)
        {
            Rabbit_Crown.gameObject.SetActive(true);
            target = Rabbit_Crown;
            target_crown_pos = _rabbit_crown_Pos;

        }
        else if (Winer_Role == Role.Fox)
        {
            Fox_Crown.gameObject.SetActive(true);
            target = Fox_Crown;
            target_crown_pos = _fox_crown_Pos;
        }

        DG.Tweening.Sequence CrownSequence = DOTween.Sequence();

        if (Game_Data_manager.Instance.RSPResult != RSP_Result.draw)
            CrownSequence.Join(target.DOAnchorPos(target_crown_pos.anchoredPosition, _duration));

        CrownSequence.AppendInterval(5f);

        CrownSequence.OnComplete(() =>
        {
            DG.Tweening.Sequence ScaleDownSeq = DOTween.Sequence();

            RectTransform name1 = Player01_name_txt.gameObject.GetComponent<RectTransform>();
            RectTransform name2 = Player02_name_txt.gameObject.GetComponent<RectTransform>();

            name1.pivot = new Vector2(0.5f, 0.5f);
            name2.pivot = new Vector2(0.5f, 0.5f);
            _center_Object.SetActive(false);

            ScaleDownSeq.Join(_timerBox.DOScale(new Vector3(0f, 0f, 0f), duration_S)
     .SetEase(Ease.OutBack));
            ScaleDownSeq.Join(_rabbitimage.DOScale(new Vector3(0f, 0f, 0f), duration_S)
.SetEase(Ease.OutBack));
            ScaleDownSeq.Join(_foximage.DOScale(new Vector3(0f, 0f, 0f), duration_S)
     .SetEase(Ease.OutBack));
            ScaleDownSeq.Join(_rabbitKey.DOScale(new Vector3(0f, 0f, 0f), duration_S)
.SetEase(Ease.OutBack));
            ScaleDownSeq.Join(_foxKey.DOScale(new Vector3(0f, 0f, 0f), duration_S)
     .SetEase(Ease.OutBack));
            ScaleDownSeq.Join(name1.DOScale(new Vector3(0f, 0f, 0f), duration_S)
     .SetEase(Ease.OutBack));
            ScaleDownSeq.Join(name2.DOScale(new Vector3(0f, 0f, 0f), duration_S)
     .SetEase(Ease.OutBack));
            ScaleDownSeq.Join(_rabbit_tag.DOScale(new Vector3(0f, 0f, 0f), duration_S)
.SetEase(Ease.OutBack));
            ScaleDownSeq.Join(_fox_tag.DOScale(new Vector3(0f, 0f, 0f), duration_S)
     .SetEase(Ease.OutBack));


            ScaleDownSeq.OnComplete(() =>
            {
                Player01_name_txt.gameObject.SetActive(false);
                Player02_name_txt.gameObject.SetActive(false);
                _rabbitimage.gameObject.SetActive(false);
                _foximage.gameObject.SetActive(false);
                _rabbitKey.gameObject.SetActive(false);
                _foxKey.gameObject.SetActive(false);
                _timerBox.gameObject.SetActive(false);
                Map_Select_UI_Manager.gameObject.SetActive(true);
                Map_Select_UI_Manager.OpenUI();
            });

        });
    }

    #region[역활선택]
    void RoleSelrect()
    {
        if (_role_end) return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            _role_end = true;
            SetPlayerRoleProperties(Role.Rabbit);
            SetWinerRoleProperties(Role.Rabbit);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            _role_end = true;
            SetPlayerRoleProperties(Role.Fox);
            SetWinerRoleProperties(Role.Fox);
        }

    }
    void RandomRole()
    {
        int randomRole = Random.Range(1, 3);
        switch (randomRole)
        {
            case 1:
                rRole = Role.Fox;
                break;
            case 2:
                rRole = Role.Rabbit;
                break;
        }
        SetPlayerRoleProperties(rRole);
        if (Game_Data_manager.Instance.RSPResult != RSP_Result.draw)
        {
            SetWinerRoleProperties(rRole);
        }
    }

    #endregion

    #region[프로퍼티]

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Role"))
        {
            Role selectedRole = (Role)changedProps["Role"];

            UpdatePlayerNameUI(targetPlayer, selectedRole);

            if (targetPlayer == PhotonNetwork.LocalPlayer)
            {
                Game_Data_manager.Instance.Role = selectedRole;
            }

            if (targetPlayer != PhotonNetwork.LocalPlayer && !_role_end)
            {
                Role myAutoRole = selectedRole == Role.Rabbit ? Role.Fox : Role.Rabbit;
                _role_end = true;
                SetPlayerRoleProperties(myAutoRole);
            }

            CheckAllPlayersSetRole();
        }
    }

    void UpdatePlayerNameUI(Player player, Role role)
    {
        if (role == Role.Rabbit)
        {
            Player01_name_txt.text = player.NickName;
        }
        else if (role == Role.Fox)
        {
            Player02_name_txt.text = player.NickName;
        }
    }


    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("WinerRole"))
        {
            Winer_Role = (Role)propertiesThatChanged["WinerRole"];
        }
    }

    public void SetPlayerRoleProperties(Role Role)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps["Role"] = Role;

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    public void SetWinerRoleProperties(Role role)
    {
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps["WinerRole"] = role;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public void CheckAllPlayersSetRole()
    {
        if (_is_all_select_Role) return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("Role", out object role))
            {
                return;
            }

            Role myrole = (Role)role;

            if (myrole == Role.None)
            {
                return;
            }
        }
        _is_all_select_Role = true;

        CancelTimer();
        _photonView.RPC("MoveCrown", RpcTarget.All);
    }
    #endregion


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

        if (elapsed >= role_Select_cont)
        {
            timerRunning = false;
            _timer_txt.text = "역활 선택 시간: 0";
            if (!_forceStopped)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    RandomRole();
                }
            }
        }
        _timer_txt.text = $"역활 선택 시간 : {Mathf.Max(0, (float)(role_Select_cont - (PhotonNetwork.Time - startTime))):F0}";
    }

    [PunRPC]
    public void RPC_CancelTimer()
    {
        timerRunning = false;
        _timer_txt.text = "역활 선택 시간: 0";
    }

    public void CancelTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _forceStopped = true;
            _photonView.RPC("RPC_CancelTimer", RpcTarget.All);
        }
    }

    #endregion
}
