using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;

namespace VertTIRP.tirp
{
    public class TIRPstatistics
    {

        public Dictionary<string, Dictionary<int, List<TIRP>>> sequence_events_tirps_dict;
        public int sum_ver_supp;
        Dictionary<string, int> sum_hor_per_seq;
        string last_modified;
        List<double> sum_mean_duration;
        List<int> n_instances_per_seq;
        List<double> mean_duration;

        public TIRPstatistics()
        {
            this.sequence_events_tirps_dict = new Dictionary<string, Dictionary<int, List<TIRP>>>();
            this.sum_ver_supp = 0;
            this.sum_hor_per_seq = new Dictionary<string, int>();
            this.last_modified = null;
            this.sum_mean_duration = new List<double>();
            this.n_instances_per_seq = new List<int>();
            this.mean_duration = new List<double>();

        }

        public int append_tirp(string seq_id, int eid, TIRP tirp)
        {
            if (this.sequence_events_tirps_dict.ContainsKey(seq_id))
            {
                if (!this.sequence_events_tirps_dict[seq_id].ContainsKey(eid))
                {
                    this.sequence_events_tirps_dict[seq_id][eid] = new List<TIRP>();
                }
                this.sequence_events_tirps_dict[seq_id][eid].Add(tirp);
                this.sum_mean_duration[this.sum_mean_duration.Count - 1] += tirp.get_duration();
                this.n_instances_per_seq[this.n_instances_per_seq.Count - 1] += 1;
            }
            else
            {
                this.sum_mean_duration.Add(tirp.get_duration());
                this.n_instances_per_seq.Add(1);
                this.sequence_events_tirps_dict[seq_id] = new Dictionary<int, List<TIRP>>();
                this.sequence_events_tirps_dict[seq_id][eid] = new List<TIRP>();
                this.sequence_events_tirps_dict[seq_id][eid].Add(tirp);
                this.sum_ver_supp += 1;
                this.sum_hor_per_seq[seq_id] = 0;
            }

            this.last_modified = seq_id;
            this.sum_hor_per_seq[seq_id] += 1;
            return this.sum_ver_supp;
        }
        public double get_mean_hor_support(Dictionary<string, int> events_per_sequence)
        {
            double rel_sum = 0.0;
            foreach (KeyValuePair<string, int> sid_sumv in this.sum_hor_per_seq)
            {
                string sid = sid_sumv.Key;
                int sumv = sid_sumv.Value;
                rel_sum += (double)sumv / events_per_sequence[sid];
            }

            return rel_sum / this.sum_ver_supp;
        }

        public double get_ver_support(int n_sequences)
        {
            return (double)this.sum_ver_supp / n_sequences;
        }

        public List<double> get_mean_duration()
        {
            if (this.last_modified != null) 
            {
                for (int i = 0; i < this.sum_mean_duration.Count; i++)
                {
                    double sum = this.sum_mean_duration[i];
                    int n_inst = this.n_instances_per_seq[i];
                    this.mean_duration.Add(sum / n_inst);
                }
                return this.mean_duration;
            }
            else
            {
                List<double> l = new List<double>();
                l.Add(0);
                return l;
            }
        }

        public string get_mean_of_means_duration(string units = "hours")
        {
            List<double> mean_durations = this.get_mean_duration();
            double sum = 0;
            for (int i = 0; i < mean_durations.Count; i++)
            {
                sum += mean_durations[i];
            }
            return (sum / mean_duration.Count).ToString();
        }

        public string getTIRPFullRepresentation(int eps, uint min_gap, uint max_gap)
        {
            int support = this.sum_ver_supp;
            TIRP t = new List<List<TIRP>>(new List<Dictionary<int, List<TIRP>>>(this.sequence_events_tirps_dict.Values)[0].Values)[0][0];

            int length = t.ti.Count;
            TI[] ints = new TI[length];
            for (int  i = 0; i < length; i++)
            {
                ints[i] = t.ti[i];
            }
            Array.Sort(ints);
            string res = "";
            res += length + " ";
            for (int i = 0; i < length; i++)
            {
                res = res + ints[i].sym + "-";
            }
            res += " ";
            if (length == 1)
            {
                res += "-.";
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    for (int j = i + 1; j < length; j++)
                    {
                        res += getRelation(ints[i], ints[j], eps, min_gap, max_gap) + ".";
                    }
                }
            }
            res += " ";
            res += support + " ";
            int instances = 0;
            foreach (KeyValuePair<string, int> sid_instances in this.sum_hor_per_seq)
            {
                instances += sid_instances.Value;
            }
            res += (length == 1 ? support : Math.Round((double)instances / support, 2)) + " ";
            foreach (KeyValuePair<string, Dictionary<int, List<TIRP>>> sid_instances in this.sequence_events_tirps_dict)
            {
                foreach (KeyValuePair<int, List<TIRP>> tirps in sid_instances.Value)
                {
                    foreach (TIRP tirp in tirps.Value)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            ints[i] = tirp.ti[i];
                        }
                        Array.Sort(ints);
                        res += sid_instances.Key + " " + printIntervals(ints) + " ";
                    }
                }
            }
            return res;
        }

        private static string printIntervals(TI[] intervals)
        {
            string res = "";
            foreach (TI t in intervals)
            {
                res += t.printInterval();
            }
            return res;
        }

        private static char getRelation(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if (allen_relationsEPS.before(a, b, eps, min_gap, max_gap).Item2 == 3) return allen_relationsEPS.ALLEN_BEFORE_PRINT;
            if (allen_relationsEPS.meets(a, b, eps, min_gap, max_gap).Item2 == 3) return allen_relationsEPS.ALLEN_MEET_PRINT;
            if (allen_relationsEPS.equal(a, b, eps, min_gap, max_gap).Item2 == 3) return allen_relationsEPS.ALLEN_EQUAL_PRINT;
            if (allen_relationsEPS.contains(a, b, eps, min_gap, max_gap).Item2 == 3) return allen_relationsEPS.ALLEN_CONTAIN_PRINT;
            if (allen_relationsEPS.starts(a, b, eps, min_gap, max_gap).Item2 == 3) return allen_relationsEPS.ALLEN_STARTS_PRINT;
            if (allen_relationsEPS.finish_by(a, b, eps, min_gap, max_gap).Item2 == 3) return allen_relationsEPS.ALLEN_FINISHBY_PRINT;
            return allen_relationsEPS.ALLEN_OVERLAP_PRINT;
        }

    }
}
