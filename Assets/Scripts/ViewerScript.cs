using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ViewerScript : MonoBehaviour
{
    public enum DisplayMode
    {
        ColorAndHeat,
        FullHeat,
        FullLongevity,
        FlatColor
    }

    public enum ColorMode
    {
        BaseColor,
        Heat,
        Longevity
    }

    public enum HeightMode
    {
        Heat,
        Longevity,
        None
    }
    
    public ColorMode CurrentColorMode;
    public HeightMode CurrentHeightMode;

    public Transform Light;
    
    [Range(0, 1)]
    public float Time;

    [Range(0, 1)]
    public float ColorLerpSpeed;
    
    private string OutputFolder = @"C:\Users\Lisa\Documents\PlaceViewer\ProcessedData\";

    public Material Mat;
    
    private const int DispatchGroupSize = 128;

    private string[] texturePaths;

    private Texture2D inputTexture;
    private byte[] inputPngData;
    
    private float currentHeatAlpha;
    private float currentLongevityAlpha;

    private float currentHeatHeightAlpha;
    private float currentLongevityHeightAlpha;

    private int lastLoadedTextureIndex;

    void Start()
    {
        //OutputFolder = File.ReadAllText(ImageProcessor.OutputPathFile);
        texturePaths = Directory.GetFiles(OutputFolder);

        inputTexture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        inputTexture.filterMode = FilterMode.Point;
    }
    
    private void Update()
    {
        UpdateColorModeProperties();
        UpdateHeightModeProperties();

        int textureIndex = (int)Mathf.Min(texturePaths.Length * Time, texturePaths.Length - 1);
        if(textureIndex != lastLoadedTextureIndex)
        {
            LoadTexture(textureIndex);
            lastLoadedTextureIndex = textureIndex;
        }
        Mat.SetVector("_LightPos", Light.position);
    }

    private void LoadTexture(int textureIndex)
    {
        string path = texturePaths[textureIndex];
        inputPngData = File.ReadAllBytes(path);
        inputTexture.LoadImage(inputPngData);

        Mat.SetTexture("_MainTex", inputTexture);
    }

    private void UpdateHeightModeProperties()
    {
        float baseHeatHeightTarget = CurrentHeightMode == HeightMode.Heat ? 1 : 0;
        float baseLongevityHeightTarget = CurrentHeightMode == HeightMode.Longevity ? 1 : 0;
        currentHeatHeightAlpha = Mathf.Lerp(currentHeatHeightAlpha, baseHeatHeightTarget, ColorLerpSpeed);
        currentLongevityHeightAlpha = Mathf.Lerp(currentLongevityHeightAlpha, baseLongevityHeightTarget, ColorLerpSpeed);
        Mat.SetFloat("_HeatHeightAlpha", currentHeatHeightAlpha);
        Mat.SetFloat("_LongevityHeightAlpha", currentLongevityHeightAlpha);
    }

    private void UpdateColorModeProperties()
    {
        float heatTarget = CurrentColorMode == ColorMode.Heat ? 1 : 0;
        float longevityTarget = CurrentColorMode == ColorMode.Longevity ? 1 : 0;
        currentHeatAlpha = Mathf.Lerp(currentHeatAlpha, heatTarget, ColorLerpSpeed);
        currentLongevityAlpha = Mathf.Lerp(currentLongevityAlpha, longevityTarget, ColorLerpSpeed);
        Mat.SetFloat("_HeatAlpha", currentHeatAlpha);
        Mat.SetFloat("_LongevityAlpha", currentLongevityAlpha);
    }

    internal void SetDisplayMode(DisplayMode displayMode)
    {
        switch (displayMode)
        {
            case DisplayMode.ColorAndHeat:
                CurrentColorMode = ColorMode.BaseColor;
                CurrentHeightMode = HeightMode.Heat;
                break;
            case DisplayMode.FullHeat:
                CurrentColorMode = ColorMode.Heat;
                CurrentHeightMode = HeightMode.Heat;
                break;
            case DisplayMode.FullLongevity:
                CurrentColorMode = ColorMode.Longevity;
                CurrentHeightMode = HeightMode.Longevity;
                break;
            case DisplayMode.FlatColor:
            default:
                CurrentColorMode = ColorMode.BaseColor;
                CurrentHeightMode = HeightMode.None;
                break;
        }
    }
}

