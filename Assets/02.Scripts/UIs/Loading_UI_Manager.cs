using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Loading_UI_Manager : MonoBehaviour
{
    [Header("UI 참조")]
    public Slider _barLoading;
    public Text _text_Loading;

    float _dotDelayTime = 0.7f;
    int _dotMaxCount = 6;
    string _loadingText = "로딩";

    float _checkDotTime;
    int _dotTextCount;
    float _gaugeTimer;

    void Start()
    {
        OpenWindow();
    }

    void Update()
    {
        _checkDotTime = Time.time;
        if (_checkDotTime >= _dotDelayTime)
        {
            _checkDotTime -= _dotDelayTime;
            _dotTextCount++;

            if (_dotTextCount >= _dotMaxCount)
            {
                _dotMaxCount = 0;
            }

            _text_Loading.text = _loadingText;
            for (int n = 0; n < _dotMaxCount; n++)
            {
                _text_Loading.text += ".";
            }
        }

        LoadingProgressCircleFunc();
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    public void OpenWindow()
    {
        gameObject.SetActive(true);

        _text_Loading.text = "로딩";
        _checkDotTime = 0;
        _dotTextCount = 0;
        SetLoadingRate(0);
    }

    public void SetLoadingRate(float rate)
    {
        _barLoading.value = rate;
    }

    void LoadingProgressCircleFunc()
    {
        _gaugeTimer += 1.0f / 0.8f * Time.deltaTime;
        if (_gaugeTimer >= 1)
        {
            _gaugeTimer = 0;
        }
    }
}
