using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeometryShaderExperimentScript : MonoBehaviour
{
    public Material Mat;

    [Range(0, 1)]
    public float Time;

    public Transform Light;

    const string OutputFolder = @"C:\Users\Lisa\Documents\PlaceViewer\ProcessedData\";
    private string[] texturePaths;
    private byte[] inputPngData;
    private Texture2D inputTexture;

    void Start ()
    {
        texturePaths = Directory.GetFiles(OutputFolder);
        inputTexture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        inputTexture.filterMode = FilterMode.Point;
    }
	
	void Update ()
    {
        int textureIndex = (int)Mathf.Min(texturePaths.Length * Time, texturePaths.Length - 1);
        string path = texturePaths[textureIndex];
        inputPngData = File.ReadAllBytes(path);
        inputTexture.LoadImage(inputPngData);

        Mat.SetTexture("_MainTex", inputTexture);
        Mat.SetVector("_LightPos", Light.position);
    }
}
