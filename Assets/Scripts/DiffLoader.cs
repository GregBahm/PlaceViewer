using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

// DiffLoader consumes the r/Place diffs.bin.gz provided by Reddit
public class DiffLoader : ISourceDataFeeder
{
    private int timestampInSeconds;
    private int xPos;
    private int yPos;
    private uint colorId;

    private byte[] data;
    private string compressedDataPath = Application.dataPath + @"\diffs.bin.gz";
    private int index = 0;

    public int CurrentStepIndex { get { return index / 16; } }
    public int TotalSteps { get { return data.Length / 16; } }

    private uint[] colorData;
    private ComputeBuffer currentImage;
    

    public DiffLoader()
    {
        data = DecompressGZip(compressedDataPath);
        colorData = new uint[1024 * 1024];
        currentImage = new ComputeBuffer(1024 * 1024, sizeof(uint));
	}
	
	public ComputeBuffer GetNextTimeslice()
    {
        int startingTmestamp = timestampInSeconds;

        while(timestampInSeconds == startingTmestamp)
        {
            SetPixel();
            GetNextDiff();
        }
        currentImage.SetData(colorData);
        return currentImage;
	}

    public void Dispose()
    {
        currentImage.Dispose();
    }

    private void SetPixel()
    {
        int index = xPos * 1024 + yPos;
        colorData[Mathf.Min(index, colorData.Length - 1)] = colorId;
    }

    private void GetNextDiff()
    {
        timestampInSeconds = BitConverter.ToUInt16(data, index);
        index += 4;
        xPos = BitConverter.ToUInt16(data, index);
        index += 4;
        yPos = BitConverter.ToUInt16(data, index);
        index += 4;
        colorId = BitConverter.ToUInt16(data, index);
        index += 4;
    }

    static byte[] DecompressGZip(string filePath)
    {
        byte[] gzip = File.ReadAllBytes(filePath);
        using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
            CompressionMode.Decompress))
        {
            const int size = 4096;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                return memory.ToArray();
            }
        }
    }
}

// ScreenshotLoader consumes a library of .pngs, if a gzip of diffs has not been provided 
public class ScreenshotLoader : ISourceDataFeeder
{
    public const string RawImagesFolder = @"F:\rPlace2022\latest\images_single\"; // Replace this with yer local folder
    private string[] images;
    private int index = 0;
    private uint[] colorData;
    private ComputeBuffer currentImage;

    public int CurrentStepIndex => throw new NotImplementedException();

    public int TotalSteps => throw new NotImplementedException();

    public ScreenshotLoader()
    {
        colorData = new uint[1024 * 1024];
        currentImage = new ComputeBuffer(1024 * 1024, sizeof(uint));
        images = Directory.GetFiles(RawImagesFolder).OrderBy(item => item).ToArray(); // Alphabetical order
    }

    public ComputeBuffer GetNextTimeslice()
    {
        throw new NotImplementedException();
        index++;
    }

    public void Dispose()
    {
        currentImage.Dispose();
    }
}

public interface ISourceDataFeeder
{
    ComputeBuffer GetNextTimeslice();
    int CurrentStepIndex { get; }
    int TotalSteps { get; }
    void Dispose();
}