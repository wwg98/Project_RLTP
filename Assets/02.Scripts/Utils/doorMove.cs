using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DoorMove : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] Doors;
    private GameObject[] selectedDoors;
    private Coroutine doorRoutine;

    public void PlayDoors()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            doorRoutine = StartCoroutine(DoorRoutine());
        }
    }

    public IEnumerator DoorRoutine()
    {
        while (true)
        {
            SelectRandomDoors();
            photonView.RPC("SyncDoors", RpcTarget.AllBuffered, selectedDoors.Select(d => d.transform.GetSiblingIndex()).ToArray());

            yield return new WaitForSeconds(15f);
        }
    }

    public void StopDoorRoutine()
    {
        if(doorRoutine != null)
        {
            StopCoroutine(doorRoutine);
            doorRoutine = null;
        }

        OpenAllDoors();
    }


    void SelectRandomDoors()
    {
        selectedDoors = Doors.OrderBy(x => Random.value).Take(4).ToArray();
    }

    [PunRPC]
    void SyncDoors(int[] doorIndices)
    {
        selectedDoors = doorIndices.Select(i => Doors[i]).ToArray();
        ToggleDoors();
    }

    void ToggleDoors()
    {
        foreach (GameObject door in selectedDoors)
        {
            bool isOpen = door.transform.GetChild(0).GetChild(2).position.y > 1f;

            if (isOpen)
                Close(door);
            else
                Open(door);
        }
    }

    void Open(GameObject door)
    {
        Transform topDoor01 = door.transform.GetChild(0).GetChild(2);
        Transform topDoor02 = door.transform.GetChild(1).GetChild(2);
        Transform bottomDoor01 = door.transform.GetChild(0).GetChild(3);
        Transform bottomDoor02 = door.transform.GetChild(1).GetChild(3);

        Vector3 topMove = new Vector3(0, 5.45f, 0);
        Vector3 bottomMove = new Vector3(0, -4.35f, 0);

        Sequence doorseq = DOTween.Sequence();

        doorseq.Join(topDoor01.DOMove(topDoor01.position + topMove, 1.0f))
               .Join(topDoor02.DOMove(topDoor02.position + topMove, 1.0f))
               .Join(bottomDoor01.DOMove(bottomDoor01.position + bottomMove, 1.0f))
               .Join(bottomDoor02.DOMove(bottomDoor02.position + bottomMove, 1.0f))
               .Play();
    }

    void Close(GameObject door)
    {
        Transform topDoor01 = door.transform.GetChild(0).GetChild(2);
        Transform topDoor02 = door.transform.GetChild(1).GetChild(2);
        Transform bottomDoor01 = door.transform.GetChild(0).GetChild(3);
        Transform bottomDoor02 = door.transform.GetChild(1).GetChild(3);

        Sequence doorseq = DOTween.Sequence();

        doorseq.Join(topDoor01.DOMove(topDoor01.position - new Vector3(0, 5.45f, 0), 1.0f))
               .Join(topDoor02.DOMove(topDoor02.position - new Vector3(0, 5.45f, 0), 1.0f))
               .Join(bottomDoor01.DOMove(bottomDoor01.position - new Vector3(0, -4.35f, 0), 1.0f))
               .Join(bottomDoor02.DOMove(bottomDoor02.position - new Vector3(0, -4.35f, 0), 1.0f))
               .Play();
    }

    public void OpenAllDoors()
    {
        foreach (GameObject door in Doors)
        {
            Open(door);
        }
    }

}
