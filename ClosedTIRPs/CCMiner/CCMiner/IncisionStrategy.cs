using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Responsible for the incision strtegy implementation 
    public class IncisionStrategy
    {
        //The endtime list to fill
        static List<EndTime> endtime_list;
        static int slices_in_co = 0;
        static int slices_in_former = 0;
        static int slices_in_later = 0;
        //Converts the event sequence to a string that represents them in coincidence rep.
        public static List<string> eventSeqToCoincidenceSeq(EventSequence q)
        {
            endtime_list = new List<EndTime>();
            //the coincidence sequence we build
            List<string> coseq = new List<string>();
            //the previous element in the end time list
            EndTime last_endtime = null;
            //fills the endtime list with the values derived from the given sequence 
            fillEndTimeList(q);
            //the end time list is now full, we need to traverse it entry by entry
            List<string> coincidence;
            bool isMeet = false;
            string pre = "";
            foreach (EndTime t in endtime_list)
            {
                isMeet = false;
                pre = "";
                coincidence = new List<string>();
                if (t.type == Constants.START)
                {
                    getSlicesForSyms(coincidence, t.syms, Constants.ST_REP);
                    if (last_endtime != null && last_endtime.time == t.time)
                    {    //meet slice insertion
                        isMeet = true;
                        pre += Constants.MEET_REP;
                    }
                }
                else
                {
                    getSlicesForSyms(coincidence, t.syms, Constants.FIN_REP);
                }
                //Check for a need to add '(' ')' 
                string coincidence_str = coincidence.Aggregate("", (acc, x) => acc + x);
                if (slices_in_co > 1 && isMeet)
                    coincidence_str = "(" + coincidence_str + ")";
                string toAdd = pre + coincidence_str;
                if (toAdd.Length > 0)
                    coseq.Add(toAdd);
                //and now, update the previous entry to be the one we've just looked at
                last_endtime = t;
            }
            return coseq;
        }

        //Looks for self-incision in the accumulated coincidence and fills the form & lat coincidences
        public static bool isSelfIncision(List<string> co, ref List<string> form, ref List<string> lat, 
            int fin_ind, bool start, bool end)
        {
            bool found = false;
            /*for (int i = 0; i < fin_ind && !found; i++)
            {
                for (int j = fin_ind; j < co.Count; j++)
                    if (co[i].CompareTo(co[j].Substring(0, co[j].Length - 1) + Constants.ST_REP) == 0)
                        found = true;
            }*/
            found = start && end;
            if (found)
            {
                form = co.GetRange(0, fin_ind);
                slices_in_former = fin_ind;
                lat = co.GetRange(fin_ind, co.Count - fin_ind);
                slices_in_later = co.Count - fin_ind;
                return true;
            }
            slices_in_co = co.Count;
            return false;
        }

        //Adds starting/finishing (by type) slices for the symbols in an endtime object
        private static void getSlicesForSyms(List<string> co, string syms, char type)
        {
            string[] symbols = syms.Split(Constants.SEP_SYM);
            int len = symbols.Length;
            for (int i = 0; i < len; i++)
                co.Add( symbols[i] + (type + ""));
        }

        //Sorts the coincidence by lexicographic order
        public static void sortByLex(List<string> co)
        {
            if (co == null)
                return;
            //Sort lexicographically
            Sorter.MergeSort(co, compareSyms);
        }

        //Intializes endtime_list according to the event sequence q
        public static void fillEndTimeList(EventSequence q)
        {
            EndTime et = null;
            foreach (EventInterval ei in q.ints)
            {
                et = new EndTime(ei.sym, ei.st_time, Constants.START);
                endTimeInsert(et);
                et = new EndTime(ei.sym, ei.fin_time, Constants.FINISH);
                endTimeInsert(et);
            }
            sortAndMergeEndTimeList();
        }

        //Inserts the next element to the endtime_list
        private static void endTimeInsert(EndTime toInsert)
        {
            endtime_list.Add(toInsert);
        }

        //Sorts the entime_list
        private static void sortAndMergeEndTimeList()
        {
            //sort
            Sorter.MergeSortEndTime(endtime_list, compareEndTimes);
            //merge
            int i = 0;
            int j = 0;
            int len = endtime_list.Count;
            while (i < len)
            {
                for (j = i + 1; j < len && endtime_list[i].time == endtime_list[j].time &&
                    endtime_list[i].type == endtime_list[j].type; j++)
                {
                    endtime_list[i].addSymToSyms(endtime_list[j].syms);
                    endtime_list.Remove(endtime_list[j]);
                    len = endtime_list.Count;
                    j--;
                }
                len = endtime_list.Count;
                i++;
            }
        }

        //Compares the to syms as for KL
        public static int compareSyms(string sym1, string sym2)
        {
            int s1 = int.Parse(sym1.Substring(0, sym1.IndexOf(Constants.DUP_SYM)));
            int s2 = int.Parse(sym2.Substring(0, sym2.IndexOf(Constants.DUP_SYM)));
            if (s1 != s2) return s1 - s2;
            int i1_start = sym1.IndexOf(Constants.DUP_SYM) + 1;
            int i1 = int.Parse(sym1.Substring(i1_start, sym1.Length - i1_start));
            int i2_start = sym2.IndexOf(Constants.DUP_SYM) + 1;
            int i2 = int.Parse(sym2.Substring(i2_start, sym2.Length - i2_start));
            return i1 - i2;
        }

        //Compares two end times
        private static int compareEndTimes(EndTime e1, EndTime e2)
        {
            int time = e1.time - e2.time;
            if (time != 0) return time;
            bool type = e1.type == e2.type;
            if(!type)
            {
                if (e1.type == Constants.FINISH) return -1;
                else return 1;
            }
            return compareSyms(e1.syms, e2.syms);
        }
    }
}
