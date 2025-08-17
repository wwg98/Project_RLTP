using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Define_Enums;
using Unity.Animations.SpringBones.GameObjectExtensions;
using Photon.Pun.Demo.Procedural;

public class SampleCard01 : MonoBehaviour
{
    [SerializeField] Text nametxt;
    [SerializeField] Text explanation;
    [SerializeField] Image image;
    [SerializeField] Sprite[] cardimages;

    In_Game_Manger In_Game_Manger;

    void Start()
    {
        In_Game_Manger = GameObject.Find("In_Game_manager").GetComponent<In_Game_Manger>();

        nametxt.text = "Active LV UP";
    }

    public void Chagecard(string name)
    {
        if (Game_Data_manager.Instance.Role == Role.Fox)
        {
            if (Game_Data_manager.Instance.PlayerName == name)
            {
                explanation.text = "�÷��̾��� ���� �ӵ��� �����մϴ�.";
                image.sprite = cardimages[0];
            }
            else
            {
                explanation.text = "�÷��̾��� ������ �Ÿ��� �����մϴ�.";
                image.sprite = cardimages[1];
            }

        }
        else if (Game_Data_manager.Instance.Role == Role.Rabbit)
        {
            if (Game_Data_manager.Instance.PlayerName == name)
            {
                explanation.text = "�÷��̾��� ������ �Ÿ��� �����մϴ�.";
                image.sprite = cardimages[1];
            }
            else
            {
                explanation.text = "�÷��̾��� ���� �ӵ��� �����մϴ�.";
                image.sprite = cardimages[0];
            }
        }
    }
}
