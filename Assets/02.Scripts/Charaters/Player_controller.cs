using System.Collections;
using UnityEngine;
using Define_Enums;
using Photon.Pun;
using TMPro;
using Photon.Pun.Demo.Procedural;
using DG.Tweening;
using UnityEngine.Windows;
using UnityEngine.InputSystem.XR;

public class Player_Controller : MonoBehaviourPunCallbacks
{
    [Header("캐릭터 스테이터스")]
    public float _move_Speed;
    public float _turn_Speed;
    public float _cool_time;
    [Header("카메라 회전")]
    public Transform CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;
    [Header("플레이어 능력치")]
    public int _speed_Lv = 1;
    public int _cooldown_LV = 1;
    public int _active_LV = 1;
    public int _power_LV = 1;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;
    private bool IsCurrentDeviceMouse = true;

    [Header("포지션 참조")]
    public Transform _rootZone;
    [Header("컴포넌트")]
    public Animator _my_Animator;
    public CharacterController _myController;
    public PhotonView _my_PhotonView;
    public Player_Interface_Manager _interface_Manager;

    public int _hash_IsAttack;
    public int _hash_IsRoll;

    public bool _isAttack = false;
    public bool _isRoll = false;
    public bool _is_Dead = false;
    public bool _on_Action = false;
    public bool _dontMove { get; set; } = false;
    public bool _Control { get; set; } = false;
    [HideInInspector] public Anistate _nowState;
    [HideInInspector] public Anistate _previousState;
    public Role _myRole { get; set; }
    public BoxCollider[] _atckZoneList;
    public In_Game_Manger _game_Manger;
    private CursorLockMode _cursorLock;
    private float _inputZ, _inputX;
    private Transform _cameraTransform;
    private float _verticalVelocity;
    private float _turnVelocity;
    private float _currentSpeed;
    public float TurnSmoothTime = 0.1f;
    public float SpeedChangeRate = 10.0f;
    private Vector2 _lookInput;
    private bool _is_hit = false;

    enum CursorLockState { Lock, Confine, None };

    public virtual void Start()
    {
        _game_Manger = GameObject.Find("In_Game_manager").GetComponent<In_Game_Manger>();
        _my_PhotonView = GetComponent<PhotonView>();
        _my_Animator = GetComponent<Animator>();
        _myController = GetComponent<CharacterController>();
        _cameraTransform = Camera.main?.transform;
        _Control = false;
        _dontMove = true;
        _nowState = Anistate.Idel;

        if (_interface_Manager == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Interface");
            _interface_Manager = go.GetComponentInParent<Player_Interface_Manager>();
        }
        else
        {
            return;
        }

        _hash_IsAttack = Animator.StringToHash("IsAttack");
        _hash_IsRoll = Animator.StringToHash("IsRoll");
    }
    public virtual void Update()
    {
        if (_my_PhotonView.IsMine)
        {
            if (_is_Dead) return;

            if (!_Control)
                return;

            Inputclick();

            if (_dontMove || _on_Action) return;

            Moveinput();

            MoveProgress(_inputZ, _inputX);
        }
    }
    void LateUpdate()
    {
        if (_my_PhotonView.IsMine)
        {
            if (_is_Dead) return;

            if (!_Control)
                return;

            CameraRotation();
        }
    }

