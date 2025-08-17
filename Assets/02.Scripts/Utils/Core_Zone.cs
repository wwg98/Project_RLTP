using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class HackingZone : MonoBehaviour
{
    public GameObject hackingUI;


    private void Start()
    {
        hackingUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Rabbit"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            Rabbit_Controller rc = other.gameObject.GetComponent<Rabbit_Controller>();
            if (pv != null && pv.IsMine && !rc._is_Gate_Open)
            {
                hackingUI.SetActive(true);
                Player_Controller opponentplayer = other.GetComponent<Player_Controller>();
                if (opponentplayer is Rabbit_Controller rabbit)
                {
                    rabbit.SetHackingState(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player_Rabbit"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                hackingUI.SetActive(false);
                Player_Controller opponentplayer = other.GetComponent<Player_Controller>();
                if (opponentplayer is Rabbit_Controller rabbit)
                {
                    rabbit.SetHackingState(false);
                }
            }

        }
    }
}

