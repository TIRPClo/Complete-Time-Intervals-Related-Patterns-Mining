using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;
using VertTIRP.tirp;

namespace VertTIRP
{
    public class VertTirpSidList
    {

        const uint MAXGAP = 3155695200;
        const uint MAXDURATION = 3155695200;
        public List<string> seq_str;
        public int seq_length;
        public Dictionary<string, Dictionary<int, List<TIRP>>> definitive_ones_indices_dict;
        public Dictionary<string, TIRPstatistics> definitive_discovered_tirp_dict;
        Dictionary<string, TIRPstatistics> temp_discovered_tirp_dict;
        int n_sequences;
        int support;

        public VertTirpSidList()
        {
            this.seq_str = new List<string>();
            this.seq_length = 0;
            this.definitive_ones_indices_dict = new Dictionary<string, Dictionary<int, List<TIRP>>>();
            this.definitive_discovered_tirp_dict = new Dictionary<string, TIRPstatistics>();
            this.temp_discovered_tirp_dict = new Dictionary<string, TIRPstatistics>();
            this.n_sequences = 0;
            this.support = 0;
        }

        public void append_item(TI ti, string sid, int eid)
        {
            List<TI> tis = new List<TI>();
            tis.Add(ti);
            List<string> r = new List<string>();
            TIRP new_tirp = new TIRP(tis, (int)ti.start, (int)ti.end, (int)ti.end, r);

            if (this.definitive_discovered_tirp_dict == null || this.definitive_discovered_tirp_dict.Count == 0)
            {
                this.definitive_discovered_tirp_dict[" "] = new TIRPstatistics();
                List<string> syms = new List<string>();
                syms.Add(ti.sym);
                this.seq_str = syms;
                this.seq_length = 1;
            }

            if (!this.definitive_ones_indices_dict.ContainsKey(sid))
            {
                this.definitive_ones_indices_dict[sid] = new Dictionary<int, List<TIRP>>();
            }

            this.definitive_discovered_tirp_dict[" "].append_tirp(sid, eid, new_tirp);

            if (!this.definitive_ones_indices_dict[sid].ContainsKey(eid))
            {
                List<TIRP> tirps = new List<TIRP>();
                tirps.Add(new_tirp);
                this.definitive_ones_indices_dict[sid][eid] = tirps;
            }
            else
            {
                this.definitive_ones_indices_dict[sid][eid].Add(new_tirp);
            }
        }

        public void set_n_sequences(int n_sequences)
        {
            this.n_sequences = n_sequences;
        }

        public double get_mean_hor_support(Dictionary<string, int> events_per_sequence, TIRPstatistics tirp_stat = null)
        {
            return tirp_stat.get_mean_hor_support(events_per_sequence);
        }

        public double get_ver_support(TIRPstatistics tirp_stat = null)
        {
            return tirp_stat.get_ver_support(this.n_sequences);
        }

        public int get_support()
        {
            return this.definitive_discovered_tirp_dict[" "].sum_ver_supp;
        }

