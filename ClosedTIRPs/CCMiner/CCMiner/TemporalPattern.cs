using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    public class TemporalPattern
    {
        public int support;
        string coSeqRep;
        int length;
        double mean_hor_support;
        List<string> syms;
        Dictionary<int, string> ids;
        List<string> actual_ids;
        List<EventSequence> sequences;
        Dictionary<int, Dictionary<int, string>> relations;
        TransformedDB pDB;

        public TemporalPattern(string cs, int sup, TransformedDB projDB)
        {
            pDB = projDB;
            coSeqRep = cs;
            support = sup;
            setSyms();
            setCoSeqLength();
            actual_ids = pDB.patterns.Keys.ToList();
            ids = new Dictionary<int, string>();
            setIntervalsAndRelations();
            setMeanHorSupport();
        }

        //Return the needed representation for the pattern
        public override string ToString()
        {
            string res = "";
            res += length + " ";
            foreach (string sym in syms)
                res += sym + "-";
            res += " ";
            if (syms.Count == 1)
                res += "-.";
            else
            {
                for (int i = 0; i < syms.Count; i++)
                    for (int j = i + 1; j < syms.Count; j++)
                        res += relations[i][j] + ".";
            }
            res += " ";
            res += support + " ";
            res += Math.Round(mean_hor_support, 2) + " ";
            for (int i = 0; i < ids.Count; i++)
                res += ids[i] + " " + sequences[i].printIntervals() + " ";
            return res;
        }

        //Sets the symbols of the pattern
        private void setSyms()
        {
            syms = CoincidenceManager.getSymbols(coSeqRep);
        }

        //Sets the co seq length
        private void setCoSeqLength()
        {
            length = syms.Count;
        }

        //Sets the inervals 
        private void setIntervalsAndRelations()
        {
            relations = new Dictionary<int, Dictionary<int, string>>();
            TemporalDB db = CCMiner.mainDB;
            sequences = new List<EventSequence>();
            for (int k = 0; k < actual_ids.Count; k++)
            {
                string t_id = TransformedDB.getID(actual_ids[k]);
                ids.Add(k, t_id);
                string id = t_id;
                EventSequence seq = new EventSequence();
                List<string> seq_syms = CoincidenceManager.
                    getPatternSymbols(pDB.patterns[actual_ids[k]].Item1);
                foreach (string sym in seq_syms)
                    seq.ints.Add(db.seqs[id].getInterval(sym));
                sequences.Add(seq);
                if (k == 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        relations.Add(i, new Dictionary<int, string>());
                        for (int j = i + 1; j < length; j++)
                        {
                            EventInterval ei1 = seq.ints[i];
                            EventInterval ei2 = seq.ints[j];
                            string rel = getRelation(ei1, ei2);
                            relations[i].Add(j, rel);
                        }
                    }
                }
            }
        }

        //Returns Allen's suitable relation for the two intervals
        private string getRelation(EventInterval ei1, EventInterval ei2)
        {
            if (ei1.fin_time < ei2.st_time) return Constants.ALLEN_BEFORE;
            if (ei1.fin_time == ei2.st_time) return Constants.ALLEN_MEET;
            if (ei1.st_time == ei2.st_time && ei1.fin_time == ei2.fin_time) return Constants.ALLEN_EQUAL;
            if (ei1.st_time < ei2.st_time && ei1.fin_time > ei2.fin_time) return Constants.ALLEN_CONTAIN;
            if (ei1.st_time == ei2.st_time && ei1.fin_time < ei2.fin_time) return Constants.ALLEN_STARTS;
            if (ei1.st_time < ei2.st_time && ei1.fin_time == ei2.fin_time) return Constants.ALLEN_FINISHBY;
            return Constants.ALLEN_OVERLAP;
        }

        //Sets the mean vertical support of the pattern 
        private void setMeanHorSupport()
        {
            if (length == 1)
            {
                mean_hor_support = support;
                return;
            }
            mean_hor_support = (double)ids.Count / support;
        }
    }
}
