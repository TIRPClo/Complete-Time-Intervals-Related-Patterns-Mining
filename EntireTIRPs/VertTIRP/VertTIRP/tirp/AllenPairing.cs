using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;

namespace VertTIRP.tirp
{
    public class AllenPairing : Allen
    {
        string calc_sort;
        List<List<Tuple<string, List<string>>>> rels_arr;
        List<string> gr_arr;
        Dictionary<string, Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>> sorted_trans_table;

        public AllenPairing(bool dummy_calc = false, bool trans = true, int eps = 0, string calc_sort = "bselfmoc"): base(dummy_calc, trans, eps)
        {
            this.calc_sort = calc_sort;
            Tuple<List<List<Tuple<string, List<string>>>>, List<string>> ret = allen_relationsEPS.get_pairing_strategy(this.calc_sort);
            this.rels_arr = ret.Item1;
            this.gr_arr = ret.Item2;

            this.sorted_trans_table = new Dictionary<string, Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>>();

            if (eps > 0)
            {
                foreach (KeyValuePair<string, string> key_entry in allen_relationsEPS.trans_table)
                {
                    string key = key_entry.Key;
                    string entry = key_entry.Value;
                    if (entry.Length == 1)
                    {
                        this.sorted_trans_table[key] = new Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>(entry, null);
                    }
                    else
                    {
                        this.sorted_trans_table[key] = new Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>(null, allen_relationsEPS.get_pairing_strategy(this.sort_rels(entry)));
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> key_entry in allen_relationsEPS.trans_table_0)
                {
                    string key = key_entry.Key;
                    string entry = key_entry.Value;
                    if (entry.Length == 1)
                    {
                        this.sorted_trans_table[key] = new Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>(entry, null);
                    }
                    else
                    {
                        this.sorted_trans_table[key] = new Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>(null, allen_relationsEPS.get_pairing_strategy(this.sort_rels(entry)));
                    }
                }
            }
        }

        public string sort_rels(string reducted_group)
        {
            string reducted_group_sorted = "";
            foreach (char ch in this.calc_sort)
            {
                if (reducted_group.IndexOf(ch) >= 0)
                {
                    reducted_group_sorted += ch;
                }
            }
            return reducted_group;
        }

        public override Tuple<string, int> calc_rel(TI a, TI b, int eps, uint min_gap, uint max_gap, List<List<Tuple<string, List<string>>>> rels_arr = null, List<string> gr_arr = null)
        {

            if (rels_arr == null)
            {
                rels_arr = this.rels_arr;
                gr_arr = this.gr_arr;
            }
            
            if (b.start < a.start || ((b.start == a.start) && (b.end < a.end)))
            {
                return new Tuple<string, int>("1", 1);
            }

            for (int i = 0; i < rels_arr.Count; i++)
            {
                List<Tuple<string, List<string>>> sentence = rels_arr[i];
                string g = gr_arr[i];
                if (g != null)
                {
                    if (allen_relationsEPS.cond_dict(g, a, b, eps, min_gap, max_gap))
                    {
                        foreach (Tuple<string, List<string>> words in sentence)
                        {
                            if (words.Item2 != null)
                            {
                                if (allen_relationsEPS.cond_dict("mo", a, b, eps, min_gap, max_gap))
                                {
                                    foreach (string w in words.Item2)
                                    {
                                        Tuple<string, int> ret = allen_relationsEPS.ind_func_dict(w, a, b, eps, min_gap, max_gap);
                                        string r = ret.Item1;
                                        int status = ret.Item2;
                                        if (status > -2)
                                        {
                                            return new Tuple<string, int>(r, status);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Tuple<string, int> ret = allen_relationsEPS.ind_func_dict(words.Item1, a, b, eps, min_gap, max_gap);
                                string r = ret.Item1;
                                int status = ret.Item2;
                                if (status > -2)
                                {
                                    return new Tuple<string, int>(r, status);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Tuple<string, int> ret = allen_relationsEPS.ind_func_dict("b", a, b, eps, min_gap, max_gap);
                    string r = ret.Item1;
                    int status = ret.Item2;
                    if (status > -2)
                    {
                        return new Tuple<string, int>(r, status);
                    }
                }
            }

            return new Tuple<string, int>("1", 1);
        }

        public override Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> get_possible_rels(string a, string b)
        {
            return this.sorted_trans_table[a + b];
        }

        public override Tuple<string, int> assign_rel(TI a, TI b, Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> possible_rels, int eps, uint min_gap, uint max_gap)
        {
            if (possible_rels.Item1 != null && possible_rels.Item1.Length == 1)
            {
                string first_r = possible_rels.Item1[0] + "";
                
                if (first_r.Equals("b"))
                {
                    Tuple<string, int> ret = allen_relationsEPS.ind_func_dict("b", a, b, eps, min_gap, max_gap);
                    string b_l = ret.Item1;
                    int b_s = ret.Item2;
                    if (b_s != -2)
                    {
                        return new Tuple<string, int>(b_l, b_s);
                    }
                    return this.calc_rel(a, b, eps, min_gap, max_gap);
                }
                else
                {
                    return new Tuple<string, int>(first_r/*ASSUMS IT IS ALWAYS A STRING*/, 3);
                }
            }
            else
            {
                /*ASSUMS IT IS ALWAYS NOT A STRING*/
                return this.calc_rel(a, b, eps, min_gap, max_gap, possible_rels.Item2.Item1, possible_rels.Item2.Item2);
            }
        }

    }
}