    #region[움직임카메라 회전]
    void Moveinput()
    {
        _inputX = UnityEngine.Input.GetAxisRaw("Horizontal");
        _inputZ = UnityEngine.Input.GetAxisRaw("Vertical");
        
        Vector3 inputDir = new Vector3(_inputX, 0, _inputZ).normalized;
        float targetSpeed = inputDir == Vector3.zero ? 0f : _move_Speed;
        float horizontalSpeed = new Vector3(_myController.velocity.x, 0, _myController.velocity.z).magnitude;
        float speedOffset = 0.1f;
        if (Mathf.Abs(horizontalSpeed - targetSpeed) > speedOffset)
        {
            _currentSpeed = Mathf.Lerp(horizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _currentSpeed = Mathf.Round(_currentSpeed * 1000f) / 1000f;
        }
        else
        {
            _currentSpeed = targetSpeed;
        }

        if (inputDir != Vector3.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _turnVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        Vector3 moveDir = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;
        _myController.Move(moveDir.normalized * (_currentSpeed * Time.deltaTime) +
                        new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }
    private void CameraRotation()
    {
        float mouseX = UnityEngine.Input.GetAxis("Mouse X");
        float mouseY = UnityEngine.Input.GetAxis("Mouse Y");

        _lookInput = new Vector2(mouseX, mouseY);
        
        if (_lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _lookInput.x * deltaTimeMultiplier;
            _cinemachineTargetPitch -= _lookInput.y * deltaTimeMultiplier; // 마우스 위 = 아래 보기
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw,
            0.0f
        );
    }
    private float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    void MoveProgress(float forward, float right)
    {
        if (Mathf.Approximately(forward, 0f) && Mathf.Approximately(right, 0f))
        {
            ChangeAnimation(Anistate.Idel);
        }
        else
        {
            ChangeAnimation(Anistate.Run);
        }
    }
    void Inputclick()
    {

        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            if (_on_Action || _is_Dead) return;

            _on_Action = true;
            _dontMove = true;

            RotateToCameraDirection();

            OnAction();
        }
    }

    void RotateToCameraDirection()
    {
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;

        if (camForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = targetRotation;
        }
    }
    #endregion

    public virtual void ChangeAnimation(Anistate newState)
    {
        if (_nowState == newState) return;
        _my_Animator.SetInteger("AniState", (int)newState);
        _previousState = _nowState;
        _nowState = newState;
    }

    #region[종료 애니메이션]
    void AttackStart()
    {
        EnableAttackZone();
    }
    void AttackEnd()
    {
        _isAttack = false;
        _dontMove = false;
        _on_Action = false;

        foreach (var zone in _atckZoneList)
        {
            zone.enabled = false;
        }
        ChangeAnimation(_previousState);
    }
    [PunRPC]
    public void RPC_RollStart(Vector3 direction)
    {
        float dashPower = 1f;
        _myController.Move(direction.normalized * dashPower);
        StartCoroutine(RollMove());
    }
    void RollStart()
    {
        if (photonView.IsMine)
        {
            Vector3 dashDirection = transform.forward;
            photonView.RPC("RPC_RollStart", RpcTarget.All, dashDirection);
        }
    }
    [PunRPC]
    public void RPC_RollEnd()
    {
        _isRoll = false;
        _dontMove = false;
        _on_Action = false;
        ChangeAnimation(_previousState);
    }
    void RollEnd()
    {
        photonView.RPC("RPC_RollEnd", RpcTarget.All);
    }
    [PunRPC]
    public void RPC_Dead()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int foxScore = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("FoxScore") ?
                            (int)PhotonNetwork.CurrentRoom.CustomProperties["FoxScore"] : 0;

            if (foxScore < 2)
            {
                Debug.Log("[RPC] 점수 증가! 현재 FoxScore: " + foxScore);
                _game_Manger.UpdateFoxScore(false);
            }
            else
            {
                Debug.Log("[RPC] 점수 증가 막음! 이미 2점 도달");
            }
        }
    }

    void Dead()
    {
        if (PhotonNetwork.IsMasterClient && !_is_hit)
        {
            _is_hit = true;
            Debug.Log("[애니메이션 이벤트] Dead() 호출됨! (마스터 클라이언트에서만 실행)");
            photonView.RPC("RPC_Dead", RpcTarget.All);
        }
    }
    #endregion

    public void SetRole(Role myRole)
    {
        _myRole = myRole;
    }

    [PunRPC]
    public void RPC_EnableAttackZone()
    {
        foreach (var zone in _atckZoneList)
        {
            zone.enabled = true;
        }
    }
    public void EnableAttackZone()
    {
        photonView.RPC("RPC_EnableAttackZone", RpcTarget.All);
    }
    IEnumerator RollMove()
    {
        float rollDuration = 0.6f;
        float rollSpeed = 4f;

        float elapsed = 0f;
        Vector3 moveDir = transform.forward;

        while (elapsed < rollDuration)
        {
            _myController.Move(moveDir * rollSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    #region[역활별 자식 상속]
    public virtual void Hitting()
    {
        // 역활별 자식 상속
    }
    public virtual void Hacking()
    {
        // 역활별 자식 상속
    }
    protected virtual void OnAction()
    {

    }
    public virtual void Active_Effect()
    {

    }
    #endregion

    public void MoveControlle(bool move)
    {
        _Control = move;
    }
    public void VIctoy_Pose()
    {
        _dontMove = true;
        _Control = false;
        ChangeAnimation(Anistate.Victory);
    }
    public void GetCardBuff(int cardnem)
    {
        //CardData data = GameTableManager._instance.GetCardDataByID(CardID);
        switch (cardnem)
        {
            case 0:
                _speed_Lv++;
                break;
            case 1:
                _cooldown_LV++;
                break;
            case 2:
                _power_LV++;
                break;
            case 3:
                _active_LV++;
                break;
        }
    }
    public void CursorLock(bool visible)
    {
        switch (_cursorLock)
        {
            case CursorLockMode.Locked:
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case CursorLockMode.Confined:
                Cursor.lockState = CursorLockMode.Confined;
                break;
            case CursorLockMode.None:
                Cursor.lockState = CursorLockMode.None;
                break;
            default:
                break;
        }

        Cursor.visible = visible; 
    }
}

