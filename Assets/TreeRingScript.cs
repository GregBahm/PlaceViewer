using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TreeRingScript : MonoBehaviour
{
    public MainViewerScript MainScript;

    public Material NorthMat;
    public Material SouthMat;
    public Material EastMat;
    public Material WestMat;
    public Material TopMat;
    public Material BottomMat;
    public Transform CubePointA;
    public Transform CubePointB;

    public Transform NorthFace;
    public Transform SouthFace;
    public Transform EastFace;
    public Transform WestFace;
    public Transform TopFace;

    private CubeSide _northSide;
    private CubeSide _southSide;
    private CubeSide _eastSide;
    private CubeSide _westSide;

    private void Start()
    {
        _northSide = new CubeSide(@"C:\Users\Lisa\Documents\PlaceTreeRings\NewYOutput", NorthMat);
        _southSide = new CubeSide(@"C:\Users\Lisa\Documents\PlaceTreeRings\NewYOutput", SouthMat);
        _eastSide = new CubeSide(@"C:\Users\Lisa\Documents\PlaceTreeRings\NewXOutput", EastMat);
        _westSide = new CubeSide(@"C:\Users\Lisa\Documents\PlaceTreeRings\NewXOutput", WestMat);
    }

    private void Update()
    {
        MainScript.TreeRingTopTime = MainScript.Time + CubePointB.localPosition.y;
        UpdateTopTransform();
        UpdateTopMatUvs();
        UpdateFaceTextures();
        UpdateFaceTransforms();
        UpdateMaterialProperties();
    }

    private void UpdateTopTransform()
    {
        TopFace.localPosition = new Vector3(CubePointA.localPosition.x, CubePointB.localPosition.y, CubePointA.localPosition.z);
        float scaleX = CubePointB.localPosition.x - CubePointA.localPosition.x;
        float scaleZ = CubePointB.localPosition.z - CubePointA.localPosition.z;
        TopFace.localScale = new Vector3(scaleX, 1, scaleZ);
    }

    private void UpdateTopMatUvs()
    {
        BottomMat.SetFloat("_ShadowAlpha", Mathf.Clamp01(CubePointB.localPosition.y * 10));
        TopMat.SetFloat("_ShadowAlpha", 0);
        BottomMat.SetVector("_UvScale", Vector4.one);
        BottomMat.SetFloat("_ShadowX1", CubePointA.localPosition.z);
        BottomMat.SetFloat("_ShadowY1", CubePointA.localPosition.x);
        BottomMat.SetFloat("_ShadowX2", CubePointB.localPosition.z);
        BottomMat.SetFloat("_ShadowY2", CubePointB.localPosition.x);
        Vector2 offsetVector = new Vector2(CubePointB.localPosition.z, CubePointB.localPosition.x);
        float scaleX = CubePointA.localPosition.x - CubePointB.localPosition.x;
        float scaleZ = CubePointA.localPosition.z - CubePointB.localPosition.z;
        Vector2 scaleVector = new Vector2(scaleZ, scaleX);
        TopMat.SetVector("_UvOffset", offsetVector);
        TopMat.SetVector("_UvScale", scaleVector);
    }

    private void UpdateMaterialProperties()
    {
        float top = MainScript.TreeRingTopTime;
        float bottom = MainScript.Time;
        _northSide.UpdateMaterial(top, bottom, CubePointB.localPosition.z, CubePointA.localPosition.z);
        _southSide.UpdateMaterial(top, bottom, CubePointA.localPosition.z, CubePointB.localPosition.z);
        _westSide.UpdateMaterial(top, bottom, CubePointA.localPosition.x, CubePointB.localPosition.x);
        _eastSide.UpdateMaterial(top, bottom, CubePointB.localPosition.x, CubePointA.localPosition.x);
    }

    private void UpdateFaceTransforms()
    {
        NorthFace.localPosition = new Vector3(CubePointB.localPosition.x, CubePointA.localPosition.y, CubePointB.localPosition.z);
        EastFace.localPosition = CubePointA.localPosition;
        SouthFace.localPosition = CubePointA.localPosition;
        WestFace.localPosition = new Vector3(CubePointB.localPosition.x, CubePointA.localPosition.y, CubePointB.localPosition.z);
        float xScale = CubePointB.localPosition.x - CubePointA.localPosition.x;
        float zScale = CubePointB.localPosition.z - CubePointA.localPosition.z;
        float yScale = CubePointB.localPosition.y - CubePointA.localPosition.y;
        Vector3 xScaleVector = new Vector3(xScale, yScale, 1);
        Vector3 zScaleVector = new Vector3(1, yScale, zScale);
        NorthFace.localScale = zScaleVector;
        SouthFace.localScale = zScaleVector;
        EastFace.localScale = xScaleVector;
        WestFace.localScale = xScaleVector;
    }

    private void UpdateFaceTextures()
    {
        float northParam = 1 - CubePointB.localPosition.x;
        float southParam = 1 - CubePointA.localPosition.x;
        float westParam = 1 - CubePointB.localPosition.z;
        float eastParam = 1 - CubePointA.localPosition.z;
        _northSide.Update(northParam);
        _southSide.Update(southParam);
        _westSide.Update(westParam);
        _eastSide.Update(eastParam);
    }
     
    public class CubeSide
    {
        private readonly string[] _texturePaths;

        private int _lastLoadedTextureIndex;
        private byte[] _inputPngData;
        private Texture2D _inputTexture;

        private Material _mat;

        public CubeSide(string sourceFolder, Material mat)
        {
            _mat = mat;
            _texturePaths = Directory.GetFiles(sourceFolder);
            _inputTexture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            _inputTexture.filterMode = FilterMode.Point;
            _inputTexture.wrapMode = TextureWrapMode.Clamp;
            _lastLoadedTextureIndex = -1;
        }

        public void Update(float param)
        {
            int textureIndex = GetTargetTextureIndex(param);
            if (_lastLoadedTextureIndex != textureIndex)
            {
                LoadTexture(textureIndex);
                _lastLoadedTextureIndex = textureIndex;
            }
        }

        private int GetTargetTextureIndex(float param)
        {
            int ret = (int)(_texturePaths.Length * param);
            ret = Mathf.Clamp(ret, 0, _texturePaths.Length - 1);
            return ret;
        }

        private void LoadTexture(int textureIndex)
        {
            string path = _texturePaths[textureIndex];
            _inputPngData = File.ReadAllBytes(path);
            _inputTexture.LoadImage(_inputPngData);

            _mat.SetTexture("_MainTex", _inputTexture);
        }

        internal void UpdateMaterial(float top, float bottom, float sideStart, float sideEnd)
        {
            _mat.SetFloat("_SideStart", sideStart);
            _mat.SetFloat("_SideEnd", sideEnd);
            _mat.SetFloat("_Top", top);
            _mat.SetFloat("_Bottom", bottom);
        }
    }
}
