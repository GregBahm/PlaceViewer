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
    private TextMaterialManager colorText;
    public Material LegendHeatTextMat;
    private TextMaterialManager heatText;
    public Material LegendLongevityTextMat;
    private TextMaterialManager longevityText;
    public Material LegendHeatVerticalTextMat;
    private TextMaterialManager heatVerticalText;

    private TextMaterialManager[] managers;

    public Color MatTextColor;
    
    private bool shownHeight;

    private float currentHeatAlpha;
    private float currentLongevityAlpha;

    private Animator animator;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("HasHeight", ShowingHeight);
        if (managers == null)
        {
            InitializeManagers();
        }
    }

    private void Update()
    {
        if(ShowingHeight != shownHeight)
        {
            animator.SetBool("HasHeight", ShowingHeight);
            shownHeight = ShowingHeight;
        }
        UpdateColorModeProperties();
        foreach (TextMaterialManager manager in managers)
        {
            manager.Update(MatTextColor, ColorLerpSpeed);
        }
    }

    private void UpdateColorModeProperties()
    {
        float heatTarget = CurrentColorMode == MainViewerScript.ColorMode.Heat ? 1 : 0;
        float longevityTarget = CurrentColorMode == MainViewerScript.ColorMode.Longevity ? 1 : 0;
        currentHeatAlpha = Mathf.Lerp(currentHeatAlpha, heatTarget, ColorLerpSpeed);
        currentLongevityAlpha = Mathf.Lerp(currentLongevityAlpha, longevityTarget, ColorLerpSpeed);
        LegendMat.SetFloat("_HeatAlpha", currentHeatAlpha);
        LegendMat.SetFloat("_LongevityAlpha", currentLongevityAlpha);
    }

    internal void SetDisplayMode(MainViewerScript.DisplayMode displayMode)
    {
        if(managers == null)
        {
            InitializeManagers();
        }

        foreach (TextMaterialManager manager in managers)
        {
            manager.MatAlphaTarget = 0;
        }
        switch (displayMode)
        {
            case MainViewerScript.DisplayMode.ColorAndHeat:
                ShowingHeight = true;
                CurrentColorMode = MainViewerScript.ColorMode.BaseColor;
                colorText.MatAlphaTarget = 1;
                heatVerticalText.MatAlphaTarget = 1;
                break;
            case MainViewerScript.DisplayMode.FullHeat:
                CurrentColorMode = MainViewerScript.ColorMode.Heat;
                ShowingHeight = true;
                heatText.MatAlphaTarget = 1;
                break;
            case MainViewerScript.DisplayMode.FullLongevity:
                CurrentColorMode = MainViewerScript.ColorMode.Longevity;
                ShowingHeight = true;
                longevityText.MatAlphaTarget = 1;
                break;
            case MainViewerScript.DisplayMode.FlatColor:
            default:
                CurrentColorMode = MainViewerScript.ColorMode.BaseColor;
                ShowingHeight = false;
                colorText.MatAlphaTarget = 1;
                break;
        }
    }

    private void InitializeManagers()
    {
        colorText = new TextMaterialManager(LegendColorTextMat);
        heatText = new TextMaterialManager(LegendHeatTextMat);
        longevityText = new TextMaterialManager(LegendLongevityTextMat);
        heatVerticalText = new TextMaterialManager(LegendHeatVerticalTextMat);
        managers = new TextMaterialManager[] { colorText, heatText, longevityText, heatVerticalText };
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
