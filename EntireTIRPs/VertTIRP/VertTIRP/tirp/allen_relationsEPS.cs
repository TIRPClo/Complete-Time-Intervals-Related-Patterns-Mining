using System;
using System.Collections.Generic;
using System.Text;
using VertTIRP.ti;

namespace VertTIRP.tirp
{
    public class allen_relationsEPS
    {

        public const char ALLEN_BEFORE_PRINT = '<';
        public const char ALLEN_MEET_PRINT = 'm';
        public const char ALLEN_OVERLAP_PRINT = 'o';
        public const char ALLEN_FINISHBY_PRINT = 'f';
        public const char ALLEN_CONTAIN_PRINT = 'c';
        public const char ALLEN_EQUAL_PRINT = '=';
        public const char ALLEN_STARTS_PRINT = 'S';

        static uint MAXGAP = 3155695200;

        public static Dictionary<string, string> trans_table_0 = new Dictionary<string, string>();
        public static Dictionary<string, string> trans_table = new Dictionary<string, string>();
        public static void init_dicts()
        {
            // before
            trans_table_0["bb"] = "b";
            trans_table_0["bc"] = "b";
            trans_table_0["bo"] = "b";
            trans_table_0["bm"] = "b";
            trans_table_0["bs"] = "b";
            trans_table_0["bf"] = "b";
            trans_table_0["be"] = "b";
            // equals
            trans_table_0["eb"] = "b";
            trans_table_0["ec"] = "c";
            trans_table_0["eo"] = "o";
            trans_table_0["em"] = "m";
            trans_table_0["es"] = "s";
            trans_table_0["ef"] = "f";
            trans_table_0["ee"] = "e";
            // contains
            trans_table_0["cb"] = "bcfmo";
            trans_table_0["cc"] = "c";
            trans_table_0["co"] = "cfo";
            trans_table_0["cm"] = "cfo";
            trans_table_0["cs"] = "cfo";
            trans_table_0["cf"] = "c";
            trans_table_0["ce"] = "c";
            // overlaps
            trans_table_0["ob"] = "b";
            trans_table_0["oc"] = "bcfmo";
            trans_table_0["oo"] = "bmo";
            trans_table_0["om"] = "b";
            trans_table_0["os"] = "o";
            trans_table_0["of"] = "bmo";
            trans_table_0["oe"] = "o";
            // meets
            trans_table_0["mb"] = "b";
            trans_table_0["mc"] = "b";
            trans_table_0["mo"] = "b";
            trans_table_0["mm"] = "b";
            trans_table_0["ms"] = "m";
            trans_table_0["mf"] = "b";
            trans_table_0["me"] = "m";
            // starts
            trans_table_0["sb"] = "b";
            trans_table_0["sc"] = "bcfmo";
            trans_table_0["so"] = "bmo";
            trans_table_0["sm"] = "b";
            trans_table_0["ss"] = "s";
            trans_table_0["sf"] = "bmo";
            trans_table_0["se"] = "s";
            // finished-by
            trans_table_0["fb"] = "b";
            trans_table_0["fc"] = "c";
            trans_table_0["fo"] = "o";
            trans_table_0["fm"] = "m";
            trans_table_0["fs"] = "o";
            trans_table_0["ff"] = "f";
            trans_table_0["fe"] = "f";

            // ---------------------------------------------------------

            // before
            trans_table["bb"] = "b";
            trans_table["bc"] = "b";
            trans_table["bo"] = "b";
            trans_table["bm"] = "b";
            trans_table["bs"] = "b";
            trans_table["bf"] = "b";
            trans_table["be"] = "b";
            trans_table["bl"] = "b";
            // contains
            trans_table["cb"] = "bcfmo";
            trans_table["cc"] = "c";
            trans_table["co"] = "cfo";
            trans_table["cm"] = "cfo";
            trans_table["cs"] = "cfo";
            trans_table["cf"] = "cf";
            trans_table["ce"] = "cf";
            trans_table["cl"] = "c";
            // overlaps
            trans_table["ob"] = "b";
            trans_table["oc"] = "bcfmo";
            trans_table["oo"] = "bmo";
            trans_table["om"] = "bm";
            trans_table["os"] = "mo";
            trans_table["of"] = "bfmo";
            trans_table["oe"] = "mo";
            trans_table["ol"] = "cfo";
            // meets
            trans_table["mb"] = "b";
            trans_table["mc"] = "bm";
            trans_table["mo"] = "bm";
            trans_table["mm"] = "bm";
            trans_table["ms"] = "bm";
            trans_table["mf"] = "bm";
            trans_table["me"] = "bm";
            trans_table["ml"] = "bm";
            // starts 
            trans_table["sb"] = "b";
            trans_table["sc"] = "bcfmo";
            trans_table["so"] = "bmo";
            trans_table["sm"] = "bm";
            trans_table["ss"] = "ms";
            trans_table["sf"] = "bmo";
            trans_table["se"] = "emos";
            trans_table["sl"] = "cflmo";
            // finished-by
            trans_table["fb"] = "bm";
            trans_table["fc"] = "cf";
            trans_table["fo"] = "fmo";
            trans_table["fm"] = "bmo";
            trans_table["fs"] = "fmo";
            trans_table["ff"] = "cfmo";
            trans_table["fe"] = "cflmo";
            trans_table["fl"] = "cf";
            // equals
            trans_table["eb"] = "bm";
            trans_table["ec"] = "cf";
            trans_table["eo"] = "fmo";
            trans_table["em"] = "bemo";
            trans_table["es"] = "eos";
            trans_table["ef"] = "cfm";
            trans_table["ee"] = "cefo";
            trans_table["el"] = "cfl";
            // left-equal
            trans_table["lb"] = "bcfmo";
            trans_table["lc"] = "c";
            trans_table["lo"] = "cfo";
            trans_table["lm"] = "cmo";
            trans_table["ls"] = "celo";
            trans_table["lf"] = "cf";
            trans_table["le"] = "cefl";
            trans_table["ll"] = "c";
        }

