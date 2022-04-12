using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GZipUnzipper
{
    class Program
    {
        static string CompressedFileName = @"F:\rPlace2022\2022_place_canvas_history.csv.gzip"; // Replace this with the path to the source data from Reddit
        static string HappyFormatFileName = @"F:\rPlace2022\easy_formatted_data.txt"; // Target output file
        static string LinesCountFile = @"F:\rPlace2022\linesCount.txt";

        static string SortingFilesFolder = @"F:\rPlace2022\SortingFiles\";
        static string SortedFilesFolder = @"F:\rPlace2022\SortedFiles\";
        static string SortedDataFile = @"F:\rPlace2022\SortedData.txt";
        static int chunksCount = 1000;

        public static PlaceTwentyTwentyTwoColorPalette Colors = new PlaceTwentyTwentyTwoColorPalette();

        static void Main(string[] args)
        {
            GenerateDataSubset();
            //ValidateData();
            //ChunkTheData();
            //SortTheChunks();
            //ReorganizeTheData();
        }

        private static void GenerateDataSubset()
        {
            string bigOutputFolder = @"F:\rPlace2022\BigOutput\"; // Folder containing 20,000 images baked out
            string[] files = Directory.GetFiles(bigOutputFolder);
            string littleOutputFolder = @"F:\rPlace2022\LittleOutput\"; // Folder to contain 1,000 images

            for (int i = 0; i < 20000; i+=40)
            {
                int imageFileIndex = i / 40;
                string outputPath = littleOutputFolder + imageFileIndex.ToString("D5") + ".png";
                File.Copy(files[i], outputPath);
            }
        }

        private static void ReorganizeTheData()
        {
            string[] files = Directory.GetFiles(SortedFilesFolder);
            using FileStream outputFileStream = File.Create(SortedDataFile);
            using StreamWriter streamWriter = new StreamWriter(outputFileStream);
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    streamWriter.WriteLine(line);
                }
            }
        }

        private static void SortTheChunks()
        {
            string[] files = Directory.GetFiles(SortingFilesFolder);
            foreach (string file in files)
            {
                SortingFile(file);
            }
        }

        private static void SortingFile(string file)
        {
            List<string> lines = File.ReadAllLines(file).ToList();
            string[] sorted = lines.OrderBy(item => GetTicks(item)).ToArray();

            string outputPath = file.Replace("Sorting", "Sorted");
            File.WriteAllLines(outputPath, sorted);
        }

        private static object GetTicks(string item)
        {
            return Convert.ToInt64(item.Split(' ')[0]);
        }

        private static void ChunkTheData()
        {
            ChunkerTable table = new ChunkerTable(chunksCount);

            using FileStream data = File.Open(HappyFormatFileName, FileMode.Open);
            using StreamReader streamReader = new StreamReader(data);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                table.ChunkLine(line);
            }
            table.Dispose();
        }

        private class ChunkerTable
        {
            private readonly Chunker[] chunkers;
            private readonly int chunksCount;

            private const long start = 637844138503150000;
            private const long end = 637847144402070000;
            private const double span = end - start;
            private readonly double chunkSpan;

            public ChunkerTable(int chunksCount)
            {
                this.chunksCount = chunksCount;
                chunkSpan = span / (chunksCount + 1);
                chunkers = CreateChunkers();
            }

            private Chunker[] CreateChunkers()
            {
                Chunker[] ret = new Chunker[chunksCount];
                for (int i = 0; i < chunksCount; i++)
                {
                    ret[i] = new Chunker(i);
                }
                return ret;
            }

            public void ChunkLine(string line)
            {
                int chunksIndex = GetChunkIndex(line);
                chunkers[chunksIndex].AddLine(line);
            }

            private int GetChunkIndex(string line)
            {
                long ticks = Convert.ToInt64(line.Split(' ')[0]);
                if(ticks == start)
                {
                    Console.WriteLine("Let's see here...");
                }
                double fromStart = ticks - start;
                int ret = (int)(fromStart / chunkSpan);
                ret = Math.Min(ret, chunksCount - 1);
                return ret;
            }

            public void Dispose()
            {
                foreach (Chunker chunker in chunkers)
                {
                    chunker.Dispose();
                }
            }
        }

        private class Chunker
        {
            public string OutputPath { get; }
            private readonly FileStream outputFileStream;
            private readonly StreamWriter streamWriter;

            public Chunker(int index)
            {
                OutputPath = SortingFilesFolder + index.ToString("D5") + ".txt";
                outputFileStream = File.Create(OutputPath);
                streamWriter = new StreamWriter(outputFileStream);
            }

            internal void AddLine(string line)
            {
                streamWriter.WriteLine(line);
            }

            public void Dispose()
            {
                streamWriter.Dispose();
                outputFileStream.Dispose();
            }
        }

        private static void ValidateData()
        {
            using FileStream outputFileStream = File.Open(HappyFormatFileName, FileMode.Open);
            using StreamReader streamReader = new StreamReader(outputFileStream);
            string line;
            long minTicks = long.MaxValue;
            long maxTicks = 0;
            while ((line = streamReader.ReadLine()) != null)
            {
                long ticks = Convert.ToInt64(line.Split(' ')[0]);
                if (ticks > maxTicks)
                {
                    maxTicks = ticks;
                }
                if (ticks < minTicks)
                {
                    minTicks = ticks;
                }
            }
            DateTime maxTime = new DateTime(maxTicks);
            DateTime minTime = new DateTime(minTicks);
        }

        private static void WriteFormattedData()
        {
            using FileStream compressedFileStream = File.Open(CompressedFileName, FileMode.Open);
            using FileStream outputFileStream = File.Create(HappyFormatFileName);
            using StreamWriter streamWriter = new StreamWriter(outputFileStream);
            using GZipStream decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            using StreamReader streamReader = new StreamReader(decompressor);
            string line;
            streamReader.ReadLine(); // Skip the header line
            long linesCount = 0;
            while ((line = streamReader.ReadLine()) != null)
            {
                string newLine = GetHappyFormattedLine(line);
                streamWriter.WriteLine(newLine);
                linesCount++;
            }
            File.WriteAllText(LinesCountFile, linesCount.ToString());
        }

        private static string GetHappyFormattedLine(string rawLine)
        {
            string[] splitLine = rawLine.Split(',');
            long time = GetTime(splitLine[0]);
            int color = GetColor(splitLine[2]);

            int x1Coord = ToCoord(splitLine[3]);
            int y1Coord = ToCoord(splitLine[4]);
            if (splitLine.Length == 5)// Regular Line
            {
                return new RegularEdit(time, color, x1Coord, y1Coord).ToString();
            }
            else if (splitLine.Length == 7) // Moderator censoring
            {
                int x2Coord = ToCoord(splitLine[5]);
                int y2Coord = ToCoord(splitLine[6]);

                return new ModEdit(time, color, x1Coord, y1Coord, x2Coord, y2Coord).ToString();
            }
            else
            {
                throw new Exception("What's this?");
            }
        }

        private static long GetTime(string stringValue)
        {
            string[] firstSplit = stringValue.Split(' ');
            string[] calendarSplit = firstSplit[0].Split('-');
            int year = Convert.ToInt32(calendarSplit[0]);
            int month = Convert.ToInt32(calendarSplit[1]);
            int day = Convert.ToInt32(calendarSplit[2]);

            string[] timeSplit = firstSplit[1].Split(':');

            int hour = Convert.ToInt32(timeSplit[0]);
            int minute = Convert.ToInt32(timeSplit[1]);

            string[] secondSplit = timeSplit[2].Split('.');
            int second = Convert.ToInt32(secondSplit[0]);
            int millisecond = secondSplit.Length == 2 ? Convert.ToInt32(secondSplit[1]) : 0;

            DateTime time = new DateTime(year, month, day, hour, minute, second, millisecond);
            return time.Ticks;
        }

        private static int GetColor(string stringValue)
        {
            return (int)Colors.GetColorIndex(stringValue);
        }

        private static int ToCoord(string stringValue)
        {
            stringValue = stringValue.Replace("\"", "");
            return Convert.ToInt32(stringValue);
        }

        public class RegularEdit
        {
            public long Time { get; }
            public int ColorIndex { get; }
            public int XCoord { get; }
            public int YCoord { get; }

            public RegularEdit(long time, int color, int x1Coord, int y1Coord)
            {
                Time = time;
                ColorIndex = color;
                XCoord = x1Coord;
                YCoord = y1Coord;
            }
            public override string ToString()
            {
                return Time + " " + ColorIndex + " " + XCoord + " " + YCoord;
            }
        }

        public class ModEdit
        {
            public long Time { get; }
            public int ColorIndex { get; }
            public int X1Coord { get; }
            public int Y1Coord { get; }
            public int X2Coord { get; }
            public int Y2Coord { get; }

            public ModEdit(long time, int color, int x1Coord, int y1Coord, int x2Coord, int y2Coord)
            {
                Time = time;
                ColorIndex = color;
                X1Coord = x1Coord;
                Y1Coord = y1Coord;
                X2Coord = x2Coord;
                Y2Coord = y2Coord;
            }
            public override string ToString()
            {
                return Time + " " + ColorIndex + " " + X1Coord + " " + Y1Coord + " " + X2Coord + " " + Y2Coord;
            }
        }
        public class PlaceTwentyTwentyTwoColorPalette
        {
            private Dictionary<string, uint> table;

            public PlaceTwentyTwentyTwoColorPalette()
            {
                table = new Dictionary<string, uint>();
                table.Add("#FFFFFF", 0);
                table.Add("#FFF8B8", 1);
                table.Add("#FFD635", 2);
                table.Add("#FFB470", 3);
                table.Add("#FFA800", 4);
                table.Add("#FF99AA", 5);
                table.Add("#FF4500", 6);
                table.Add("#FF3881", 7);
                table.Add("#E4ABFF", 8);
                table.Add("#DE107F", 9);
                table.Add("#D4D7D9", 10);
                table.Add("#BE0039", 11);
                table.Add("#B44AC0", 12);
                table.Add("#9C6926", 13);
                table.Add("#94B3FF", 14);
                table.Add("#898D90", 15);
                table.Add("#811E9F", 16);
                table.Add("#7EED56", 17);
                table.Add("#6D482F", 18);
                table.Add("#6D001A", 19);
                table.Add("#6A5CFF", 20);
                table.Add("#51E9F4", 21);
                table.Add("#515252", 22);
                table.Add("#493AC1", 23);
                table.Add("#3690EA", 24);
                table.Add("#2450A4", 25);
                table.Add("#00CCC0", 26);
                table.Add("#00CC78", 27);
                table.Add("#00A368", 28);
                table.Add("#009EAA", 29);
                table.Add("#00756F", 30);
                table.Add("#000000", 31);
            }

            public uint GetColorIndex(string hex)
            {
                return table[hex];
            }
        }
    }
}