        public VertTirpSidList join(VertTirpSidList f, Allen ps, int eps, uint min_gap = 0, uint max_gap = MAXGAP, uint max_duration = MAXDURATION, double min_ver_sup = 0, double min_confidence = 0.9)
        {
            VertTirpSidList new_sidlist = new VertTirpSidList();
            new_sidlist.seq_str = new List<string>(this.seq_str);
            new_sidlist.seq_str.Add(f.seq_str[0]);

            new_sidlist.seq_length = this.seq_length + 1;

            new_sidlist.definitive_ones_indices_dict = new Dictionary<string, Dictionary<int, List<TIRP>>>();
            new_sidlist.definitive_discovered_tirp_dict = new Dictionary<string, TIRPstatistics>();
            new_sidlist.temp_discovered_tirp_dict = new Dictionary<string, TIRPstatistics>();
            new_sidlist.n_sequences = this.n_sequences;
            new_sidlist.support = 0;

            bool mine_last_equal = TI.compareSymbols(this.seq_str[this.seq_str.Count -1], f.seq_str[0]) < 0;

            foreach (KeyValuePair<string, Dictionary<int, List<TIRP>>> seq_id_dict_pos_tirps in this.definitive_ones_indices_dict)
            {
                string seq_id = seq_id_dict_pos_tirps.Key;
                Dictionary<int, List<TIRP>> dict_pos_tirps = seq_id_dict_pos_tirps.Value;
                if (f.definitive_ones_indices_dict.ContainsKey(seq_id))
                {
                    List<int> f_eids = new List<int>(f.definitive_ones_indices_dict[seq_id].Keys);
                    int last_f_first = f.definitive_ones_indices_dict[seq_id][f_eids[f_eids.Count -1]][0].first;
                    int first_f_first = f.definitive_ones_indices_dict[seq_id][f_eids[0]][0].first;
                    foreach (KeyValuePair<int, List<TIRP>> self_first_eid_self_tirps in dict_pos_tirps)
                    {
                        int self_first_eid = self_first_eid_self_tirps.Key;
                        List<TIRP> self_tirps = self_first_eid_self_tirps.Value;
                        if (self_first_eid < f_eids[f_eids.Count - 1])
                        {
                            TIRP first_one_me = self_tirps[0];
                            long me_first = first_one_me.first;

                            if (min_gap > 0)
                            {
                                me_first = first_one_me.first + min_gap;
                            }

                            if (last_f_first >= me_first)
                            {
                                long me_second = 0;
                                if (max_gap != MAXGAP)
                                {
                                    me_second = me_first + max_gap;
                                }

                                /*ADDED*/
                                //if ((max_gap == MAXGAP) || ((max_gap != MAXGAP) && first_f_first <= me_second))
                                //{
                                    Dictionary<int, List<TIRP>> f_dict_pos_tirps = f.definitive_ones_indices_dict[seq_id];
                                    foreach (KeyValuePair<int, List<TIRP>> f_pos_f_tirps in f_dict_pos_tirps)
                                    {
                                        int f_pos = f_pos_f_tirps.Key;
                                        List<TIRP> f_tirps = f_pos_f_tirps.Value;
                                        /*ADDED*/
                                        /*if ((max_gap != MAXGAP) && f_tirps[0].first > me_second)
                                        {
                                            break;
                                        }*/
                                        //else
                                        //{
                                            if (f_pos > self_first_eid)
                                            {
                                                int ext_status = new_sidlist.update_tirp_attrs(seq_id, f_pos, f, mine_last_equal, ps, this.definitive_ones_indices_dict[seq_id][self_first_eid], eps, min_gap, max_gap, max_duration, min_ver_sup, this.definitive_discovered_tirp_dict, min_confidence);
                                                if (ext_status == 2)
                                                {
                                                    break;
                                                }
                                            }
                                        //}
                                    }
                                //}
                            }
                        }
                    }
                }
            }
            new_sidlist.temp_discovered_tirp_dict = null;
            return new_sidlist;
        }

