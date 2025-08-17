using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_Zone : MonoBehaviour
{
    Player_Controller _Owner;
    BoxCollider _collider;
    In_Game_Manger _game_Manger;

    bool _is_Hit = false;

    private void Start()
    {
        _game_Manger = GameObject.Find("In_Game_manager").GetComponent<In_Game_Manger>();
    }

    public void InitAttackZone(Player_Controller onwer)
    {
        if (onwer == null)
        {
            Debug.LogError("Owner is null!");
            return;
        }

        _Owner = onwer;
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Rabbit") && !_is_Hit )
        {
            _is_Hit=true;
            Player_Controller opponentplayer = other.GetComponent<Player_Controller>();
            opponentplayer.Hitting();
            _Owner.Active_Effect();
            if (PhotonNetwork.IsMasterClient)
            {
                _game_Manger.StartSlowMotion();
            }
        }
    }
}
