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

        public static PlaceTwentyTwentyTwoColorPalette Colors = new PlaceTwentyTwentyTwoColorPalette();

        static void Main(string[] args)
        {
            using FileStream compressedFileStream = File.Open(CompressedFileName, FileMode.Open);
            using FileStream outputFileStream = File.Create(HappyFormatFileName);
            using StreamWriter streamWriter = new StreamWriter(outputFileStream);
            using GZipStream decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            using StreamReader streamReader = new StreamReader(decompressor);
            string line;
            streamReader.ReadLine(); // Skip the header line
            long linesCount = 0;
            while((line = streamReader.ReadLine()) != null)
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
