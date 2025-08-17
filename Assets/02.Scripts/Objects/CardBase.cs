using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBase : MonoBehaviour
{
    [Header("ī�� UI ����")]
    public Text _rarity_txt;
    public Text _name_text;
    public Text _skill_txt;
    public Image _card_image;

    public int _card_ID { get; set; }

    public void InitCard(int Card_Num)
    {
        CardData data = GameTableManager._instance.GetCardDataByID(Card_Num);

        _rarity_txt.text = data.CardQuality.ToString();
        _name_text.text = data.CardName_Full;
        _skill_txt.text = data.AdditionalEffectDescribtion;
        _card_ID = Card_Num;
        Sprite cardSprite = Resources.Load<Sprite>($"Images/{data.ImageName}");
        if (cardSprite != null)
        {
            _card_image.sprite = cardSprite;
        }
        else
        {
            Debug.LogWarning($"�̹��� '{data.ImageName}' �� ã�� �� �����ϴ�. Resources/Images/ ��� Ȯ�� �ʿ�.");   
        }
    }
}
