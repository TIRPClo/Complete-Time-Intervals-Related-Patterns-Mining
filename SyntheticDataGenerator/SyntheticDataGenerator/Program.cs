using System;
using System.Collections.Generic;
using System.Text;

namespace SyntheticDataGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string file_name = "sample_generated_dataset.csv";
            int minE = 100, maxE = 200, minSTIs = 100, maxSTIs = 200, maxDuration = 20, maxTimestamp = 1000, maxS = 20;
            Console.WriteLine("Starting Generation of Dataset");
            DatasetsGenerator.generateDataset(file_name, minE, maxE, minSTIs, maxSTIs, maxDuration, maxTimestamp, maxS);
            Console.WriteLine("Finished Generation of Dataset");
        }

    }
}