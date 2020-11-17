using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Represents an event sequence which is actually a sequence of event intervals
    public class EventSequence
    {
        //The sequence of event intervals
        public List<EventInterval> ints;
        //Mapping between sym to event interval
        public Dictionary<string, EventInterval> intsdic;

        public EventSequence()
        {
            ints = new List<EventInterval>();
            intsdic = new Dictionary<string, EventInterval>();
        }
        
        //Adds the event interval to the list using binary search
        public void addIntervalBinary(EventInterval ei)
        {
            intsdic.Add(ei.sym, ei);
            int min = 0;
            int max = ints.Count - 1;
            int mid = 0;
            while (min <= max)
            {
                mid = (min + max) / 2;
                if (ei.Compare(ints[mid]) < 0)
                    max = mid - 1;
                else
                    min = mid + 1;
            }
            ints.Insert(min, ei);
            //ints.Add(ei);
        }

        //Returns the event interval corresponding to the given symbol
        public EventInterval getInterval(string sym)
        {
            //if (intsdic.ContainsKey(sym))
                return intsdic[sym];
            //return null;
        }

        //Returns a string representation of the sequence intervals
        public string printIntervals()
        {
            string res = "";
            foreach (EventInterval ei in ints)
                res += "[" + ei.st_time + "-" + ei.fin_time + "]";
            return res;
        }
    }
}
