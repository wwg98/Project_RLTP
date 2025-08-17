using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Define_Enums;
using Photon.Pun;
using static UnityEngine.Rendering.DebugUI;

public class Card_Selection_UI_Manager : MonoBehaviour
{
    [Header("UI참조")]
    public RectTransform _door01;
    public RectTransform _door02;
    public RectTransform _open_Destination01_pos;
    public RectTransform _open_Destination02_pos;
    public RectTransform _close_Destination01_pos;
    public RectTransform _close_Destination02_pos;
    public RectTransform _back_Ground;
    public GameObject _player_Interface;
    public GameObject _result_UI;
    public Result_BOX_UI Result_BOX_UI;
    public In_Game_Manger in_Game_Manger;
    public GameObject _hacking_UI;
    public GameObject _timer;
    public GameObject _turnPlayer_txt;
    [Header("카드 배치 정보")]
    public GameObject _deck;
    public RectTransform[] Cards;
    public RectTransform _card_start_pos;
    public float _baseRadius = 250f;
    public float _angleRange = 90f;
    public float _duration = 1f;



    public void OpenUI()
    {
        Interface(false);
        _result_UI.gameObject.SetActive(false);
        _hacking_UI.gameObject.SetActive(false);

        DG.Tweening.Sequence Openseq = DOTween.Sequence();

        Openseq.AppendInterval(3f);

        Openseq.Append(_door01.DOAnchorPos(_open_Destination01_pos.anchoredPosition, _duration));
        Openseq.Join(_door02.DOAnchorPos(_open_Destination02_pos.anchoredPosition, _duration));
        Openseq.AppendInterval(2f);

        Openseq.OnComplete(() =>
        {
            DG.Tweening.Sequence cardDownseq = DOTween.Sequence();
            RectTransform deckrect = _deck.GetComponent<RectTransform>();

            cardDownseq.Join(deckrect.DOAnchorPos(new Vector2(0, -900), _duration));
            cardDownseq.OnComplete(() =>
            {
                Card_Place();
            });
        });
    }
      
