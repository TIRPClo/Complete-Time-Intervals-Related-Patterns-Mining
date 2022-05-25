using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VertTIRP.ti;
using VertTIRP.tirp;

namespace VertTIRP
{
    public class VertTIRP
    {

        const uint MAXGAP = 3155695200;
        const uint MAXDURATION = 3155695200;
        const uint MIN_DURATION = 0;
        string out_file;
        Dictionary<string, int> events_per_sequence;
        double min_sup_rel;
        double min_confidence;
        uint min_gap;
        uint max_gap;
        uint min_duration;
        uint max_duration;
        int max_length;
        int min_length;
        int eps;
        int tirp_count;
        int min_sup;
        List<string> f1;
        Dictionary<string, VertTirpSidList> vertical_db;
        VertTirpNode tree;
        Allen allen;

        public VertTIRP(string out_file = null, double min_sup_rel = 0.5, double min_confidence = -1, uint min_gap = 0, uint max_gap = MAXGAP,
            uint min_duration = MIN_DURATION, uint max_duration= MAXDURATION, int max_length = -1, int min_length = 1, int eps = 500, 
            bool dummy_calc = false, string ps = "mocfbesl", bool trans = true)
        {
            this.out_file = out_file;
            this.events_per_sequence = new Dictionary<string, int>();
            this.min_sup_rel = min_sup_rel;
            this.min_confidence = min_confidence;
            this.min_gap = min_gap;
            this.max_gap = max_gap;
            this.min_duration = min_duration;
            this.max_duration = max_duration;
            this.max_length = max_length;
            this.min_length = min_length;
            this.eps = eps;

            this.tirp_count = 0;
            this.min_sup = 0;
            this.f1 = new List<string>();
            this.vertical_db = new Dictionary<string, VertTirpSidList>();
            this.tree = new VertTirpNode();

            if (min_gap < 0 || max_gap < 0)
            {
                throw new ArgumentException("Invalid");
            }
            if (min_gap != 0 || max_gap != 0)
            {
                if (min_gap >= max_gap)
                {
                    throw new ArgumentException("Invalid");
                }
            }

            if (!dummy_calc)
            {
                this.allen = new AllenPairing(dummy_calc, trans, eps, ps);
            }
            else
            {
                this.allen = new AllenDummy(dummy_calc, trans, eps);
            }
        }

        public static int runVertTIRP(string filepath, string result_file_name, int num_entities, double ver_sup, uint ming, uint maxg, uint mind, uint maxd, int eps, bool dummy, string ps, bool trans, bool avoid_same_var_states)
        {
            allen_relationsEPS.init_dicts();

            Tuple<List<List<LinkedList_>>, List<string>, int> ret = ti2lstis.ti_read_KLF(filepath);
            List<List<LinkedList_>> list_of_ti_users = ret.Item1;
            List<string> list_of_users = ret.Item2;
            int ti_count = ret.Item3;

            VertTIRP co = new VertTIRP(result_file_name, ver_sup, -1, ming, maxg, mind, maxd, -1, 1, eps, dummy, ps, trans);

            int tirp_count = co.mine_tirps(list_of_ti_users, list_of_users, avoid_same_var_states);

            co.write_tirps(eps, ming, maxg, true);

            return tirp_count;
        }

        public void write_tirps(int eps, uint min_gap, uint max_gap, bool dfs = true)
        {
            if (this.out_file != null)
            {
                if (dfs)
                {
                    this.tree.write_tree_dfs(this.min_length, this.out_file, this.events_per_sequence, eps, min_gap, max_gap);
                }
                else
                {
                    this.tree.write_tree_bfs(this.tree, this.min_length, this.out_file, this.events_per_sequence);
                }
            }
            else
            {
                if (dfs)
                {
                    this.tree.write_tree_dfs(this.min_length, null, this.events_per_sequence, eps, min_gap, max_gap);
                }
                else
                {
                    this.tree.write_tree_bfs(this.tree, this.min_length, null, this.events_per_sequence);
                }
            }
        }

