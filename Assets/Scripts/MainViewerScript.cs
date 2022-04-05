using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MainViewerScript : MonoBehaviour
{
    public const string OutputFolder = @"F:\rPlace2022\Output"; // Change this to a local folder

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

    public Material Mat;

    private TextureLoader mainTextureLoader;

    private string[] texturePaths;
    
    private float currentHeatAlpha;
    private float currentLongevityAlpha;

    private float currentHeatHeightAlpha;
    private float currentLongevityHeightAlpha;


    void Start()
    {
        texturePaths = Directory.GetFiles(OutputFolder);
        mainTextureLoader = new TextureLoader(Mat);
    }
    
    private void Update()
    {
        UpdateColorModeProperties();
        UpdateHeightModeProperties();
        mainTextureLoader.UpdateTexture(Time, texturePaths);

        Mat.SetVector("_LightPos", Light.position);
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

    class TextureLoader
    {
        private int lastLoadedTextureIndex;
        
        private Texture2D inputTexture;
        private byte[] inputPngData;

        private Material[] mats;

        public TextureLoader(params Material[] mats)
        {
            this.mats = mats;
            inputTexture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            inputTexture.filterMode = FilterMode.Point;
            inputTexture.wrapMode = TextureWrapMode.Clamp;
        }

        public void UpdateTexture(float time, string[] texturePaths)
        {
            int textureIndex = (int)Mathf.Min(texturePaths.Length * time, texturePaths.Length - 1);

            if (textureIndex != lastLoadedTextureIndex)
            {
                LoadTexture(textureIndex, texturePaths);
                lastLoadedTextureIndex = textureIndex;
            }
        }

        private void LoadTexture(int textureIndex, string[] texturePaths)
        {
            string path = texturePaths[textureIndex];
            inputPngData = File.ReadAllBytes(path);
            inputTexture.LoadImage(inputPngData);

            foreach (Material mat in mats)
            {
                mat.SetTexture("_MainTex", inputTexture);
            }
        }
    }
}

