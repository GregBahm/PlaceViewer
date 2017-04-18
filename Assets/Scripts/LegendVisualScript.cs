using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegendVisualScript : MonoBehaviour
{
    public MainViewerScript.ColorMode CurrentColorMode;
    public bool ShowingHeight;
    
    [Range(0, 1)]
    public float ColorLerpSpeed = .3f;
    

    public Material LegendMat;
    
    public Material LegendColorTextMat;
    private TextMaterialManager _colorText;
    public Material LegendHeatTextMat;
    private TextMaterialManager _heatText;
    public Material LegendLongevityTextMat;
    private TextMaterialManager _longevityText;
    public Material LegendHeatVerticalTextMat;
    private TextMaterialManager _heatVerticalText;

    private TextMaterialManager[] _managers;

    public Color MatTextColor;
    
    private bool _shownHeight;

    private float _currentHeatAlpha;
    private float _currentLongevityAlpha;

    private Animator _animator;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.SetBool("HasHeight", ShowingHeight);
        if (_managers == null)
        {
            InitializeManagers();
        }
    }

    private void Update()
    {
        if(ShowingHeight != _shownHeight)
        {
            _animator.SetBool("HasHeight", ShowingHeight);
            _shownHeight = ShowingHeight;
        }
        UpdateColorModeProperties();
        foreach (TextMaterialManager manager in _managers)
        {
            manager.Update(MatTextColor, ColorLerpSpeed);
        }
    }

    private void UpdateColorModeProperties()
    {
        float heatTarget = CurrentColorMode == MainViewerScript.ColorMode.Heat ? 1 : 0;
        float longevityTarget = CurrentColorMode == MainViewerScript.ColorMode.Longevity ? 1 : 0;
        _currentHeatAlpha = Mathf.Lerp(_currentHeatAlpha, heatTarget, ColorLerpSpeed);
        _currentLongevityAlpha = Mathf.Lerp(_currentLongevityAlpha, longevityTarget, ColorLerpSpeed);
        LegendMat.SetFloat("_HeatAlpha", _currentHeatAlpha);
        LegendMat.SetFloat("_LongevityAlpha", _currentLongevityAlpha);
    }

    internal void SetDisplayMode(MainViewerScript.DisplayMode displayMode)
    {
        if(_managers == null)
        {
            InitializeManagers();
        }

        foreach (TextMaterialManager manager in _managers)
        {
            manager.MatAlphaTarget = 0;
        }
        switch (displayMode)
        {
            case MainViewerScript.DisplayMode.ColorAndHeat:
                ShowingHeight = true;
                CurrentColorMode = MainViewerScript.ColorMode.BaseColor;
                _colorText.MatAlphaTarget = 1;
                _heatVerticalText.MatAlphaTarget = 1;
                break;
            case MainViewerScript.DisplayMode.FullHeat:
                CurrentColorMode = MainViewerScript.ColorMode.Heat;
                ShowingHeight = true;
                _heatText.MatAlphaTarget = 1;
                break;
            case MainViewerScript.DisplayMode.FullLongevity:
                CurrentColorMode = MainViewerScript.ColorMode.Longevity;
                ShowingHeight = true;
                _longevityText.MatAlphaTarget = 1;
                break;
            case MainViewerScript.DisplayMode.FlatColor:
            default:
                CurrentColorMode = MainViewerScript.ColorMode.BaseColor;
                ShowingHeight = false;
                _colorText.MatAlphaTarget = 1;
                break;
        }
    }

    private void InitializeManagers()
    {
        _colorText = new TextMaterialManager(LegendColorTextMat);
        _heatText = new TextMaterialManager(LegendHeatTextMat);
        _longevityText = new TextMaterialManager(LegendLongevityTextMat);
        _heatVerticalText = new TextMaterialManager(LegendHeatVerticalTextMat);
        _managers = new TextMaterialManager[] { _colorText, _heatText, _longevityText, _heatVerticalText };
    }

    class TextMaterialManager
    {
        private Material mat;
        public float MatAlphaTarget;
        private float matCurrentAlpha;

        public TextMaterialManager(Material mat)
        {
            this.mat = mat;
        }

        public void Update(Color matTextColor, float colorLerpSpeed)
        {
            matCurrentAlpha = Mathf.Lerp(matCurrentAlpha, MatAlphaTarget, colorLerpSpeed);
            Color color = new Color(matTextColor.r, matTextColor.g, matTextColor.b, matCurrentAlpha);
            mat.SetColor("_Color", color);
        }
    }
}
