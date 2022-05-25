using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;

namespace VertTIRP.tirp
{
    public abstract class Allen
    {

        public bool dummy_calc;
        public bool trans;
        public int eps;

        public Allen(bool dummy_calc, bool trans = true, int eps = 0)
        {
            this.dummy_calc = dummy_calc;
            this.trans = trans;
            this.eps = eps;
        }

        public abstract Tuple<string, int> calc_rel(TI a, TI b, int eps, uint min_gap, uint max_gap, List<List<Tuple<string, List<string>>>> rels_arr = null, List<string> gr_arr = null);

        public abstract Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> get_possible_rels(string a, string b);

        public abstract Tuple<string, int> assign_rel(TI a, TI b, Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> possible_rels, int eps, uint min_gap, uint max_gap);

    }
}
