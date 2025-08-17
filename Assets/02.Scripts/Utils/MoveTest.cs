using Define_Enums;
using Photon.Pun;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    [Header("캐릭터 스테이터스")]
    public float _move_Speed;
    public float _turn_Speed;
    public float _cool_time;
    [Header("포지션 참조")]
    public Transform _rootZone;
    [Header("컴포넌트")]
    public Animator _my_Animator;
    public CharacterController _myController;
    public PhotonView _my_PhotonView;
    public Player_Interface_Manager _interface_Manager;
    [Header("카메라 회전")]
    public Transform CinemachineCameraTarget;  // 시네머신 카메라가 따라갈 타겟
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;
    private bool IsCurrentDeviceMouse = true; // PC만 쓴다면 true 고정

    private Vector2 _lookInput;


    public int _hash_IsAttack;
    public int _hash_IsRoll;

    public bool _isAttack = false;
    public bool _isRoll = false;
    public bool _is_Dead = false;
    public bool _on_Action = false;
    public bool _dontMove { get; set; } = true;
    public bool _Control { get; set; } = false;
    [HideInInspector] public Anistate _nowState;
    [HideInInspector] public Anistate _previousState;
    public Role _myRole;
    public BoxCollider[] _atckZoneList;
    public In_Game_Manger _game_Manger;

    private float _inputZ, _inputX;
    private Vector3 MoveVec;

    private Transform _cameraTransform;
    private Vector2 _moveInput;
    private float _verticalVelocity;
    private float _turnVelocity;
    private float _currentSpeed;
    public float TurnSmoothTime = 0.1f;
    public float SpeedChangeRate = 10.0f;

    public virtual void Start()
    {
        _my_Animator = GetComponent<Animator>();
        _myController = GetComponent<CharacterController>();
        _Control = false;
        _dontMove = true;
        _nowState = Anistate.Idel;

        _hash_IsAttack = Animator.StringToHash("IsAttack");
        _hash_IsRoll = Animator.StringToHash("IsRoll");

        _cameraTransform = Camera.main?.transform;
    }

    public virtual void Update()
    {
        Moveinput();
    }
    void LateUpdate()
    {
        CameraRotation();
    }

    void Moveinput()
    {
        _inputX = UnityEngine.Input.GetAxisRaw("Horizontal");
        _inputZ = UnityEngine.Input.GetAxisRaw("Vertical");

        print(_inputZ);
        print(_inputX);
        
        Vector3 inputDir = new Vector3 (_inputX, 0 , _inputZ).normalized;
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

}
