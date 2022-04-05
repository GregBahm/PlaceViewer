using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

// DiffSource consumes the r/Place diffs.bin.gz provided by Reddit
// Outputs a compute buffer of color index values
public class DiffSource : IRawDataSource
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
    public ComputeBuffer PixelIndexValuesBuffer { get; private set; }
    

    public DiffSource()
    {
        data = DecompressGZip(compressedDataPath);
        colorData = new uint[1024 * 1024];
        PixelIndexValuesBuffer = new ComputeBuffer(1024 * 1024, sizeof(uint));
	}
	
	public void SetNextStep()
    {
        int startingTmestamp = timestampInSeconds;

        while(timestampInSeconds == startingTmestamp)
        {
            SetPixel();
            GetNextDiff();
        }
        PixelIndexValuesBuffer.SetData(colorData);
	}

    public void Dispose()
    {
        PixelIndexValuesBuffer.Dispose();
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
