using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ImageProcessor : MonoBehaviour
{
    public bool WriteOutput;

    public bool DisplayLongevity;
    
    public string OutputFolder;
    private bool validFolder;

    public Material Mat;

    public static string OutputPathFile
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "pngOutputFolder.txt");
        }
    }

    [Range(0, 1)]
    public float HeatBurst;
    [Range(.9f, 1)]
    public float HeatDecay;

    [Range(0f, 1)]
    public float HistoryShift;

    [Range(0, 5)]
    public float HeatArc;

    public ComputeShader Compute;

    public Text Progressbar;

    private int index;

    const int ImageResolution = 1024;
    const int FullResolution = ImageResolution * ImageResolution;


    private DiffLoader _diffLoader;

    private int _computeKernel;
    private int _occlusionKernel;

    private ComputeBuffer _dataBuffer;
    private int _dataBufferStride = sizeof(float) * 2 + sizeof(float) + sizeof(uint) + sizeof(float) * 16;

    private ComputeBuffer _gridBuffer;
    private int _gridBufferStride = sizeof(float) * 2;
    
    private const int DispatchGroupSize = 128;
    
    private byte[] outputPngData;

    private Texture2D outputVessel;
    public RenderTexture intermediateRenderTexture;
    public RenderTexture outputRenderTexture;

    [DllImport("user32.dll")]
    private static extern void FolderBrowserDialog();

    struct ParticleData
    {
        public Vector2 SourcePosition;
        public float Heat;
        public uint Color;
        public float ColorHistory0;
        public float ColorHistory1;
        public float ColorHistory2;
        public float ColorHistory3;
        public float ColorHistory4;
        public float ColorHistory5;
        public float ColorHistory6;
        public float ColorHistory7;
        public float ColorHistory8;
        public float ColorHistory9;
        public float ColorHistory10;
        public float ColorHistory11;
        public float ColorHistory12;
        public float ColorHistory13;
        public float ColorHistory14;
        public float ColorHistory15;
    }

    void Start()
    {
        if (WriteOutput)
        {
            System.Windows.Forms.FolderBrowserDialog outputFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            outputFolderDialog.Description = "Where do you want to store the processed PNG data?";
            System.Windows.Forms.DialogResult result = outputFolderDialog.ShowDialog();
            OutputFolder = outputFolderDialog.SelectedPath;
            validFolder = Directory.Exists(OutputFolder);
            if (validFolder)
            {
                File.WriteAllText(OutputPathFile, OutputFolder);
            }
            else
            {
                return;
            }
        }

        _computeKernel = Compute.FindKernel("CSMain");
        _occlusionKernel = Compute.FindKernel("OcclusionCompute");
        InitializeDataBuffer();
        InitializeGridBuffer();
        
        _diffLoader = new DiffLoader();
        
        outputVessel = new Texture2D(ImageResolution, ImageResolution, TextureFormat.ARGB32, false);

        intermediateRenderTexture = new RenderTexture(ImageResolution, ImageResolution, 0, RenderTextureFormat.ARGB32);
        intermediateRenderTexture.filterMode = FilterMode.Point;
        intermediateRenderTexture.wrapMode = TextureWrapMode.Clamp;
        intermediateRenderTexture.enableRandomWrite = true;
        intermediateRenderTexture.Create();

        outputRenderTexture = GetRenderTexture();
    }

    private Texture2D GetInputTexture()
    {
        Texture2D ret = new Texture2D(1024, 1024);
        ret.filterMode = FilterMode.Point;
        ret.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < 1024; i++)
        {
            for (int j = 0; j < 1024; j++)
            {
                ret.SetPixel(i, j, Color.black);
            }
        }
        ret.Apply();
        return ret;
    }

    private RenderTexture GetRenderTexture()
    {
        RenderTexture ret = new RenderTexture(ImageResolution, ImageResolution, 0, RenderTextureFormat.ARGB32);
        ret.filterMode = FilterMode.Point;
        ret.wrapMode = TextureWrapMode.Clamp;
        ret.enableRandomWrite = true;
        ret.Create();
        return ret;
    }

    private void InitializeGridBuffer()
    {
        _gridBuffer = new ComputeBuffer(FullResolution, _gridBufferStride);
        Vector2[] data = new Vector2[FullResolution];
        for (int i = 0; i < ImageResolution; i++)
        {
            for (int j = 0; j < ImageResolution; j++)
            {
                Vector2 position = new Vector2((float)i / ImageResolution, (float)j / ImageResolution);
                data[i * ImageResolution + j] = position;
            }
        }
        _gridBuffer.SetData(data);
    }

    private void InitializeDataBuffer()
    {
        _dataBuffer = new ComputeBuffer(FullResolution, _dataBufferStride);
        ParticleData[] data = new ParticleData[FullResolution];
        for (int i = 0; i < ImageResolution; i++)
        {
            for (int j = 0; j < ImageResolution; j++)
            {
                Vector2 sourcePosition = new Vector2(i, j);
                data[i * ImageResolution + j] = new ParticleData() {SourcePosition = sourcePosition};
            }
        }
        _dataBuffer.SetData(data);
    }
    
    private Rect kRec = new Rect(0, 0, ImageResolution, ImageResolution);

    private void Update()
    {
        if (WriteOutput)
        {
            if (!validFolder)
            {
                Progressbar.text = "No valid output folder was selected.";
                return;
            }
        }
        if(_diffLoader.CurrentDiffIndex == _diffLoader.TotalDiffCount -1)
        {
            Progressbar.text = "Processing Complete";
            return;
        }
        index++;
        Progressbar.text = GetProgressText() ;
        string outputPath = OutputFolder + index + ".png";

        Compute.SetBuffer(_computeKernel, "_SourceDataBuffer", _diffLoader.GetNextTimeslice());
        
        int groupSize = Mathf.CeilToInt((float)FullResolution / DispatchGroupSize);

        Compute.SetFloat("_FrameIndex", index);
        Compute.SetFloat("_HeatBurst", HeatBurst);
        Compute.SetFloat("_HeatDecay", HeatDecay);
        Compute.SetBuffer(_computeKernel, "_DataBuffer", _dataBuffer);
        Compute.SetTexture(_computeKernel, "OutputTexture", intermediateRenderTexture);
        Compute.Dispatch(_computeKernel, groupSize, 1, 1);

        Compute.SetBuffer(_occlusionKernel, "_DataBuffer", _dataBuffer);
        Compute.SetTexture(_occlusionKernel, "OcclusionInputTexture", intermediateRenderTexture);
        Compute.SetTexture(_occlusionKernel, "OutputTexture", outputRenderTexture);
        Compute.Dispatch(_occlusionKernel, groupSize, 1, 1);
        
        if(WriteOutput)
        {
            RenderTexture.active = outputRenderTexture;
            outputVessel.ReadPixels(kRec, 0, 0);
            RenderTexture.active = null;
            outputPngData = outputVessel.EncodeToPNG();
            File.WriteAllBytes(outputPath, outputPngData);
        }

        Mat.SetFloat("_LongevityHeightAlpha", DisplayLongevity ? 1 : 0);
        Mat.SetFloat("_HeatHeightAlpha", DisplayLongevity ? 0 : 1);
        Mat.SetFloat("_LongevityAlpha", DisplayLongevity ? 1 : 0);
        Mat.SetFloat("_HeatAlpha", 0);
        Mat.SetTexture("_MainTex", outputRenderTexture);
    }

    private string GetProgressText()
    {
        int percent = (int)(100 * ((float)_diffLoader.CurrentDiffIndex / _diffLoader.TotalDiffCount));
        return _diffLoader.CurrentDiffIndex + " of " + _diffLoader.TotalDiffCount
            + "\n" + percent + "% complete";
    }
}

