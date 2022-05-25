using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;

namespace VertTIRP.tirp
{
    public class TIRP
    {
        public List<TI> ti;
        List<string> r;
        public int first;
        public int max_last;
        /*ADDED*/public int earliest_finish;

        public TIRP(List<TI> ti, int first, int max_last, /*ADDED*/int earliest_finish, List<string> r = null)
        {
            this.ti = ti;

            if (r == null)
            {
                r = new List<string>();
            }

            this.r = r;

            this.first = first;
            this.max_last = max_last;
            /*ADDED*/this.earliest_finish = earliest_finish;
        }

        public TIRP my_copy()
        {
            List<string> new_r = new List<string>();
            foreach (string i in this.r)
            {
                new_r.Add(i);
            }

            List<TI> new_ti = new List<TI>();
            foreach(TI t in this.ti)
            {
                new_ti.Add(new TI(t.sym, t.start, t.end));
            }

            return new TIRP(new_ti, this.first, this.max_last, /*ADDED*/this.earliest_finish, new_r);
        }

        public bool less_than(TIRP tirp)
        {
            if (this.first < tirp.first)
            {
                return true;
            }
            else
            {
                if (this.first == tirp.first)
                {
                    return this.max_last < tirp.max_last;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool less_equal(TIRP tirp)
        {
            if (this.first < tirp.first)
            {
                return true;
            }
            else
            {
                if (this.first == tirp.first)
                {
                    return (this.max_last < tirp.max_last) || (this.max_last == tirp.max_last);
                }
                else
                {
                    return false;
                }
            }
        }

        public string get_rel_as_str()
        {
            string ans = "";
            foreach(string x in this.r)
            {
                ans += (string)x;
            }
            return ans;
        }

        public int get_duration()
        {
            return this.max_last - this.first;
        }

        public Tuple<TIRP, int> extend_with(TI s_ti, int eps, uint min_gap, uint max_gap, uint max_duration, bool mine_last_equal, Allen allen)
        {
            Tuple<string, int> ret = allen.calc_rel(this.ti[this.ti.Count - 1], s_ti, eps, min_gap, max_gap);
            string c_rel = ret.Item1;
            int status_rel = ret.Item2;

            if (!mine_last_equal && c_rel.Equals("e"))
            {
                return new Tuple<TIRP, int>(null, 1);
            }

            if (status_rel < 3)
            {
                return new Tuple<TIRP, int>(null, status_rel);
            }

            List<string> new_rel = new List<string>();
            foreach (string nr in this.r)
            {
                new_rel.Add(nr);
            }
            for (int i = 0; i < this.ti.Count; i++)
            {
                new_rel.Add("");
            }
            new_rel[new_rel.Count - 1] = c_rel;

            List<TI> new_ti = new List<TI>();
            foreach (TI n in this.ti)
            {
                new_ti.Add(new TI(n.sym, n.start, n.end));
            }
            new_ti.Add(s_ti);

            int new_max_last = (int)s_ti.end;
            if (new_max_last < this.max_last)
            {
                new_max_last = this.max_last;
            }

            if (allen_relationsEPS.ttu(new_max_last - this.first) > max_duration)
            {
                return new Tuple<TIRP, int>(null, 1);
            }

            int size_rel = new_rel.Count;
            int size_sym = new_ti.Count;

            int r_idx = 1;
            int temp = size_sym - r_idx;
            int first_pos = (int)(((Math.Pow(temp, 2) - temp) / 2) - 1);
            while (first_pos >= 0)
            {
                int second_pos = size_rel - r_idx;
                int pos_to_assign = second_pos - 1;
                TI existent_node = new_ti[temp - 2];
                if (allen.trans)
                {
                    Tuple<string, Tuple<List<List<Tuple<string, List<string>>>>, List<string>>> possible_rels = allen.get_possible_rels(new_rel[first_pos], new_rel[second_pos]);
                    Tuple<string, int> c_rel_status_rel = allen.assign_rel(existent_node, s_ti, possible_rels, eps, min_gap, max_gap);
                    c_rel = c_rel_status_rel.Item1;
                    status_rel = c_rel_status_rel.Item2;
                }
                else
                {
                    Tuple<string, int> c_rel_status_rel = allen.calc_rel(existent_node, s_ti, eps, min_gap, max_gap);
                    c_rel = c_rel_status_rel.Item1;
                    status_rel = c_rel_status_rel.Item2;
                }    

                if (status_rel < 3)
                {
                    return new Tuple<TIRP, int>(null, status_rel);
                }

                new_rel[pos_to_assign] = c_rel;
                r_idx += 1;
                temp = size_sym - r_idx;
                first_pos = (int)(((Math.Pow(temp, 2) - temp) / 2) - 1);
            }
            return new Tuple<TIRP, int>(new TIRP(new_ti, (int)new_ti[0].start, new_max_last, Math.Min(this.earliest_finish, (int)s_ti.end), new_rel), 3);
        }

    }
}
