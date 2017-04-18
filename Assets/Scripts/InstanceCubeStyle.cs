using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InstanceCubeStyle : MonoBehaviour
{

    public enum ColorMode
    {
        BaseColor,
        Heat,
        Longevity,
        None
    }

    public enum HeightMode
    {
        Heat,
        Longevity,
        None
    }
    
    public ColorMode CurrentColorMode;
    public HeightMode CurrentHeightMode;


    public bool MoveMode;
    public bool ScaleMode;

    public Transform MainHand;
    public Transform OffHand;

    public GameObject MainTimelineControl;
    public Transform CurrentTimeUi;
    public Transform TimeStartUi;
    public Transform TimeEndUi;
    public Transform TimeContainerUi;
    private float timeUiScale;
    
    private bool scaling;
    private float initialScale;
    private float initialHandDistance;

    [Range(0, 1)]
    public float Time;

    [Range(0, 1)]
    public float ColorLerpSpeed;
    
    const string OutputFolder = @"C:\Users\Lisa\Documents\PlaceViewer\ProcessedData\";

    public Material Mat;
    
    public Mesh PointMesh;
    
    struct MeshData
    {
        public Vector3 Position;
        public Vector3 Normal;
    }
    
    private ComputeBuffer _meshBuffer;
    private int _meshBufferStride = 24;
    
    private ComputeBuffer _gridBuffer;
    private int _gridBufferStride = sizeof(float) * 2;

    private MeshData[] _mesh;
    private const int DispatchGroupSize = 128;

    private string[] texturePaths;

    private Texture2D inputTexture;
    private byte[] inputPngData;
    
    float currentBaseColorAlpha;
    float currentHeatAlpah;
    float currentLongevityAlpha;

    float currentHeatHeightAlpha;
    float currentLongevityHeightAlpha;

    void Start()
    {
        _mesh = GetMeshDefinition();
        _meshBuffer = new ComputeBuffer(_mesh.Length, _meshBufferStride);
        _meshBuffer.SetData(_mesh);
        _gridBuffer = InitializeGridBuffer();

        texturePaths = Directory.GetFiles(OutputFolder);

        inputTexture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        inputTexture.filterMode = FilterMode.Point;
        timeUiScale = (TimeStartUi.position - TimeEndUi.position).magnitude;
    }

    private void DisplayTimeUi()
    {
        MainTimelineControl.SetActive(true);
        MainTimelineControl.transform.position = MainHand.position;
        MainTimelineControl.transform.rotation = MainHand.rotation;
        CurrentTimeUi.localPosition = Vector3.zero;
        float startOffset = timeUiScale * Time;
        float endOffset = timeUiScale * (1 - Time);
        TimeStartUi.localPosition = new Vector3(-startOffset, 0, 0);
        TimeEndUi.localPosition = new Vector3(endOffset, 0, 0);
        TimeContainerUi.position = (TimeStartUi.position + TimeEndUi.position) / 2;
    }

    private void UpdateTimeline()
    {
        bool timelineTrigger = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        if(timelineTrigger && !MainTimelineControl.activeSelf)
        {
            DisplayTimeUi();
        }
        if(!timelineTrigger && MainTimelineControl.activeSelf)
        {
            MainTimelineControl.SetActive(false);
        }
        if(timelineTrigger)
        {
            float l2 = (TimeStartUi.position - TimeEndUi.position).sqrMagnitude;
            float theDot = Vector3.Dot(MainHand.position - TimeStartUi.position, TimeEndUi.position - TimeStartUi.position);
            Time = Mathf.Max(0, Mathf.Min(1, theDot / l2));
            CurrentTimeUi.position = Vector3.Lerp(TimeStartUi.position, TimeEndUi.position, Time);
        }
    }

    private ComputeBuffer InitializeGridBuffer()
    {
        float multiplier = (float)1024 / 1000;
        ComputeBuffer ret = new ComputeBuffer(1000 * 1000, _gridBufferStride);
        Vector2[] data = new Vector2[1000 * 1000];
        for (int i = 0; i < 1000; i++)
        {
            for (int j = 0; j < 1000; j++)
            {
                float xGridPos = i / (multiplier * 1000);
                float yGridPos = j / (multiplier * 1000);
                Vector2 position = new Vector2(xGridPos, 1 - yGridPos);
                data[i * 1000 + j] = position;
            }
        }
        ret.SetData(data);
        return ret;
    }

    private string GetOutputPathFromFile(string file)
    {
        string filename = Path.GetFileName(file);
        return Path.Combine(OutputFolder, filename);
    }

    private MeshData[] GetMeshDefinition()
    {
        MeshData[] ret = new MeshData[PointMesh.triangles.Length];
        for (int i = 0; i < PointMesh.triangles.Length; i++)
        {
            int vert = PointMesh.triangles[i];
            MeshData meshData = new MeshData() { Position = PointMesh.vertices[vert], Normal = PointMesh.normals[vert] };
            ret[i] = meshData;
        }
        return ret;
    }

    private void UpdateTransform()
    {
        transform.parent = MoveMode || ScaleMode ? MainHand.transform : null;
        if (ScaleMode)
        {
            float dist = (MainHand.position - OffHand.position).magnitude;
            if(!scaling)
            {
                scaling = true;
                initialScale = MainHand.transform.localScale.x;
                initialHandDistance = dist;
            }
            else
            {
                float newScale = initialScale * (dist / initialHandDistance);
                MainHand.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        else
        {
            scaling = false;
        }
    }
    
    private void Update()
    {
        MoveMode = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        ScaleMode = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        CurrentColorMode = GetCurrentColorMode();
        CurrentHeightMode = GetCurrentHeightMode();
        UpdateColorModeProperties();
        UpdateHeightModeProperties();
        UpdateTimeline();
        UpdateTransform();
        int textureIndex = (int)Mathf.Min(texturePaths.Length * Time, texturePaths.Length - 1);
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

    private HeightMode GetCurrentHeightMode()
    {
        bool heatReleased = OVRInput.GetDown(OVRInput.Button.One);
        bool longevityReleased = OVRInput.GetDown(OVRInput.Button.Two);
        if (heatReleased)
        {
            return CurrentHeightMode == HeightMode.Heat ? HeightMode.None : HeightMode.Heat;
        }
        if (longevityReleased)
        {
            return CurrentHeightMode == HeightMode.Longevity ? HeightMode.None : HeightMode.Longevity;
        }
        return CurrentHeightMode;
    }

    private ColorMode GetCurrentColorMode()
    {
        bool heatReleased = OVRInput.GetDown(OVRInput.Button.Three);
        bool longevityReleased = OVRInput.GetDown(OVRInput.Button.Four);
        if(heatReleased)
        {
            return CurrentColorMode == ColorMode.Heat ? ColorMode.BaseColor : ColorMode.Heat;
        }
        if(longevityReleased)
        {
            return CurrentColorMode == ColorMode.Longevity ? ColorMode.BaseColor : ColorMode.Longevity;
        }
        return CurrentColorMode;
    }

    private void UpdateColorModeProperties()
    {
        float baseColorTarget = CurrentColorMode == ColorMode.BaseColor ? 1 : 0;
        float heatTarget = CurrentColorMode == ColorMode.Heat ? 1 : 0;
        float longevityTarget = CurrentColorMode == ColorMode.Longevity ? 1 : 0;
        currentBaseColorAlpha = Mathf.Lerp(currentBaseColorAlpha, baseColorTarget, ColorLerpSpeed);
        currentHeatAlpah = Mathf.Lerp(currentHeatAlpah, heatTarget, ColorLerpSpeed);
        currentLongevityAlpha = Mathf.Lerp(currentLongevityAlpha, longevityTarget, ColorLerpSpeed);
        Mat.SetFloat("_ColorAlpha", currentBaseColorAlpha);
        Mat.SetFloat("_HeatAlpha", currentHeatAlpah);
        Mat.SetFloat("_LongevityAlpha", currentLongevityAlpha);
    }

    private void OnRenderObject()
    {
        Mat.SetMatrix("_Transformer", transform.localToWorldMatrix);
        Mat.SetBuffer("_GridBuffer", _gridBuffer);
        Mat.SetBuffer("_MeshBuffer", _meshBuffer);
        Mat.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, _mesh.Length, 1000 * 1000);
    }
}

