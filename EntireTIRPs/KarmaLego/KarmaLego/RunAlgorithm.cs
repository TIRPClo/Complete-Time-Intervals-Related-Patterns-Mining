using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace KarmaLego
{
    public class RunAlgorithm
    {
        const int METHOD_KL = 0;
        const int METHOD_LL = 1;
        const int Karma_Index = 0;
        const int Dharma_Index = 1;

        //Main program
        public static void Main(string[] args)
        {
            //Execution parameters:
            //number of entities
            int num_entities = 65;
            //minimum vertical support percentage
            double min_support = 50;
            //maximal gap
            int maximal_gap = 30;
            //dataset name
            string file_path = "Datasets/asl/asl";
            run_algorithm(num_entities, min_support, maximal_gap, file_path);
        }

        //Execute the algorithm
        public static void run_algorithm(int num_entities, double min_support, int maximal_gap, string file_path)
        {
            int index = Dharma_Index;
            int method = METHOD_KL;
            //set directory, input & output files
            string inputFile = file_path + ".csv";
            //set variables 
            bool setHS1 = false;
            int epsilon = 0;
            long dt1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            string rt = "";
            string outFile = file_path + "-support-" + min_support + "-maxgap-" + maximal_gap + ".txt";
            File.Delete(outFile);
            //Select whether to use KarmaLego or the LL algorithm
            if (method == METHOD_KL)
            {
                //Build either the karma or dharma index according to the index flag
                if (index == Karma_Index)
                {
                    Karma k = new Karma(epsilon, maximal_gap, min_support, inputFile, setHS1);
                    Lego.RunLego(k, outFile, KLC.KL_TRANS_YES, k.getRelStyle());
                }
                else
                {
                    Dharma d = new Dharma(int.MaxValue, true, KLC.dharma_relVecSymVecSymDic, true, KLC.forwardMining,
                        KLC.RELSTYLE_ALLEN7, epsilon, maximal_gap, min_support, inputFile, true, setHS1, 1, ref rt);
                    DLego.RunLego(d, outFile, KLC.KL_TRANS_YES, d.getRelStyle());
                }
            }
            Console.WriteLine("---------------");
            Console.WriteLine("---------------");
            Console.WriteLine("---------------");
            long dt2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            long diff = dt2 - dt1;
            Console.WriteLine(diff);
            Thread.Sleep(2000);
            //Write running stats to file
            string[] to_write = new string[1];
            to_write[0] = diff + "";
            System.IO.File.WriteAllLines(outFile + "-stats.txt", to_write);
            //outputConverter(outFile);
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