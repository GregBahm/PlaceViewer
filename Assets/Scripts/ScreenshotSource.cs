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
        colorData = new uint[1024 * 1024];
        PixelIndexValuesBuffer = new ComputeBuffer(1024 * 1024, sizeof(uint));
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
        for (int x = 0; x < 1024; x++)
        {
            for (int y = 0; y < 1024; y++)
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
        table.Add("BE0039", 0);
        table.Add("FF4500", 1);
        table.Add("FFA800", 2);
        table.Add("FFD635", 3);
        table.Add("00A368", 4);
        table.Add("00CC78", 5);
        table.Add("7EED56", 6);
        table.Add("00756F", 7);
        table.Add("009EAA", 8);
        table.Add("2450A4", 9);
        table.Add("3690EA", 10);
        table.Add("51E9F4", 11);
        table.Add("493AC1", 12);
        table.Add("6A5CFF", 13);
        table.Add("811E9F", 14);
        table.Add("B44AC0", 15);
        table.Add("FF3881", 16);
        table.Add("FF99AA", 17);
        table.Add("6D482F", 18);
        table.Add("9C6926", 19);
        table.Add("000000", 20);
        table.Add("898D90", 21);
        table.Add("D4D7D9", 22);
        table.Add("FFFFFF", 23);
    }

    public uint GetColorIndex(string hex)
    {
        return table[hex];
    }
}