using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Represents a temporal database
    public class TemporalDB
    {
        //The entries in the DB organized as a dictionary of event sequences each one with a unique id
        public Dictionary<string, EventSequence> seqs;

        public TemporalDB(string fn)
        {
            seqs = new Dictionary<string, EventSequence>();
            createSequencesKLF(fn);
        }

        //Reads the instances in the DB to extract a sorted event sequence for each sequence id ---------> KarmaLego Format
        public void createSequencesKLF(string filePath)
        {
            try
            {
                TextReader tr = new StreamReader(filePath);
                string readLine = tr.ReadLine();
                //Move on until significant start
                while (readLine != null && !readLine.StartsWith(Constants.FILE_START))
                {
                    readLine = tr.ReadLine();
                }
                if (!(readLine == Constants.FILE_START && tr.Peek() >= 0 && tr.ReadLine().StartsWith(Constants.FILE_NUM)))
                    throw new InvalidOperationException(Constants.FILE_FORMAT_ERR);
                //Start reading the entities
                Dictionary<string, int> syms_counter = new Dictionary<string, int>();
                EventSequence es = null;
                while (tr.Peek() >= 0) 
                {
                    readLine = tr.ReadLine();
                    string[] mainDelimited = readLine.Split(';');
                    string entityID = mainDelimited[0].Split(',')[0];
                    readLine = tr.ReadLine();
                    mainDelimited = readLine.Split(';');
                    es = new EventSequence();
                    seqs.Add(entityID, es);
                    syms_counter.Clear();
                    for (int i = 0; i < mainDelimited.Length - 1; i++)
                    {
                        string[] tisDelimited = mainDelimited[i].Split(',');
                        string symbol = tisDelimited[2];
                        if (syms_counter.ContainsKey(symbol))
                            syms_counter[symbol]++;
                        else
                            syms_counter.Add(symbol, 1);
                        //For a symbol x, 'x#num_of_x_occurrences_in_entity_till_now' (e.g. x#1, x#2 etc.) is added as a new event interval 
                        string symbol_with_index = symbol + Constants.DUP_SYM + (syms_counter[symbol] + "");
                        es.addIntervalBinary(new EventInterval(symbol_with_index, int.Parse(tisDelimited[0]), int.Parse(tisDelimited[1])));
                    }
                }
                tr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("The reading process has failed: {0}", e.ToString());
            }
        }

        //Transforms the DB to coincidence rep. by using the incision strategy
        public TransformedDB transformDB()
        {
            Dictionary<string, List<string>> trans_db = new Dictionary<string, List<string>>();
            Dictionary<string, Tuple<string, int>> initial_patterns = new Dictionary<string, Tuple<string, int>>();
            Dictionary<string, int> subids = new Dictionary<string, int>();
            foreach (KeyValuePair<string, EventSequence> entry in seqs)
            {
                trans_db.Add(entry.Key, IncisionStrategy.eventSeqToCoincidenceSeq(entry.Value));
                initial_patterns.Add(entry.Key, new Tuple<string, int>("", -1));
                subids.Add(entry.Key, 0);
            }
            return new TransformedDB(trans_db, initial_patterns, subids, null, 0);
        }
    }
}
