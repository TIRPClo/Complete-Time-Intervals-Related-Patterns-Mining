using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZMiner
{
    public class Utils
    {
        string[] possibletypes = {
            "=",
            "S",
            "fi",
            "c",
            "o",
            "m",
            "<",
        };

        string[] R = { "=", "S", "fi", "c", "o", "m", "<" };

        public static string getRelation(EventInterval A, EventInterval B, Dictionary<string, double> constraints)
        {
            string relation = null;
            double epsilon = constraints["epsilon"];
            double gap = constraints["gap"];
            if(Math.Abs(B.getStart() - A.getStart()) <= epsilon)
            {
                if(Math.Abs(B.getEnd() - A.getEnd()) <= epsilon)
                {
                    relation = "=";
                }
                else if (B.getEnd() - A.getEnd() > epsilon)
                {
                    relation = "S";
                }
            }
            else if (Math.Abs(B.getEnd() - A.getEnd()) <= epsilon && B.getStart() - A.getStart() > epsilon)
            {
                relation = "fi";
            }
            else if (B.getStart() - A.getStart() > epsilon && A.getEnd() - B.getEnd() > epsilon)
            {
                relation = "c";
            }
            else if (A.getEnd() - B.getStart() > epsilon && B.getStart() - A.getStart() > epsilon)
            {
                relation = "o";
            }
            else if (Math.Abs(B.getStart() - A.getEnd()) <= epsilon)
            {
                relation = "m";
            }
            else if (B.getStart() - A.getEnd() > epsilon && (gap == 0 || B.getStart() - A.getEnd() < gap))
            {
                relation = "<";
            }
            return relation;
        }

        public static string[] createSTIsListKLF(string filePath)
        {
            TextReader tr = new StreamReader(filePath);
            string readLine = tr.ReadLine();
            //Move on until the significant start
            while (readLine != null && !readLine.StartsWith("startToncepts"))
            {
                readLine = tr.ReadLine();
            }
            if (!(readLine == "startToncepts" &&
                tr.ReadLine().StartsWith("numberOfEntities")))
            {
                throw new InvalidOperationException("incorrect file format");
            }
            //Start reading the entities
            List<string> stis_list = new List<string>();
            while (tr.Peek() >= 0)
            {
                readLine = tr.ReadLine();
                string[] mainDelimited = readLine.Split(';');
                string entityID = mainDelimited[0].Split(',')[0];
                readLine = tr.ReadLine();
                mainDelimited = readLine.Split(';');
                for (int i = 0; i < mainDelimited.Length - 1; i++)
                {
                    string[] tisDelimited = mainDelimited[i].Split(',');
                    int symbol = int.Parse(tisDelimited[2]);
                    stis_list.Add(entityID + " " + symbol + " " + 
                        int.Parse(tisDelimited[0]) + " " + int.Parse(tisDelimited[1]));
                }
            }
            tr.Close();
            string[] stis_list_array = stis_list.ToArray();
            return stis_list_array;
        }

        public static Tuple<int, int, int, double, double , Dictionary<string, 
            List<Tuple<int, int, int>>>> preprocess(string filename)
        {
            string[] lines = createSTIsListKLF(filename);
            List<List<string>> stis = new List<List<string>>();
            foreach(string line in lines)
            {
                List<string> l = new List<string>();
                l.Add(line);
                stis.Add(l);
            }
            List<int> distinct_events = new List<int>();
            List<string[]> new_list = new List<string[]>();
            Dictionary<string, List<Tuple<int, int, int>>> final_list = new Dictionary<string, List<Tuple<int, int, int>>>();
            Dictionary<string, int> timelength = new Dictionary<string, int>();
            for(int i = 0; i < stis.Count; i++)
            {
                new_list.Add(stis[i][0].Split(" "));
            }
            List<string> entities_ids = new List<string>();
            for (int i = 0; i < new_list.Count; i++)
            {
                if (!entities_ids.Contains(new_list[i][0]))
                {
                    entities_ids.Add(new_list[i][0]);
                }
            }
            for (int i = 0; i < entities_ids.Count; i++)
            {
                final_list.Add(entities_ids[i], new List<Tuple<int, int, int>>());
            }
            for (int i = 0; i < new_list.Count; i++)
            {
                final_list[new_list[i][0]].Add(new Tuple<int, int, int>(int.Parse(new_list[i][1]), 
                    int.Parse(new_list[i][2]), int.Parse(new_list[i][3])));
                if (!distinct_events.Contains(int.Parse(new_list[i][1])))
                {
                    distinct_events.Add(int.Parse(new_list[i][1]));
                }
                if (!timelength.ContainsKey(new_list[i][0]))
                {
                    timelength[new_list[i][0]] = 0;
                }
                timelength[new_list[i][0]] = Math.Max(timelength[new_list[i][0]], int.Parse(new_list[i][3]));
            }
            int tseq = final_list.Count;
            int tdis = distinct_events.Count;
            int tintv = new_list.Count;
            double aintv = new_list.Count / (double)final_list.Count;
            int sum = 0;
            foreach (KeyValuePair<string, int> e2t in timelength)
            {
                sum = sum + e2t.Value;
            }
            double avgtime = sum / (double)timelength.Keys.Count;
            return new Tuple<int, int, int, double, double, 
                Dictionary<string, List<Tuple<int, int, int>>>>(tseq, tdis, tintv, aintv, avgtime, final_list);
        }

        public static Dictionary<string, double> makeConstraints(string[] args, Dictionary<string, List<Tuple<int, int, int>>> dataset)
        {
            Dictionary<string, double> constraints = new Dictionary<string, double>();
            constraints["minSupPercent"] = 0;
            if (args.Length > 0)
            {
                constraints["minSupPercent"] = double.Parse(args[0]);
            }
            double dsp = dataset.Count * double.Parse(args[0]);
            int minsup = (int)dsp == dsp ? (int)dsp : (int)dsp + 1;
            constraints["minSup"] = minsup;
            constraints["epsilon"] = 0;
            if (args.Length > 1)
            {
                constraints["epsilon"] = double.Parse(args[1]);
            }
            constraints["gap"] = double.MaxValue;
            if (args.Length > 2)
            {
                constraints["gap"] = double.Parse(args[2]);
            }
            constraints["timeoutseconds"] = double.MaxValue;
            if (args.Length > 3)
            {
                constraints["timeoutseconds"] = double.Parse(args[3]);
            }
            if(constraints["timeoutseconds"] == -1)
            {
                constraints["timeoutseconds"] = double.MaxValue;
            }
            constraints["level"] = double.MaxValue;
            if (args.Length > 4)
            {
                constraints["level"] = double.Parse(args[4]);
            }
            if (constraints["level"] == -1)
            {
                constraints["level"] = double.MaxValue;
            }
            constraints["printF"] = 1;
            if (args.Length > 5 && args[5].ToLower() == "false")
            {
                constraints["printF"] = 0;
            }
            constraints["printL"] = 1;
            if (args.Length > 6 && args[6].ToLower() == "false")
            {
                constraints["printL"] = 0;
            }
            return constraints;
        }
    }
}
