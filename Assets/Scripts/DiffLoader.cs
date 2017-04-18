using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class DiffLoader
{
    private int _timestampInSeconds;
    private int _xPos;
    private int _yPos;
    private uint _colorId;

    private byte[] _data;
    private string _compressedDataPath = Application.dataPath + @"\diffs.bin.gz";
    private int _index = 0;

    public int CurrentDiffIndex { get { return _index / 16; } }
    public int TotalDiffCount { get { return _data.Length / 16; } }

    private uint[] _colorData;
    private ComputeBuffer _currentImage;
    

    public DiffLoader()
    {
        _data = DecompressGZip(_compressedDataPath);
        _colorData = new uint[1024 * 1024];
        _currentImage = new ComputeBuffer(1024 * 1024, sizeof(uint));
	}
	
	public ComputeBuffer GetNextTimeslice()
    {
        int startingTmestamp = _timestampInSeconds;

        while(_timestampInSeconds == startingTmestamp)
        {
            SetPixel();
            GetNextDiff();
        }
        _currentImage.SetData(_colorData);
        return _currentImage;
	}

    private void SetPixel()
    {
        int index = _xPos * 1024 + _yPos;
        _colorData[Mathf.Min(index, _colorData.Length - 1)] = _colorId;
    }

    private void GetNextDiff()
    {
        _timestampInSeconds = BitConverter.ToUInt16(_data, _index);
        _index += 4;
        _xPos = 1024 - BitConverter.ToUInt16(_data, _index);
        _index += 4;
        _yPos = BitConverter.ToUInt16(_data, _index);
        _index += 4;
        _colorId = BitConverter.ToUInt16(_data, _index);
        _index += 4;
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

