using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public int Index;
    public int ID;
    public string CardName_Full;
    public string CardName_Short;
    public int CardQuality;
    public string CardType;
    public float PlayerSpeed;
    public float BasicCooltime;
    public float ActiveDown;
    public float UseTimeDecrease;
    public float DebuffDurationEnemy;
    public float DebuffDurationSelf;
    public float BuffDurationSelf;
    public float TrapResetCooltime;
    public float ActiveCooltime;
    public float ActiveUseTime;
    public string RoleRestriction;
    public string ImageName;
    public string AdditionalEffectDescribtion;
}