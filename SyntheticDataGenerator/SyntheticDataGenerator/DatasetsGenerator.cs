using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace SyntheticDataGenerator
{
    class MyRandom : Random
    {
        public override Int32 Next(Int32 minValue, Int32 maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");
            if (minValue == maxValue) return minValue;
            Int64 diff = maxValue - minValue;
            while (true)
            {
                byte[] _uint32Buffer = new byte[4];
                RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
                _rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(minValue + (rand % diff));
                }
            }
        }
    }

    public class DatasetsGenerator
    {
        public static void generateDataset(string file_name, int minE, int maxE, int minSTIs, int maxSTIs, 
            int maxDuration, int maxTimestamp, int maxS)
        {
            Random rnd_entities = new Random();
            int num_entities = rnd_entities.Next(minE, maxE);
            int extra_time = 20;
            string[] text = new string[num_entities * 2 + 3];
            text[0] = "";
            text[1] = "startToncepts";
            text[2] = "numberOfEntities," + num_entities;
            for (int i = 0; i < num_entities; i++)
            {
                Dictionary<int, List<string>> syms_dic = new Dictionary<int, List<string>>();
                text[3 + 2 * i] = (i + 1) + "," + (i + 1) + ";";
                Random rnd_intervals = new MyRandom();
                int num_intervals = rnd_intervals.Next(minSTIs, maxSTIs);
                string[] intervals = new string[num_intervals];
                for (int j = 0; j < num_intervals; j++)
                {
                    while (true)
                    {
                        Random rnd_start = new MyRandom();
                        int start = rnd_start.Next(1, maxTimestamp - extra_time);
                        Random rnd_fin = new MyRandom();
                        int fin = rnd_fin.Next(start + 1, start + 1 + maxDuration);
                        Random rnd_symbol = new MyRandom();
                        int sym = rnd_symbol.Next(1, maxS);
                        if (!overlapSame(syms_dic, sym, start, fin))
                        {
                            if (!syms_dic.ContainsKey(sym))
                            {
                                syms_dic.Add(sym, new List<string>());
                            }
                            syms_dic[sym].Add(start + "," + fin);
                            intervals[j] = start + "," + fin + "," + sym;
                            break;
                        }
                        syms_dic[sym].Add(start + "," + fin);
                    }
                }
                sortIntervals(intervals);
                text[3 + (2 * i + 1)] = writeIntervals(intervals);
            }
            File.WriteAllLines(file_name, text);
        }

        private static bool overlapSame(Dictionary<int, List<string>> syms_dic, int sym, int start, int fin)
        {
            if (!syms_dic.ContainsKey(sym))
            {
                return false;
            }
            List<string> times = syms_dic[sym];
            foreach (string time in times)
            {
                int time_start = int.Parse(time.Split(',')[0]);
                int time_fin = int.Parse(time.Split(',')[1]);
                if (start >= time_start && start <= time_fin)
                {
                    return true;
                }
                if (fin >= time_start && start <= time_fin)
                {
                    return true;
                }
                if (start <= time_start && fin >= time_fin)
                {
                    return true;
                }
            }
            return false;
        }

        private static void sortIntervals(string[] intervals)
        {
            for (int i = 0; i < intervals.Length - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    if (compareTwoIntervals(intervals[j - 1], intervals[j]) > 0)
                    {
                        string temp = intervals[j - 1];
                        intervals[j - 1] = intervals[j];
                        intervals[j] = temp;
                    }
                }
            }
        }

        private static int compareTwoIntervals(string int1, string int2)
        {
            string[] parts1 = int1.Split(',');
            string[] parts2 = int2.Split(',');
            int st_dif = int.Parse(parts1[0]) - int.Parse(parts2[0]);
            if (st_dif != 0) return st_dif;
            int fn_dif = int.Parse(parts1[2]) - int.Parse(parts2[1]);
            if (fn_dif != 0) return fn_dif;
            return int.Parse(parts1[2]) - int.Parse(parts2[2]);
        }

        private static string writeIntervals(string[] intervals)
        {
            string line = "";
            foreach (string interval in intervals)
            {
                line += interval + ";";
            }
            return line;
        }

    }
}