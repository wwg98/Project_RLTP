using Define_Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleCard02 : MonoBehaviour
{
    [SerializeField] Text nametxt;
    [SerializeField] Text explanation;
    [SerializeField] Image image;
    [SerializeField] Sprite[] cardimages;

    void Start()
    {
        nametxt.text = "Power LV UP";
    }

    public void Chagecard(string name)
    {
        if (Game_Data_manager.Instance.Role == Role.Fox)
        {
            if (Game_Data_manager.Instance.PlayerName == name)
            {
                explanation.text = "플레이어의 공격 범위가 증가합니다.";
                image.sprite = cardimages[0];
            }
            else
            {
                explanation.text = "플레이어의 구르기의 무적 시간이 증가합니다.";
                image.sprite = cardimages[1];
            }

        }
        else if (Game_Data_manager.Instance.Role == Role.Rabbit)
        {
            if (Game_Data_manager.Instance.PlayerName == name)
            {
                explanation.text = "플레이어의 구르기의 무적 시간이 증가합니다.";
                image.sprite = cardimages[1];
            }
            else
            {
                explanation.text = "플레이어의 공격 범위가 증가합니다.";
                image.sprite = cardimages[0];
            }
        }
    }
}
