using System;
using System.Collections.Generic;
using System.Text;

namespace ZMiner
{
    public class Database
    {
        List<EventSequence> sequences;
        Dictionary<int, int> initialSupport;
        object frequentSecondElements;

        public Database(Dictionary<string, List<Tuple<int, int, int>>> database)
        {
            this.sequences = new List<EventSequence>();
            this.initialSupport = new Dictionary<int, int>();
            this.frequentSecondElements = new object();
            foreach (KeyValuePair<string, List<Tuple<int, int, int>>> id2seqList in database)
            {
                string id = id2seqList.Key;
                List<Tuple<int, int, int>> eSeqList = id2seqList.Value;
                EventSequence newSeq = new EventSequence(id);
                List<int> eventList = newSeq.processAdding(eSeqList);
                this.sequences.Add(newSeq);
                foreach (int label in eventList)
                {
                    if (!this.initialSupport.ContainsKey(label))
                    {
                        this.initialSupport.Add(label, 0);
                    }
                    this.initialSupport[label] = this.initialSupport[label] + 1;
                }
            }
        }

        public string getDescription()
        {
            List<string> rst = new List<string>();
            for (int i = 0; i < this.sequences.Count; i++)
            {
                rst.Add("eSeq " + i + " : " + this.sequences[i].getDescription());
            }
            string ret = "";
            foreach(string srst in rst)
            {
                ret += "\n" + srst;
            }
            return ret.Substring(1);
        }

        public Dictionary<int, int> getInitialSupport()
        {
            return this.initialSupport;
        }

        public void setInitialSupport(Dictionary<int, int> new_val)
        {
            this.initialSupport = new_val;
        }

        public List<EventSequence> getSequences()
        {
            return this.sequences;
        }
    }
}