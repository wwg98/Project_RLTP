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
                explanation.text = "�÷��̾��� ���� ������ �����մϴ�.";
                image.sprite = cardimages[0];
            }
            else
            {
                explanation.text = "�÷��̾��� �������� ���� �ð��� �����մϴ�.";
                image.sprite = cardimages[1];
            }

        }
        else if (Game_Data_manager.Instance.Role == Role.Rabbit)
        {
            if (Game_Data_manager.Instance.PlayerName == name)
            {
                explanation.text = "�÷��̾��� �������� ���� �ð��� �����մϴ�.";
                image.sprite = cardimages[1];
            }
            else
            {
                explanation.text = "�÷��̾��� ���� ������ �����մϴ�.";
                image.sprite = cardimages[0];
            }
        }
    }
}