    public void Card_Place()
    {
        Interface(false);
        int count = Cards.Length;
        float startAngle = -_angleRange / 2f;

        DG.Tweening.Sequence placeSequence = DOTween.Sequence();

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (_angleRange / (count - 1)) * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 targetPos = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * _baseRadius;

            Cards[i].DOKill();

            placeSequence.Join(Cards[i].DOAnchorPos(targetPos, _duration).SetEase(Ease.OutQuad));
            placeSequence.Join(Cards[i].DORotate(Vector3.forward * angle, _duration).SetEase(Ease.OutQuad));
            placeSequence.OnComplete(() =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    in_Game_Manger.Updateprogress(GameBattleProgress.BUFF_CHOICE);
                }
            });
        }
    }
    public void Card_Gather()
    {
        int count = Cards.Length;
        RectTransform deckrect = _deck.GetComponent<RectTransform>();

        DG.Tweening.Sequence gatherSequence = DOTween.Sequence();

        for (int i = 0; i < count; i++)
        {
            Cards[i].DOKill();

            gatherSequence.Join(Cards[i].DOAnchorPos(Vector2.zero, _duration).SetEase(Ease.InQuad));
            gatherSequence.Join(Cards[i].DORotate(Vector3.zero, _duration).SetEase(Ease.InQuad));
        }

        gatherSequence.OnComplete(() =>
        {
            DG.Tweening.Sequence bgUpseq = DOTween.Sequence();

            bgUpseq.Join(deckrect.DOAnchorPos(_card_start_pos.anchoredPosition, _duration));

            bgUpseq.OnComplete(() =>
            {
                DG.Tweening.Sequence Closeseq = DOTween.Sequence();
                Closeseq.Append(_door01.DOAnchorPos(_close_Destination01_pos.anchoredPosition, _duration));
                Closeseq.Join(_door02.DOAnchorPos(_close_Destination02_pos.anchoredPosition, _duration));

                Closeseq.AppendCallback(() => {
                    _back_Ground.gameObject.SetActive(false); 
                });

                Closeseq.AppendInterval(2f); 

                Closeseq.Append(_door01.DOAnchorPos(_open_Destination01_pos.anchoredPosition, _duration));
                Closeseq.Join(_door02.DOAnchorPos(_open_Destination02_pos.anchoredPosition, _duration));

                Closeseq.OnComplete(() =>
                {

                    if (PhotonNetwork.IsMasterClient)
                    {
                        in_Game_Manger.Updateprogress(GameBattleProgress.INGAME);
                    }
                    Interface(true);
                   
                    _turnPlayer_txt.gameObject.SetActive(false);
                    _timer.gameObject.SetActive(false);
                });

            });
        });
    }
    public void CardSelect(RectTransform selectCardRect)
    {
        float duration = 0.8f;
        CardBase cardinfo = selectCardRect.gameObject.GetComponent<CardBase>();

        Vector2 firstPos = selectCardRect.anchoredPosition;
        Vector3 firstRot = selectCardRect.localEulerAngles;
        Vector3 firstScale = selectCardRect.localScale;

        selectCardRect.SetAsLastSibling();
        selectCardRect.DOKill();

        DG.Tweening.Sequence selectSequence = DOTween.Sequence();

        selectSequence.Append(selectCardRect
            .DOAnchorPos(new Vector2(0, 0), duration)
            .SetEase(Ease.OutQuad));

        selectSequence.Join(selectCardRect
            .DOScale(1.2f, duration)
            .SetEase(Ease.OutBack));

        selectSequence.Join(selectCardRect
            .DORotate(Vector3.zero, duration)
            .SetEase(Ease.OutQuad));

        selectSequence.AppendInterval(1f);

        selectSequence.Append(selectCardRect
            .DOAnchorPos(new Vector2(0, -400), duration)
            .SetEase(Ease.InQuad));

        selectSequence.Join(selectCardRect
            .DOScale(0.5f, duration)
            .SetEase(Ease.InBack));

        selectSequence.AppendCallback(() =>
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Round", out object roundValue) &&
            roundValue is int round)
            {
                string targetName = "";

                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Turn", out object turnValue) &&
                    turnValue is int turn)
                {
                    if (round == 1)
                    {
                        if (turn == 1 || turn == 2)
                        {
                            targetName = in_Game_Manger.loserName;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (turn == 1)
                        {
                            targetName = in_Game_Manger.winnerName;
                        }
                        else
                        {
                            return;
                        }
                    }

                    // 실제 카드 교체
                    in_Game_Manger.changecard(targetName);
                }
            }
        });

        selectSequence.AppendInterval(1f);

        selectSequence.Append(selectCardRect
            .DOAnchorPos(firstPos, duration)
            .SetEase(Ease.OutQuad));

        selectSequence.Join(selectCardRect
            .DOScale(firstScale, duration)
            .SetEase(Ease.OutBack));

        selectSequence.Join(selectCardRect
            .DORotate(firstRot, duration)
            .SetEase(Ease.OutQuad));

        selectSequence.OnComplete(() =>
        {
            in_Game_Manger.NextTurn();
        });
    }

    public void ScoreCheck()
    {
        _timer.SetActive(false);
        Interface(false);

        DG.Tweening.Sequence cardSeq = DOTween.Sequence();
        RectTransform deckrect = _deck.GetComponent<RectTransform>();

        cardSeq.Join(_back_Ground.DOAnchorPos(Vector2.zero, _duration));
        cardSeq.AppendInterval(3f);
        cardSeq.Append(_back_Ground.DOAnchorPos(new Vector2(0, 1130), _duration));

        cardSeq.OnComplete(() =>
        {
            Interface(true);
            in_Game_Manger.StartSharedTimer();
            in_Game_Manger.playermoveOn();
        });
    }

    public void NextRoundchoice()
    {
        _timer.SetActive(true);
        _turnPlayer_txt.gameObject.SetActive(true);
        Interface(false);

        DG.Tweening.Sequence Closeseq = DOTween.Sequence();

        Closeseq.Append(_door01.DOAnchorPos(_close_Destination01_pos.anchoredPosition, _duration));
        Closeseq.Join(_door02.DOAnchorPos(_close_Destination02_pos.anchoredPosition, _duration));

        Closeseq.AppendInterval(2f);

        Closeseq.AppendCallback(() => {
            _back_Ground.gameObject.SetActive(true);
        });

        Closeseq.Append(_door01.DOAnchorPos(_open_Destination01_pos.anchoredPosition, _duration));
        Closeseq.Join(_door02.DOAnchorPos(_open_Destination02_pos.anchoredPosition, _duration));

        Closeseq.OnComplete(() =>
        {

            DG.Tweening.Sequence cardDownseq = DOTween.Sequence();
            RectTransform deckrect = _deck.GetComponent<RectTransform>();

            cardDownseq.Join(_back_Ground.DOAnchorPos(Vector2.zero, _duration));

            cardDownseq.AppendInterval(3f);

            cardDownseq.Append(deckrect.DOAnchorPos(new Vector2(0, -900), _duration));

            cardDownseq.OnComplete(() =>
            {
                Interface(true);
                if (PhotonNetwork.IsMasterClient)
                {
                    in_Game_Manger.NextRound();
                }
                Card_Place();
            });
        });
    }


    public void  RsultOn()
    {
        Victory();
    }
    void  Victory()
    {
        _timer.SetActive(false);
        _turnPlayer_txt.gameObject.SetActive(false);
        Interface(false);

        DG.Tweening.Sequence downBG = DOTween.Sequence();

        downBG.Append(_door01.DOAnchorPos(_close_Destination01_pos.anchoredPosition, _duration));
        downBG.Join(_door02.DOAnchorPos(_close_Destination02_pos.anchoredPosition, _duration));

        downBG.AppendInterval(2f);

        downBG.OnComplete(() =>
        {
            _result_UI.gameObject.SetActive(true);

            Result_BOX_UI.OpenUI(Game_Data_manager.Instance.WinnerRole);
        });

    }
    public CardBase[] GetCards()
    {
        CardBase[] bases = new CardBase[Cards.Length];

        for (int i = 0; i < Cards.Length; i++)
        {
            bases[i] = Cards[i].gameObject.GetComponent<CardBase>();
        }

        return bases;
    }

    void Interface(bool visible)
    {
        foreach (Transform child in _player_Interface.transform)
        {
            child.gameObject.SetActive(visible);
        }
    }
}
