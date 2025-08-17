using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Player_Interface_Manager : MonoBehaviour
{
    public Image[] ScoreBords;

    int scorecount = 0;

    private void Start()
    {
        SetAllAlpha(0f);
        scorecount = 0;
    }


    public void GetFoxScore()
    {
        Image scoreimage = ScoreBords[scorecount];
        Color c = Color.black;
        c.a = 1.0f;
        scoreimage.color = c;
        scorecount++;
    }

    public void GetRabbitScore()
    {
        Image scoreimage = ScoreBords[scorecount];
        Color c = Color.white;
        c.a = 1.0f;
        scoreimage.color = c;
        scorecount++;
    }

    public void ColorReset()
    {
        SetAllAlpha(0f);
        scorecount = 0;
    }

    public void SetAllAlpha(float alpha)
    {
        foreach (Image img in ScoreBords)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Clamp01(alpha);
                img.color = c;
            }
        }
    }

}
