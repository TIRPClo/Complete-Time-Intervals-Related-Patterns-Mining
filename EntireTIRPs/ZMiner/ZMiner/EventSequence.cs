using System;
using System.Collections.Generic;
using System.Text;

namespace ZMiner
{
    public class EventSequence
    {
        string id;
        List<EventInterval> sequences;

        public EventSequence(string id)
        {
            this.id = id;
            this.sequences = new List<EventInterval>();
        }

        public List<int> processAdding(List<Tuple<int, int, int>> eSeqList)
        {
            List<int> eventList = new List<int>();
            foreach(Tuple<int, int, int> eve in eSeqList){
                EventInterval newInterval = new EventInterval(eve.Item1, eve.Item2, eve.Item3);
                this.sequences.Add(newInterval);
                eventList.Add(newInterval.getLabel());
            }
            return eventList;
        }

        public string getDescription()
        {
            List<string> rst = new List<string>();
            foreach(EventInterval eve in this.sequences)
            {
                rst.Add(eve.getDescription());
            }
            string ret = "";
            foreach(string srst in rst)
            {
                ret = ret + ", " + srst;
            }
            ret = ret.Substring(2);
            return "(" + ret + ")";
        }

        public List<EventInterval> getSequences()
        {
            return this.sequences;
        }

        public void setSequences(List<EventInterval> new_val)
        {
            this.sequences = new_val;
        }

        public string getID()
        {
            return this.id;
        }
    }
}   