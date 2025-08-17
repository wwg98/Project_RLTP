using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Define_Enums;

public class Exit_Zone : MonoBehaviour
{
    public Player_Interface_Manager _interfaceManager;
    public In_Game_Manger _game_Manger;

    bool on = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Rabbit"))
        {
            if(!on && PhotonNetwork.IsMasterClient)
            {
                on = true;
                _game_Manger.UpdateRabbitScore();
            }
        }
    }
}