        public static Tuple<List<List<Tuple<string, List<string>>>>, List<string>> get_pairing_strategy(string str_rels)
        {
            List<List<Tuple<string, List<string>>>> rels_arr = new List<List<Tuple<string, List<string>>>>();
            List<string> gr_arr = new List<string>();
            int i = 0;
            Dictionary<string, bool> added = new Dictionary<string, bool>();

            while (i < str_rels.Length)
            {
                string c = str_rels[i] + "";
                if (!added.ContainsKey(c) && c.Equals("b"))
                {
                    List<Tuple<string, List<string>>> l = new List<Tuple<string, List<string>>>();
                    l.Add(new Tuple<string, List<string>>(c, null));
                    rels_arr.Add(l);
                    gr_arr.Add(null);
                    added[c] = true;
                }
                else if (!added.ContainsKey(c) && (c.Equals("c") || c.Equals("f") || c.Equals("m") || c.Equals("o")))
                {
                    gr_arr.Add("cfmo");
                    int j = 0, k = 0;
                    if (c.Equals("m") || c.Equals("o"))
                    {
                        List<Tuple<string, List<string>>> l = new List<Tuple<string, List<string>>>();
                        List<string> c_list = new List<string>();
                        c_list.Add(c);
                        l.Add(new Tuple<string, List<string>>(null, c_list));
                        rels_arr.Add(l);
                        added[c] = true;
                        j = i + 1;
                        k = i + 1;

                        while (k < str_rels.Length)
                        {
                            c = str_rels[k] + "";
                            if (!added.ContainsKey(c))
                            {
                                if (c.Equals("m") || c.Equals("o"))
                                {
                                    List<Tuple<string, List<string>>> l1 = rels_arr[rels_arr.Count - 1];
                                    Tuple<string, List<string>> l2 = l1[l1.Count - 1];
                                    l2.Item2.Add(c);
                                    added[c] = true;
                                }
                            }
                            k = k + 1;
                        }

                        while (j < str_rels.Length)
                        {
                            c = str_rels[j] + "";
                            if (!added.ContainsKey(c))
                            {
                                if (c.Equals("c") || c.Equals("f"))
                                {
                                    rels_arr[rels_arr.Count - 1].Add(new Tuple<string, List<string>>(c, null));
                                    added[c] = true;
                                }
                            }
                            j = j + 1;
                        }
                    }
                    else
                    {
                        List<Tuple<string, List<string>>> l = new List<Tuple<string, List<string>>>();
                        l.Add(new Tuple<string, List<string>>(c, null));
                        rels_arr.Add(l);
                        added[c] = true;
                        j = i + 1;

                        while (j < str_rels.Length)
                        {
                            c = str_rels[j] + "";
                            if (!added.ContainsKey(c))
                            {
                                if (c.Equals("c") || c.Equals("f"))
                                {
                                    rels_arr[rels_arr.Count - 1].Add(new Tuple<string, List<string>>(c, null));
                                    added[c] = true;
                                }
                                else if (c.Equals("m") || c.Equals("o"))
                                {
                                    List<string> c_list = new List<string>();
                                    c_list.Add(c);
                                    rels_arr[rels_arr.Count - 1].Add(new Tuple<string, List<string>>(null, c_list));
                                    added[c] = true;
                                    k = j + 1;
                                    while (k < str_rels.Length) 
                                    {
                                        c = str_rels[k] + "";
                                        if (!added.ContainsKey(c))
                                        {
                                            if (c.Equals("m") || c.Equals("o"))
                                            {
                                                List<Tuple<string, List<string>>> l1 = rels_arr[rels_arr.Count - 1];
                                                Tuple<string, List<string>> l2 = l1[l1.Count - 1];
                                                l2.Item2.Add(c);
                                                added[c] = true;
                                            }
                                        }
                                        k = k + 1;
                                    }
                                }
                            }
                            j = j + 1;
                        }
                    }
                }
                else
                {
                    if (!added.ContainsKey(c))
                    {
                        int j = 0;
                        List<Tuple<string, List<string>>> l = new List<Tuple<string, List<string>>>();
                        l.Add(new Tuple<string, List<string>>(c, null)); 
                        rels_arr.Add(l);
                        added[c] = true;
                        gr_arr.Add("sel");
                        j = i + 1;
                        while (j < str_rels.Length)
                        {
                            c = str_rels[j] + "";
                            if (!added.ContainsKey(c))
                            {
                                if (c.Equals("s") || c.Equals("e") || c.Equals("l"))
                                {
                                    rels_arr[rels_arr.Count - 1].Add(new Tuple<string, List<string>>(c, null));
                                    added[c] = true;
                                }
                            }
                            j = j + 1;
                        }
                    }
                }
                i = i + 1;
            }

            return new Tuple<List<List<Tuple<string, List<string>>>>, List<string>>(rels_arr, gr_arr);
        }

