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

    private const string dataLocation = @"F:\rPlace2022\easy_formatted_data.txt"; // Set this to your local data location
    public ComputeBuffer PixelIndexValuesBuffer { get; private set; }

    public int CurrentStepIndex { get; private set; }

    public int TotalSteps { get; } = 2000; // Determines how many PNGs you want to produce

    private long timeStepSize;

    private FileStream fileStream;
    private StreamReader fileReader;

    public GZipSource()
    {
        PixelIndexValuesBuffer = new ComputeBuffer(2048 * 2048, sizeof(uint));
        fileStream = File.Open(dataLocation, FileMode.Open);
        fileReader = new StreamReader(fileStream);

        timeStepSize = (TimeEnd - TimeStart) / TotalSteps;
    }

    public void Dispose()
    {
        fileStream.Dispose();
        fileReader.Dispose();
    }

    public void SetNextStep()
    {
        throw new NotImplementedException();
    }
}
