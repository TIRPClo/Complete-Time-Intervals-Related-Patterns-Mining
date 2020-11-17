using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Responsible for handling coincidences related operations
    public class CoincidenceManager
    {
        //Splits the coincidence sequence to slices
        public static List<string> splitCoSeqToSlices(List<string> co, Func<string, bool> filt)
        {
            List<string> slices = new List<string>();
            foreach (string c in co)
                slices.AddRange(splitCoToSlices(c, filt));
            return slices;
        }

        //Splits one coincidence to slices
        //note: for @(x1-x2+) -> x1- and @x2+ added 
        public static List<string> splitCoToSlices(string co, Func<string, bool> filt)
        {
            if (co.Length == 0) return new List<string>();
            List<string> slices = new List<string>();
            string isMeet = "";
            if (co[0] == Constants.MEET_REP)
            {
                isMeet += co[0];
                //Take of @( )
                if (co.IndexOf('(') >= 0)
                    co = co.Substring(2, co.Length - 3);
                else
                    co = co.Substring(1);
            }
            string acc = "";
            for (int i = 0; i < co.Length; i++)
            {
                if (co[i] == Constants.ST_REP)
                {
                    if(filt(isMeet + acc + co[i]))
                        slices.Add(isMeet + acc + co[i]);
                    acc = "";
                    continue;
                }
                if (co[i] == Constants.FIN_REP)
                {
                    if(filt(acc + co[i]))
                        slices.Add(acc + co[i]);
                    acc = "";
                    continue;
                }
                acc += co[i];
            }
            return slices;
        }

        //Splits one coincidence to slices
        //note: for @(x1-x2+) -> x1- and @x2+ added 
        public static List<string> splitCoToSlices(string co)
        {
            if (co.Length == 0) return new List<string>();
            List<string> slices = new List<string>();
            string isMeet = "";
            if (co[0] == Constants.MEET_REP)
            {
                isMeet += co[0];
                //Take of @( )
                if (co.IndexOf('(') >= 0)
                    co = co.Substring(2, co.Length - 3);
                else
                    co = co.Substring(1);
            }
            string acc = "";
            for (int i = 0; i < co.Length; i++)
            {
                if (co[i] == Constants.ST_REP)
                {
                    slices.Add(isMeet + acc + co[i]);
                    acc = "";
                    continue;
                }
                if (co[i] == Constants.FIN_REP)
                {
                    slices.Add(acc + co[i]);
                    acc = "";
                    continue;
                }
                acc += co[i];
            }
            return slices;
        }

        //Returns the symbol part of the slice
        public static string getSymPartOfSlice(string slc)
        {
            return slc.Substring(0, slc.IndexOf(Constants.DUP_SYM)) + slc[slc.Length - 1];
        }

        //Return true iff the co is a partial one (partial = has been already projected by some slice)
        public static bool isPartialCo(string co)
        {
            return co.IndexOf(Constants.CO_REP) >= 0;
        }

        //Parses a coincidence sequence to an ordered array of coincidences, removing insignificant slices from the coincidences
        //note: coincidence that starts with @(x) would be @(x) and it needs to be referenced correctly
        public static List<string> rem_ins_slices(List<string> co, Dictionary<string, int> ins_slices, bool first_meet)
        {
            List<string> coes = new List<string>();
            string acc = "";
            bool isLastCoRemoved = false;
            int index = 0;
            string add = "";
            for (int i = 0; i < co.Count; i++)
            {
                List<string> slices = splitCoToSlices(co[i], (slc) => 
                    {
                        string s1 = slc.Substring(0, slc.IndexOf(Constants.DUP_SYM)) + slc[slc.Length - 1];
                        if (ins_slices.ContainsKey(s1) && ins_slices[s1] < Constants.MINSUP)
                        {
                            if (slc[0] != Constants.MEET_REP) return false;
                            string s2 = s1.Substring(1);
                            return ins_slices.ContainsKey(s2) && ins_slices[s2] >= Constants.MINSUP;
                        }
                        return true;
                    });
                acc = "";
                add = "";
                bool is_st = false;
                foreach (string slc in slices)
                {
                    if (slc[0] == Constants.MEET_REP)
                    {
                        add = "" + Constants.MEET_REP;
                        acc += slc.Substring(1);
                    }
                    else
                        acc += slc;
                    if (slc.IndexOf(Constants.ST_REP) >= 0)
                        is_st = true;
                }
                if (!is_st)
                    add = "";
                if (isLastCoRemoved && !(index == 1 && first_meet))
                    add = "";
                int c = slices.Count; 
                if (c == 1)
                {
                    coes.Add(add + acc);
                    isLastCoRemoved = false;
                }
                else
                {
                    if (c > 1)
                    {
                        if (add.Equals(""))
                            coes.Add(acc);
                        else
                            coes.Add(add + "(" + acc + ")");
                        isLastCoRemoved = false;
                    }
                    else
                        isLastCoRemoved = true;
                }
                acc = "";
                index++;
            }
            return coes;
        }

        //Parses a coincidence sequence to an ordered list with its slices
        //note: for @(x1-x2+) -> x1- and @x2+ added 
        public static List<string> splitCoSeqToSlices(string co)
        {
            List<string> slices = new List<string>();
            string acc = "";
            string add = "";
            bool isLong = false;
            for (int i = 0; i < co.Length; i++)
            {
                if (co[i] == Constants.ST_REP)
                {
                    slices.Add(add + acc + co[i]);
                    acc = "";
                    if (!isLong)
                        add = "";
                    continue;
                }
                if (co[i] == Constants.FIN_REP)
                {
                    slices.Add(acc + co[i]);
                    acc = "";
                }
                else
                {
                    if (co[i] == '(')
                    {
                        isLong = true;
                        continue;
                    }
                    if (co[i] == ')')
                    {
                        add = "";
                        isLong = false;
                        continue;
                    }
                    if (co[i] == Constants.MEET_REP)
                    {
                        add += Constants.MEET_REP;
                    }
                    else
                    {
                        acc += co[i];
                    }
                }
            }
            return slices;
        }

        //Check if all slices appear in pairs in the given coincidence sequence
        public static bool allInPairs(string alpha)
        {
            List<string> slcs = splitCoSeqToSlices(alpha);
            if (slcs.Count % 2 != 0) return false;
            Dictionary<string, int> slc_occs = new Dictionary<string, int>();
            foreach (string slc in slcs)
            {
                if (slc.IndexOf(Constants.ST_REP) >= 0)
                {
                    if (slc[0] == Constants.MEET_REP || slc[0] == Constants.CO_REP)
                    {
                        if (slc_occs.ContainsKey(slc.Substring(1, slc.Length - 2)))
                            slc_occs[slc.Substring(1, slc.Length - 2)]++;
                        else
                            slc_occs.Add(slc.Substring(1, slc.Length - 2), 1);
                        continue;
                    }
                    if (slc_occs.ContainsKey(slc.Substring(0, slc.Length - 1)))
                        slc_occs[slc.Substring(0, slc.Length - 1)]++;
                    else
                        slc_occs.Add(slc.Substring(0, slc.Length - 1), 1);
                }
                else
                {
                    if (slc[0] == Constants.CO_REP)
                    {
                        if (slc_occs.ContainsKey(slc.Substring(1, slc.Length - 2)))
                            slc_occs[slc.Substring(1, slc.Length - 2)]--;
                        else
                            return false;
                        continue;
                    }
                    if (slc_occs.ContainsKey(slc.Substring(0, slc.Length - 1)))
                        slc_occs[slc.Substring(0, slc.Length - 1)]--;
                    else
                        return false;
                }
            }
            foreach (KeyValuePair<string, int> slc in slc_occs)
            {
                if (slc.Value != 0)
                    return false;
            }
            return true;
        }

        //Check if no slices appear in pairs in the given coincidence sequence
        public static bool noneInPairs(List<string> BS, List<string> SF)
        {
            List<string> slcs = BS.Take(BS.Count).ToList();
            foreach (string slc in SF)
                if (!slcs.Contains(slc))
                    slcs.Add(slc);
            //Union created
            List<string> st_slcs = slcs.Where(slc => slc.IndexOf(Constants.ST_REP) >= 0).ToList();
            foreach (string slc in st_slcs)
            {
                string tmp_slc = slc;
                if (tmp_slc[0] == Constants.MEET_REP)
                {
                    tmp_slc = tmp_slc.Substring(1);
                }
                if (tmp_slc[0] == Constants.CO_REP)
                {
                    tmp_slc = tmp_slc.Substring(1);
                }
                foreach (string fin_slc in slcs)
                {
                    if (fin_slc.IndexOf(slc.Replace(Constants.ST_REP, Constants.FIN_REP)) >= 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //Creates the correct coincidence representation for the pattern discovered ------> can do each time we grow a pattern also !!!!!!!!!!! 
        public static string outToCoSeq(string co)
        {
            string coseq = "";
            string[] coarray = splitCoSeqToSlices(co).ToArray();
            if (coarray.Length == 0) return coseq;
            string curr = coarray[0];
            int curr_counter = 1;
            for (int i = 1; i < coarray.Length; i++)
            {
                if (coarray[i][0] != Constants.CO_REP)
                {
                    if (curr_counter > 1)
                    {
                        if (curr.IndexOf(Constants.MEET_REP) >= 0)
                        {
                            string temp = "";
                            for (int j = 0; j < curr.Length; j++)
                                if (curr[j] != Constants.MEET_REP)
                                    temp += curr[j];
                            coseq += ("@(" + temp + ")");
                        }
                        else
                            coseq += ("(" + curr + ")");
                    }
                    else
                        coseq += curr;
                    curr = coarray[i];
                    curr_counter = 1;
                    continue;
                }
                curr += coarray[i].Substring(1);
                curr_counter++;
            }
            if (curr_counter > 1)
            {
                if (curr.IndexOf(Constants.MEET_REP) >= 0)
                {
                    string temp = "";
                    for (int j = 0; j < curr.Length; j++)
                        if (curr[j] != Constants.MEET_REP)
                            temp += curr[j];
                    coseq += ("@(" + temp + ")");
                }
                else
                    coseq += ("(" + curr + ")");
            }
            else
                coseq += curr;
            return coseq;
        }

        //Returns the symbols of the coincidence sequence
        public static List<string> getSymbols(string cs)
        {
            List<string> syms = new List<string>();
            string acc = "";
            for (int i = 0; i < cs.Length; i++)
            {
                if (cs[i] == Constants.ST_REP)
                {
                    if (acc[0] == Constants.MEET_REP)
                        acc = acc.Substring(1);
                    syms.Add(acc);
                    acc = "";
                    continue;
                }
                if (cs[i] == Constants.FIN_REP)
                    acc = "";
                else if (cs[i] != '(' && cs[i] != ')')
                    acc += cs[i];
            }
            return syms;
        }

        //Returns the symbols of the pattern internal rep.
        public static List<string> getPatternSymbols(string cs)
        {
            List<string> syms = new List<string>();
            string acc = "";
            for (int i = 0; i < cs.Length; i++)
            {
                if (cs[i] == Constants.ST_REP)
                {
                    if (acc[0] == Constants.MEET_REP)
                        acc = acc.Substring(1);
                    syms.Add(acc);
                    acc = "";
                    continue;
                }
                if (cs[i] == Constants.FIN_REP)
                    acc = "";
                else if (cs[i] != Constants.CO_REP)
                    acc += cs[i];
            }
            return syms;
        }

        //Returns the symbols part of the slices without dups
        public static List<string> getSymPartOfSlices(List<string> slcs)
        {
            List<string> slices = new List<string>();
            foreach (string slc in slcs)
            {
                string sympart = getSymPartOfSlice(slc);
                if (!slices.Contains(sympart)) slices.Add(sympart);
            }
            return slices;
        }

        //Returns true iff the last added finishing slice has a concrete starting one before
        public static bool checkNoEndBeforeStart(string slice, string before_co)
        {
            //slice = slice.Substring(0, slice.Length - 1) + Constants.ST_REP;
            if (slice[0] == Constants.CO_REP)
            {
                slice = slice.Substring(1);
            }
            return (before_co.IndexOf(slice.Replace(Constants.FIN_REP, Constants.ST_REP)) >= 0);
        }

        //Returns the part of coincidence before the given slice
        public static string getPartBefore(string co, string slc)
        {
            List<string> slices = splitCoSeqToSlices(co);
            slices = getSymPartOfSlices(slices);
            if (slices.Count <= 1) return "";
            string res = "";
            bool found = false;
            string st = "", fn = "";
            if (co[0] == Constants.MEET_REP) //@(...)
            {
                List<string> slcs = splitCoSeqToSlices(co.Substring(1));
                int len = slcs.Count;
                for (int i = 0; i < len; i++)
                {
                    if (slcs[i].Equals(slc) || (slcs[i][0] == Constants.MEET_REP && slcs[i].Substring(1).Equals(slc)))
                        found = true;
                    else
                    {
                        if (!found)
                            res += slcs[i];
                    }
                    if (found)
                    {
                        slcs.Remove(slcs[i]);
                        len--;
                        i--;
                        continue;
                    }
                }
                string add = Constants.MEET_REP + "";
                if (res.IndexOf(Constants.ST_REP) < 0)
                    add = "";
                if (slcs.Count > 1 && add.Length > 0)
                {
                    st = "(";
                    fn = ")";
                }
                return add + st + res + fn;
            }
            int length = slices.Count;
            for (int i = 0; i < length; i++)
            {
                if (slices[i].Equals(slc) || (slices[i][0] == Constants.MEET_REP && slices[i].Substring(1).Equals(slc)))
                    found = true;
                else
                {
                    if (!found)
                        res += slices[i];
                }
                if (found)
                {
                    slices.Remove(slices[i]);
                    length--;
                    i--;
                    continue;
                }
            }
            return st + res + fn;
        }
    }
}
