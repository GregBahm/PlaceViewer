using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class DiffLoader
{
    private int TimestampInSeconds;
    private int XPos;
    private int YPos;
    private uint ColorId;

    private byte[] data;
    private string compressedDataPath = Application.dataPath + @"\diffs.bin.gz";
    private int index = 0;

    public int CurrentDiffIndex { get { return index / 16; } }
    public int TotalDiffCount { get { return data.Length / 16; } }

    private uint[] ColorData;
    private ComputeBuffer currentImage;
    

    public DiffLoader()
    {
        data = DecompressGZip(compressedDataPath);
        ColorData = new uint[1024 * 1024];
        currentImage = new ComputeBuffer(1024 * 1024, sizeof(uint));
	}
	
	public ComputeBuffer GetNextTimeslice()
    {
        int startingTmestamp = TimestampInSeconds;

        while(TimestampInSeconds == startingTmestamp)
        {
            SetPixel();
            GetNextDiff();
        }
        currentImage.SetData(ColorData);
        return currentImage;
	}

    private void SetPixel()
    {
        int index = XPos * 1024 + YPos;
        ColorData[Mathf.Min(index, ColorData.Length - 1)] = ColorId;
    }

    private void GetNextDiff()
    {
        TimestampInSeconds = BitConverter.ToUInt16(data, index);
        index += 4;
        XPos = 1024 - BitConverter.ToUInt16(data, index);
        index += 4;
        YPos = BitConverter.ToUInt16(data, index);
        index += 4;
        ColorId = BitConverter.ToUInt16(data, index);
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