        public int mine_tirps(List<List<LinkedList_>> list_of_ti_seqs, List<string> list_of_seqs, bool avoid_same_var_states = true)
        {
            this.to_vertical(list_of_ti_seqs, list_of_seqs);

            List<int> procs = new List<int>();

            for (int i = 0; i < this.f1.Count; i++)
            {
                VertTirpNode node = new VertTirpNode(
                    getListRep(this.vertical_db[this.f1[i]].seq_str).ToString(),
                    1, this.tree, this.vertical_db[this.f1[i]]
                );
                this.dfs_pruning(
                    this.vertical_db[this.f1[i]], this.f1, node, 
                    this.tree, avoid_same_var_states
                );
            }

            return this.tirp_count;
        }

        public void dfs_pruning(VertTirpSidList pat_sidlist, List<string> f_l, VertTirpNode node, VertTirpNode father, bool avoid_same_var_states = true)
        {
            Console.WriteLine(node.patt);

            father.add_child(node);

            if (pat_sidlist.seq_length >= this.min_length)
            {
                this.tirp_count += pat_sidlist.definitive_discovered_tirp_dict.Count;
            }

            Dictionary<string, VertTirpSidList> s_temp = new Dictionary<string, VertTirpSidList>();

            if ((this.max_length == -1)  || ((this.max_length != -1) && ((pat_sidlist.seq_length + 1) <= this.max_length)))
            {
                foreach (string s in f_l)
                {
                    if (!this.same_variable(s, pat_sidlist.seq_str[pat_sidlist.seq_str.Count - 1], avoid_same_var_states))
                    {
                        VertTirpSidList s_bm = pat_sidlist.join(this.vertical_db[s], this.allen, this.eps, this.min_gap, this.max_gap, this.max_duration, this.min_sup, this.min_confidence);
                        if (s_bm.definitive_ones_indices_dict != null && s_bm.definitive_ones_indices_dict.Count > 0)
                        {
                            s_temp[s] = s_bm;
                        }
                    }
                }

                List<string> s_syms = new List<string>(s_temp.Keys);
                foreach (KeyValuePair<string, VertTirpSidList> j_j_pat in s_temp)
                {
                    string j = j_j_pat.Key;
                    VertTirpSidList j_pat = j_j_pat.Value;
                    VertTirpNode s_node = new VertTirpNode(getListRep(j_pat.seq_str).ToString(), j_pat.seq_length, node, j_pat);
                    this.dfs_pruning(j_pat, s_syms, s_node, node, avoid_same_var_states);
                }
            }
        }

        public bool same_variable(string sym1, string sym2, bool avoid_same_var_states = true)
        {
            if (!avoid_same_var_states)
            {
                return false;
            }

            string[] sym1_c = sym1.Split("_");
            string[] sym2_c = sym2.Split("_");
            return sym1_c[0] == sym2_c[0];
        }

        public void to_vertical(List<List<LinkedList_>> list_of_ti_seqs, List<string> list_of_seqs)
        {
            int eid = 0;

            for (int i = 0; i < list_of_ti_seqs.Count; i++)
            {
                LinkedList_ item_sets = list_of_ti_seqs[i][0];
                string name = list_of_seqs[i];
                this.events_per_sequence[name] = item_sets.size;
                foreach (TI_node its in item_sets.iter())
                {
                    if ((allen_relationsEPS.ttu(its.ti.end - its.ti.start) >= this.min_duration) && 
                        (allen_relationsEPS.ttu(its.ti.end - its.ti.start) <= this.max_duration))
                    {
                        if (!this.vertical_db.ContainsKey(its.ti.sym))
                        {
                            this.vertical_db[its.ti.sym] = new VertTirpSidList();
                            bool first_item = true;
                        }
                        this.vertical_db[its.ti.sym].append_item(its.ti, name, eid);

                        eid += 1;
                    }
                }
                
                eid = 0;
            }

            int n_sequences = list_of_seqs.Count;
            this.min_sup = (int)Math.Ceiling(this.min_sup_rel * n_sequences);
            if (this.min_sup == 0)
            {
                this.min_sup = 1;
            }

            foreach (string it_name in new List<string>(this.vertical_db.Keys))
            {
                if (this.vertical_db[it_name].get_support() >= this.min_sup)
                {
                    this.vertical_db[it_name].set_n_sequences(n_sequences);
                    this.f1.Add(it_name);
                }
                else
                {
                    this.vertical_db[it_name] = null;
                }
            }

            this.f1.Sort();
        }

        private static string getListRep(List<string> l)
        {
            string ans = "[";
            foreach (string value in l)
            {
                ans += value + ", ";
            }
            return ans.Substring(0, ans.Length - 2) + "]";
        }

    }
}
