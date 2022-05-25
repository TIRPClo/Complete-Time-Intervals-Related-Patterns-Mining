using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;

namespace VertTIRP.tirp
{
    public class AllenDummy : Allen
    {

        public AllenDummy(bool dummy_calc = false, bool trans = true, int eps = 0) : base(dummy_calc, trans, eps)
        {

        }

        public override Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> get_possible_rels(string a, string b)
        {
            if (this.eps > 0)
            {
                return new Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>(allen_relationsEPS.trans_table[a + b], null);
            }
            else
            {
                return new Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>>(allen_relationsEPS.trans_table_0[a + b], null);
            }
        }

        public override Tuple<string, int> calc_rel(TI a, TI b, int eps, uint min_gap, uint max_gap, List<List<Tuple<string, List<string>>>> rels_arr = null, List<string> gr_arr = null)
        {
            if (b.start < a.start || ((b.start == a.start) && (b.end < a.end)))
            {
                return new Tuple<string, int>("1", 1);
            }

            string final_rel = "1";
            int final_status = 1;
            string[] rels = { "l", "f", "s", "o", "b", "c", "m", "e" };
            foreach (string r in rels)
            {
                Tuple<string, int> rel_status = allen_relationsEPS.rel_func_dict(r, a, b, eps, min_gap, max_gap);
                string rel = rel_status.Item1;
                int status = rel_status.Item2;
                if (status != -2)
                {
                    final_rel = rel;
                    final_status = status;
                }
            }

            return new Tuple<string, int>(final_rel, final_status);
        }

        public override Tuple<string, int> assign_rel(TI a, TI b, Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> possible_rels, int eps, uint min_gap, uint max_gap)
        {
            if (possible_rels.Item1.Length == 1)
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
                    return new Tuple<string, int>(first_r, 3);
                }
            }
            else
            {
                foreach (char r in possible_rels.Item1)
                {
                    Tuple<string, int> rel_status = allen_relationsEPS.rel_func_dict(r + "", a, b, eps, min_gap, max_gap);
                    string rel = rel_status.Item1;
                    int status = rel_status.Item2;
                    if (status != -2)
                    {
                        return new Tuple<string, int>(rel, status);
                    }
                }
                return this.calc_rel(a, b, eps, min_gap, max_gap);
            }
        }

    }
}
