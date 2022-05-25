using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using VertTIRP.ti;
using VertTIRP.tirp;

namespace VertTIRP
{
    class RunAlgorithm
    {
        static void Main(string[] args)
        {
            int num_entities = 65;
            double ver_sup = 50;
            double hor_sup = 0;
            int eps = 0;
            bool dummy = false;
            bool trans = true;
            string filepath = "Datasets/asl/asl.csv";
            bool avoid_same_var_states = false;
            uint ming = 0;
            uint maxg = 30;
            string result_file_name = filepath + "-support-" + ver_sup + "-maxgap-" + maxg + ".txt";
            uint mind = 0;
            uint maxd = 3155695200;
            string ps = "mocfbes";

            long dt1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            File.Delete(result_file_name);

            //Run Algorithm
            int tirp_count = VertTIRP.runVertTIRP(filepath, result_file_name, num_entities, (double)ver_sup/100, ming, maxg, mind, maxd, eps, dummy, ps, trans, avoid_same_var_states);

            long dt2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            long diff = dt2 - dt1;
            Console.WriteLine("Finished Running: support - " + ver_sup + " gap - " + maxg);
            Console.WriteLine(diff);
            Thread.Sleep(2000);
            string[] to_write = new string[1];
            to_write[0] = diff + "";
            File.WriteAllLines(result_file_name + "-stats.txt", to_write);
            //outputConverter(result_file_name);
        }

        //Sort output for comparisons
        public static void outputConverter(string file)
        {
            TextReader tr = new StreamReader(file);
            string readLine = tr.ReadLine();
            //Move on until significant start
            List<string> output = new List<string>();
            while (readLine != null)
            {
                output.Add(readLine);
                readLine = tr.ReadLine();
            }
            output.Sort();
            string[] outA = output.ToArray();
            File.WriteAllLines(file + "_converted.txt", outA);
        }

    }
}