        public int update_tirp_attrs(string seq_id, int f_eid, VertTirpSidList f_sidlist, bool mine_last_equal, Allen ps, List<TIRP> tirps_to_extend, int eps, uint min_gap, uint max_gap, uint max_duration, double min_ver_sup, Dictionary<string, TIRPstatistics> father_discovered_tirp_dict, double min_confidence)
        {

            List<TI> f_ti = f_sidlist.definitive_ones_indices_dict[seq_id][f_eid][0].ti;

            bool all_max_gap_exceeded = true;
            bool at_least_one_tirp = false;

            foreach(TIRP tirp_to_extend in tirps_to_extend)
            {
                Tuple<TIRP, int> new_tirp_status = tirp_to_extend.extend_with(f_ti[0], eps, min_gap, max_gap, max_duration, mine_last_equal, ps);
                TIRP new_tirp = new_tirp_status.Item1; 
                int status = new_tirp_status.Item2;

                if (new_tirp != null)
                {
                    at_least_one_tirp = true;

                    string new_rel = new_tirp.get_rel_as_str();
                    if (!this.temp_discovered_tirp_dict.ContainsKey(new_rel))
                    {
                        this.temp_discovered_tirp_dict[new_rel] = new TIRPstatistics();
                    }

                    int vert_supp = this.temp_discovered_tirp_dict[new_rel].append_tirp(seq_id, f_eid, new_tirp);

                    bool conf_constraint = true;
                    if (min_confidence != -1)
                    {
                        int father_supp = 0;
                        if (tirp_to_extend.get_rel_as_str().Equals(""))
                        {
                            father_supp = father_discovered_tirp_dict[" "].sum_ver_supp;
                        }
                        else
                        {
                            father_supp = father_discovered_tirp_dict[tirp_to_extend.get_rel_as_str()].sum_ver_supp;
                        }
                        conf_constraint = ((vert_supp / father_supp) >= min_confidence);
                    }

                    if (vert_supp >= min_ver_sup && conf_constraint)
                    {
                        if (this.definitive_discovered_tirp_dict.ContainsKey(new_rel))
                        {
                            this.definitive_discovered_tirp_dict[new_rel] = this.temp_discovered_tirp_dict[new_rel];

                            if (!this.definitive_ones_indices_dict.ContainsKey(seq_id))
                            {
                                this.definitive_ones_indices_dict[seq_id] = new Dictionary<int, List<TIRP>>();
                            }

                            if (!this.definitive_ones_indices_dict[seq_id].ContainsKey(f_eid))
                            {
                                List<TIRP> l = new List<TIRP>();
                                l.Add(new_tirp);
                                this.definitive_ones_indices_dict[seq_id][f_eid] = l;
                            }
                            else
                            {
                                List<TIRP> l = new List<TIRP>();
                                l.Add(new_tirp);
                                this.first_sorted_extend(seq_id, f_eid, l);
                            }
                        }
                        else
                        {
                            this.definitive_discovered_tirp_dict[new_rel] = this.temp_discovered_tirp_dict[new_rel];

                            foreach(KeyValuePair<string, Dictionary<int, List<TIRP>>> sid_eid_tirps in this.definitive_discovered_tirp_dict[new_rel].sequence_events_tirps_dict)
                            {
                                string sid = sid_eid_tirps.Key;
                                Dictionary<int, List<TIRP>> eid_tirps = sid_eid_tirps.Value;
                                foreach (KeyValuePair<int, List<TIRP>> eid_tirps_entry in eid_tirps)
                                {
                                    int eid = eid_tirps_entry.Key;
                                    List<TIRP> tirps = eid_tirps_entry.Value;
                                    if (!this.definitive_ones_indices_dict.ContainsKey(sid))
                                    {
                                        this.support += 1;
                                        this.definitive_ones_indices_dict[sid] = new Dictionary<int, List<TIRP>>();
                                    }

                                    if (!this.definitive_ones_indices_dict[sid].ContainsKey(eid))
                                    {
                                        this.definitive_ones_indices_dict[sid][eid] = new List<TIRP>(tirps);
                                    }
                                    else
                                    {
                                        this.first_sorted_extend(sid, eid, tirps);
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (status != 2)
                    {
                        all_max_gap_exceeded = false;
                    }
                }
            }

            if (at_least_one_tirp)
            {
                return 3;
            }
            else if (all_max_gap_exceeded)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        public void first_sorted_extend(string sid, int eid, List<TIRP> new_tirps)
        {
            List<TIRP> current_tirps = this.definitive_ones_indices_dict[sid][eid];
            int i = 0;
            while (i < new_tirps.Count)
            {
                if (new_tirps[i].max_last > current_tirps[0].max_last)
                {
                    List<TIRP> tmp = new List<TIRP>();
                    tmp.Add(new_tirps[i]);
                    tmp.AddRange(current_tirps);
                    current_tirps = tmp;
                }
                else
                {
                    current_tirps.Add(new_tirps[i]);
                }
                i += 1;
            }

            this.definitive_ones_indices_dict[sid][eid] = current_tirps;
        }

    }
}
