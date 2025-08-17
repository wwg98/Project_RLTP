using System.Collections;
using System.Collections.Generic;
using Define_Enums;
using Photon.Pun;
using UnityEngine;

public class Rabbit_Controller : Player_Controller
{
    private bool _canHacking = false;
    public bool _is_Gate_Open { get; set; } = false;


    public override void Start()
    {
        base.Start();
        _canHacking = false;
    }
    public override void Update()
    {
        base.Update();
        if (_canHacking && !_is_Dead && !_isRoll && _is_Gate_Open == false)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _is_Gate_Open = true;
                _game_Manger.ExitOpen();
            }
        }
    }

    public override void ChangeAnimation(Anistate newState)
    {
        base.ChangeAnimation(newState);
        switch (newState)
        {
            case Anistate.Idel:
                _isRoll = false;
                _my_Animator.SetBool(_hash_IsRoll, _isRoll);
                _is_Dead = false;
                _my_Animator.SetBool("Death", _is_Dead);
                break;
            case Anistate.Run:
                _isRoll = false;
                _my_Animator.SetBool(_hash_IsRoll, _isRoll);
                _is_Dead = false;
                _my_Animator.SetBool("Death", _is_Dead);
                break;
            case Anistate.Roll:
                _my_Animator.SetBool(_hash_IsRoll, _isRoll);
                break;

            case Anistate.Death:
                _my_Animator.SetBool("Death", _is_Dead);
                break;
            case Anistate.Victory:
                _isRoll = false;
                _my_Animator.SetBool(_hash_IsRoll, _isRoll);
                _is_Dead = false;
                _my_Animator.SetBool("Death", _is_Dead);
                break;
        }
    }
    protected override void OnAction()
    {
        _isRoll = true;
        ChangeAnimation(Anistate.Roll);
    }
    public override void Hitting()
    {
        if (_isRoll == false && _is_Dead == false)
        {
            _is_Dead = true;
            ChangeAnimation(Anistate.Death);
        }
    }

    public void SetHackingState(bool canHack)
    {
        _canHacking = canHack;
    }
}
