using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// ScreenshotSource consumes a library of .pngs, if a gzip of diffs has not been provided 
// Outputs a compute buffer set with color index values
public class ScreenshotSource : IRawDataSource
{
    public const string RawImagesFolder = @"F:\rPlace2022\latest\images_single\"; // Replace this with yer local folder
    private string[] images;
    private byte[] rawImageData;
    private Texture2D imageVessel;

    private uint[] colorData;
    private readonly PlaceTwentyTwentyTwoColorPalette palette;
    public ComputeBuffer PixelIndexValuesBuffer { get; private set; }

    public int CurrentStepIndex { get; private set; }

    public int TotalSteps { get; }

    public ScreenshotSource()
    {
        colorData = new uint[MainViewerScript.ImageResolution * MainViewerScript.ImageResolution];
        PixelIndexValuesBuffer = new ComputeBuffer(MainViewerScript.ImageResolution * MainViewerScript.ImageResolution, sizeof(uint));
        images = Directory.GetFiles(RawImagesFolder).OrderBy(item => item).ToArray(); // Alphabetical order
        TotalSteps = images.Length;
        imageVessel = new Texture2D(2, 2); // Will automaticaly resize to texture dimensions on load
        palette = new PlaceTwentyTwentyTwoColorPalette();
    }

    public void SetNextStep()
    {
        string filePath = images[CurrentStepIndex];
        rawImageData = File.ReadAllBytes(filePath);
        imageVessel.LoadImage(rawImageData);

        int pixelIndex = 0;
        for (int x = 0; x < MainViewerScript.ImageResolution; x++)
        {
            for (int y = 0; y < MainViewerScript.ImageResolution; y++)
            {
                if(x < imageVessel.width && y < imageVessel.height)
                {
                    Color color = imageVessel.GetPixel(x, y);
                    colorData[pixelIndex] = GetIndexValue(color);
                }
                pixelIndex++;
            }
        }
        PixelIndexValuesBuffer.SetData(colorData);
        CurrentStepIndex++;
    }

    private uint GetIndexValue(Color color)
    {
        string hexString = ColorUtility.ToHtmlStringRGB(color);
        return palette.GetColorIndex(hexString);

    }

    public void Dispose()
    {
        PixelIndexValuesBuffer.Dispose();
    }
}

public class PlaceTwentyTwentyTwoColorPalette
{
    private Dictionary<string, uint> table;

    public PlaceTwentyTwentyTwoColorPalette()
    {
        table = new Dictionary<string, uint>();
        table.Add("FFFFFF", 0);
        table.Add("FFF8B8", 1);
        table.Add("FFD635", 2);
        table.Add("FFB470", 3);
        table.Add("FFA800", 4);
        table.Add("FF99AA", 5);
        table.Add("FF4500", 6);
        table.Add("FF3881", 7);
        table.Add("E4ABFF", 8);
        table.Add("DE107F", 9);
        table.Add("D4D7D9", 10);
        table.Add("BE0039", 11);
        table.Add("B44AC0", 12);
        table.Add("9C6926", 13);
        table.Add("94B3FF", 14);
        table.Add("898D90", 15);
        table.Add("811E9F", 16);
        table.Add("7EED56", 17);
        table.Add("6D482F", 18);
        table.Add("6D001A", 19);
        table.Add("6A5CFF", 20);
        table.Add("51E9F4", 21);
        table.Add("515252", 22);
        table.Add("493AC1", 23);
        table.Add("3690EA", 24);
        table.Add("2450A4", 25);
        table.Add("00CCC0", 26);
        table.Add("00CC78", 27);
        table.Add("00A368", 28);
        table.Add("009EAA", 29);
        table.Add("00756F", 30);
        table.Add("000000", 31);
    }

    public uint GetColorIndex(string hex)
    {
        return table[hex];
    }
}