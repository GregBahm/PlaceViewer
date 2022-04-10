using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

// GZipSource uses the r/Place 2022_place_canvas_history.cvs.gzip provided by Reddit in 2022
// First I process the raw GZip with the GzipUnzipper to produce data in a format that is more to my liking
// Then this consumes that file a compute buffer of color index values, to be consumed by the BakingScript
public class GZipSource : IRawDataSource
{
    public const int totalPixelEdits = 160353104;
    public const long TimeStart = 637846304315770000;
    public const long TimeEnd = 637847144402070000;

    private const string dataLocation = @"F:\rPlace2022\SortedFiles\"; // Generated from the GZipUnzipper methods
    public ComputeBuffer PixelIndexValuesBuffer { get; private set; }

    public int CurrentStepIndex { get; private set; }
    private uint[] colorData;

    public int TotalSteps { get; } = 1000; // Determines how many PNGs you want to produce

    private long currentTimeLimit;
    private long timeStepSize;

    string currentLine;

    public GZipSource()
    {
        colorData = new uint[MainViewerScript.ImageResolution * MainViewerScript.ImageResolution];
        PixelIndexValuesBuffer = new ComputeBuffer(MainViewerScript.ImageResolution * MainViewerScript.ImageResolution, sizeof(uint));

        timeStepSize = (TimeEnd - TimeStart) / TotalSteps;
        currentTimeLimit = TimeStart;
    }

    public void Dispose()
    {
        PixelIndexValuesBuffer.Dispose();
    }

    public void SetNextStep()
    {
        currentTimeLimit += timeStepSize;
        string[] data = GetData();
        foreach (string line in data)
        {
            ProcessLine(line.Split(' '));
        }
        PixelIndexValuesBuffer.SetData(colorData);
        CurrentStepIndex++;
    }

    private string[] GetData()
    {
        string[] files = Directory.GetFiles(dataLocation);
        string file = files[CurrentStepIndex];
        return File.ReadAllLines(file);
    }

    private void ProcessLine(string[] lineComponents)
    {
        if(lineComponents.Length == 4)
        {
            SetPixelDiff(lineComponents);
        }
        else if(lineComponents.Length == 6)
        {
            SetRectangleDiff(lineComponents);
        }
        else
        {
            throw new Exception("What's this?");
        }
    }

    private void SetRectangleDiff(string[] lineComponents)
    {
        RectangleDiff rectDiff = new RectangleDiff(lineComponents);
        for (int x = rectDiff.X1Pos; x < rectDiff.X2Pos; x++)
        {
            for (int y = rectDiff.Y1Pos; y < rectDiff.Y2Pos; y++)
            {
                int index = CoordsToIndex(x, y);
                colorData[index] = rectDiff.ColorIndex;
            }
        }
    }

    private void SetPixelDiff(string[] lineComponents)
    {
        PixelDiff pixelDiff = new PixelDiff(lineComponents);
        int index = CoordsToIndex(pixelDiff.XPos, pixelDiff.YPos);
        colorData[index] = pixelDiff.ColorIndex;
    }

    private static int CoordsToIndex(int x, int y)
    {
        return MainViewerScript.ImageResolution * y + x;
    }

    private struct RectangleDiff
    {
        public uint ColorIndex { get; }
        public int X1Pos { get; }
        public int Y1Pos { get; }
        public int X2Pos { get; }
        public int Y2Pos { get; }

        public RectangleDiff(string[] lineComponents)
        {
            ColorIndex = (uint)Convert.ToInt32(lineComponents[1]);
            X1Pos = Convert.ToInt32(lineComponents[2]);
            Y1Pos = Convert.ToInt32(lineComponents[3]);
            X2Pos = Convert.ToInt32(lineComponents[4]);
            Y2Pos = Convert.ToInt32(lineComponents[5]);

            if(X1Pos > X2Pos)
            {
                int oldX2Pos = X2Pos;
                X2Pos = X1Pos;
                X1Pos = oldX2Pos;
            }

            if (Y1Pos > Y2Pos)
            {
                int oldY2Pos = Y2Pos;
                Y2Pos = Y1Pos;
                Y1Pos = oldY2Pos;
            }
        }
    }

    private struct PixelDiff
    {
        public uint ColorIndex { get; }
        public int XPos { get; }
        public int YPos { get; }

        public PixelDiff(string[] lineComponents)
        {
            ColorIndex = (uint)Convert.ToInt32(lineComponents[1]);
            XPos = Convert.ToInt32(lineComponents[2]);
            YPos = Convert.ToInt32(lineComponents[3]);
        }
    }
}