        public static Tuple<string, int> before_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_s_a_e = ttu(b.start - a.end);
            if (b_s_a_e > eps)
            {
                if (min_gap != 0 && b_s_a_e < min_gap)
                {
                    return new Tuple<string, int>("1", 1);
                }
                else if (max_gap != MAXGAP && b_s_a_e >= max_gap)/*ADDED >=*/
                {
                    return new Tuple<string, int>("2", 2);
                }
                else
                {
                    return new Tuple<string, int>("b", 3);
                }
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> meets_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_s_a_e = ttu(b.start - a.end);
            if (Math.Abs((int)b_s_a_e) <= eps)
            {
                return new Tuple<string, int>("m", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> overlaps_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_s_a_e = ttu(b.start - a.end);
            if (b_s_a_e < (-eps))
            {
                return new Tuple<string, int>("o", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> contains_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_e_a_e = ttu(b.end - a.end);
            if (b_e_a_e < (-eps))
            {
                return new Tuple<string, int>("c", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> finish_by_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_e_a_e = ttu(b.end - a.end);
            if (b_e_a_e <= eps)
            {
                return new Tuple<string, int>("f", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> equal_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_e_a_e = ttu(b.end - a.end);
            if (b_e_a_e <= eps)
            {
                return new Tuple<string, int>("e", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> starts_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_e_a_e = ttu(b.end - a.end);
            if (b_e_a_e > eps)
            {
                return new Tuple<string, int>("s", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> left_contains_ind(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if (eps == 0)
            {
                return new Tuple<string, int>("-2", -2);
            }
            Nullable<int> b_e_a_e = ttu(b.end - a.end);
            if (b_e_a_e < (-eps))
            {
                return new Tuple<string, int>("l", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static bool sel_cond(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_s_a_s = ttu(b.start - a.start);
            return Math.Abs((int)b_s_a_s) <= eps;
        }

        public static bool cfmo_cond(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_s_a_s = ttu(b.start - a.start);
            return b_s_a_s > eps;
        }

        public static bool mo_cond(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            Nullable<int> b_e_a_e = ttu(b.end - a.end);
            return b_e_a_e > eps;
        }

        public static bool true_cond(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            return true;
        }

        public static Tuple<string, int> ind_func_dict(string rel, TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if (rel.Equals("b"))
            {
                return before_ind(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("m"))
            {
                return meets_ind(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("o"))
            {
                return overlaps_ind(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("c"))
            {
                return contains_ind(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("f"))
            {
                return finish_by_ind(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("e"))
            {
                return equal_ind(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("s"))
            {
                return starts_ind(a, b, eps, min_gap, max_gap);
            }
            else
            {
                return left_contains_ind(a, b, eps, min_gap, max_gap);
            }
        }

        public static bool cond_dict(string rels, TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if (rels.Equals("sel"))
            {
                return sel_cond(a, b, eps, min_gap, max_gap);
            }
            else if (rels.Equals("cfmo"))
            {
                return cfmo_cond(a, b, eps, min_gap, max_gap);
            }
            else if (rels.Equals("mo"))
            {
                return mo_cond(a, b, eps, min_gap, max_gap);
            }
            else
            {
                return false;
            }
        }

        public static Nullable<int> ttu(Nullable<int> diff)
        {
            return diff;
        }

        public static Tuple<string, int> before(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            return ind_func_dict("b", a, b, eps, min_gap, max_gap);
        }

        public static Tuple<string, int> meets(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((Math.Abs((int)ttu(b.start - a.end)) <= eps) && (ttu(b.start - a.start) > eps) && (ttu(b.end - a.end) > eps))
            {
                return new Tuple<string, int>("m", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> overlaps(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((ttu(b.start - a.start) > eps) && (ttu(b.end - a.end) > eps) && (ttu(a.end - b.start) > eps))
            {
                return new Tuple<string, int>("o", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> contains(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((ttu(b.start - a.start) > eps) && (ttu(a.end - b.end) > eps))
            {
                return new Tuple<string, int>("c", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> finish_by(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((ttu(b.start - a.start) > eps) && (Math.Abs((int)ttu(b.end - a.end)) <= eps))
            {
                return new Tuple<string, int>("f", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> equal(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((Math.Abs((int)ttu(b.start - a.start)) <= eps) && (Math.Abs((int)ttu(b.end - a.end)) <= eps))
            {
                return new Tuple<string, int>("e", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> starts(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((Math.Abs((int)ttu(b.start - a.start)) <= eps) && (ttu(b.end - a.end) > eps))
            {
                return new Tuple<string, int>("s", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> left_contains(TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if ((eps > 0 && Math.Abs((int)ttu(b.start -a.start)) <= eps) && (ttu(b.end - a.end) < (-eps)))
            {
                return new Tuple<string, int>("l", 3);
            }
            else
            {
                return new Tuple<string, int>("-2", -2);
            }
        }

        public static Tuple<string, int> rel_func_dict(string rel, TI a, TI b, int eps, uint min_gap, uint max_gap)
        {
            if (rel.Equals("b"))
            {
                return before(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("m"))
            {
                return meets(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("o"))
            {
                return overlaps(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("c"))
            {
                return contains(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("f"))
            {
                return finish_by(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("e"))
            {
                return equal(a, b, eps, min_gap, max_gap);
            }
            else if (rel.Equals("s"))
            {
                return starts(a, b, eps, min_gap, max_gap);
            }
            else
            {
                return left_contains(a, b, eps, min_gap, max_gap);
            }
        }

    }
}
