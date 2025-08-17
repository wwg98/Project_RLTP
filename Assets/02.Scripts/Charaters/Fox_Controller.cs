using System.Collections;
using System.Collections.Generic;
using Define_Enums;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class Fox_Controller : Player_Controller
{
    [SerializeField]
    public Transform effect_pos;
    public GameObject[] _hit_effect;

    public override void Start()
    {
        base.Start();

        _atckZoneList = new BoxCollider[_rootZone.childCount];
        for (int i = 0; i < _atckZoneList.Length; i++)
        {
            _atckZoneList[i] = _rootZone.GetChild(i).GetComponent<BoxCollider>();
            _atckZoneList[i].enabled = false;
            Attack_Zone az = _atckZoneList[i].GetComponent<Attack_Zone>();
            az.InitAttackZone(this);
        }
    }
    public override void ChangeAnimation(Anistate newState)
    {
        base.ChangeAnimation(newState);
        switch (newState)
        {
            case Anistate.Idel:
                _isAttack = false;
                _my_Animator.SetBool(_hash_IsAttack, _isAttack);
                break;
            case Anistate.Run:
                _isAttack = false;
                _my_Animator.SetBool(_hash_IsAttack, _isAttack);
                break;
            case Anistate.Attack:
                _my_Animator.SetBool(_hash_IsAttack, _isAttack);
                break;
            case Anistate.Victory:
                _isAttack = false;
                _my_Animator.SetBool(_hash_IsAttack, _isAttack);
                break;
        }
    }

    protected override void OnAction()
    {
        _isAttack = true;
        ChangeAnimation(Anistate.Attack);
    }
    public override void Hitting()
    {
        // 공격 성공
    }

    public override void Active_Effect()
    {
        if (effect_pos == null)
        {
            return;
        }

        if (_hit_effect == null || _hit_effect.Length == 0 || _hit_effect[0] == null)
        {
            return;
        }

        GameObject effectInstance = Instantiate(_hit_effect[0], effect_pos.position, effect_pos.rotation);
    }
}