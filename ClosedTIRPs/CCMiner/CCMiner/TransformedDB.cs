using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Represents a DB that has been transformed to coincidence rep. by the incision strategy
    public class TransformedDB
    {
        public Dictionary<string, int> subids;
        //The mapping between IDs to coincidence sequences
        public Dictionary<string, List<string>> trans_db;
        //The mapping between IDs to concrete patterns
        public Dictionary<string, Tuple<string, int>> patterns;
        //The last slice the given DB was projected by
        string projectedBy;
        //The support of the accumulated coincidence sequence in the projected DB
        public int sup;

        public TransformedDB(Dictionary<string, List<string>> tdb, Dictionary<string, Tuple<string, int>> pat,
            Dictionary<string, int> sub, string pb, int support)
        {
            trans_db = tdb;
            patterns = pat;
            projectedBy = pb;
            sup = support;
            subids = sub;
        }

        //Returns the frequent slices with their current supports
        //Returns also the infriquent slices as a list if the flag is set to true
        public Dictionary<string, int> slicesFreq()
        {
            //vertical support in current db entities (extended)
            Dictionary<string, int> slices_support = new Dictionary<string, int>();
            //Entries IDs and the slices already seen for it
            Dictionary<string, List<string>> slices_line_ignore = new Dictionary<string, List<string>>();
            //For each entity postfix in the current db:
            foreach (KeyValuePair<string, List<string>> entry in trans_db) 
            {
                //Add the entity id to ignores if its the first time we see it
                string ent_id = getID(entry.Key);
                if (!slices_line_ignore.ContainsKey(entry.Key))
                    slices_line_ignore.Add(entry.Key, new List<string>());
                int time = patterns[entry.Key].Item2;
                //Take the slices of the postfix
                List<string> concrete_slices = CoincidenceManager.splitCoSeqToSlices(entry.Value, 
                    (slc) => maxGapHolds(ent_id, time, slc));
                //For each concrete slice:
                for (int i = 0; i < concrete_slices.Count; i++)
                {
                    //if slice is @x: Do the following both for @x and x
                    //otherwise: Do only for x
                    string slc = CoincidenceManager.getSymPartOfSlice(concrete_slices[i]);
                    string conc_slc = concrete_slices[i];
                    for (int j = 0; j < 2; j++)
                    {
                        if (slices_support.ContainsKey(slc))
                        {
                            //Not the first time we meet the slice
                            if (!slices_line_ignore[entry.Key].Contains(slc))
                            {
                                slices_support[slc]++;
                                slices_line_ignore[entry.Key].Add(slc);
                            }
                        }
                        else
                        {
                            //First time we see the slice
                            slices_support.Add(slc, 1);
                            slices_line_ignore[entry.Key].Add(slc);
                        }
                        if (slc[0] == Constants.MEET_REP)
                        {
                            slc = slc.Substring(1);
                            conc_slc = conc_slc.Substring(1);
                        }
                        else
                            break;
                    }
                }
            }
            return slices_support;
        }

        //Returns id part of the entity current accumulated id
        public static string getID(string key)
        {
            string id = "";
            if (key.IndexOf('.') >= 0)
                id = key.Substring(0, key.IndexOf('.'));
            else
                id = key;
            return id;
        }

        //Returns true if the max gap constraint holds
        public static bool maxGapHolds(string id, int time, string slc)
        {
            if (time < 0) return true;
            if (slc[0] == Constants.MEET_REP)
                slc = slc.Substring(1);
            EventSequence es = CCMiner.mainDB.seqs[id];
            if (slc[0] == Constants.CO_REP)
                slc = slc.Substring(1);
            string tosearch = slc.Substring(0, slc.Length - 1);
            EventInterval ei2 = es.intsdic[tosearch];
            return Constants.MAX_GAP > ei2.st_time - time;  
        }

        public static EventInterval getInt(string id, string slc)
        {
            EventSequence es = CCMiner.mainDB.seqs[id];
            string tosearch = slc.Substring(0, slc.Length - 1);
            EventInterval ei2 = es.intsdic[tosearch];
            return ei2;
        }
        
        //Returns the pattern's last slice in the intermediate rep.
        public static string lastPatSlice(string pat)
        {
            string acc = "";
            for(int i = pat.Length - 1; i >= 0; i--)
            {
                if (i != pat.Length - 1 &&
                    (pat[i] == Constants.ST_REP || pat[i] == Constants.FIN_REP || pat[i] == Constants.CO_REP))
                    break;
                acc = pat[i] + acc;
            }
            return acc;
        }

        // Projects the DB with insignificant slices removal iff check_ins = true (alpha is always a single slice)
        public TransformedDB projectDB(string alpha, Dictionary<string, int> ins_slices, bool check_ins) 
        {
            KeyValuePair<int, KeyValuePair<List<string>, string>> proj;
            List<string> postfix;
            int instance_counter = 0;
            //the new subids
            Dictionary<string, int> projSubids = new Dictionary<string, int>();
            //new db sequences
            Dictionary<string, List<string>> projDB = new Dictionary<string, List<string>>();
            //new extended patterns
            Dictionary<string, Tuple<string, int>> projPatterns = new Dictionary<string, Tuple<string, int>>();
            //Find the discovered patterns add figure out the real support
            List<string> patIDs = new List<string>();
            //For each co sequence in db
            int i = 0;
            foreach (KeyValuePair<string, List<string>> entry in trans_db)
            {
                i++;
                string ent_id = getID(entry.Key);
                if(!projSubids.ContainsKey(ent_id))
                    projSubids.Add(ent_id, subids[ent_id]);
                List<string> cleared = null;
                if (check_ins)
                    cleared = CoincidenceManager.rem_ins_slices(entry.Value, ins_slices,
                        checkNextCoMeetOption(entry.Value, patterns[entry.Key].Item1));
                int last_time = patterns[entry.Key].Item2;
                if (check_ins)
                    proj = canProjectBy(alpha, cleared, patterns[entry.Key].Item1, ent_id, 0, "", last_time);
                else
                    proj = canProjectBy(alpha, entry.Value, patterns[entry.Key].Item1, ent_id, 0, "", last_time);
                postfix = proj.Value.Key; 
                if (postfix != null)
                {
                    projDB.Add(entry.Key, postfix);
                    int updated = getUpdatedEndTime(ent_id, last_time, proj.Value.Value);
                    projPatterns.Add(entry.Key, new Tuple<string, int>(patterns[entry.Key].Item1 + proj.Value.Value, updated));
                    if (!patIDs.Contains(ent_id))
                        patIDs.Add(ent_id);
                    bool stop = false;
                    int index = 0;
                    index = projSubids[ent_id];
                    List<string> prev_postfix;
                    string prev_entry = entry.Key;
                    //Look for more occurences in the rest of the entity's sequence
                    while(!stop)
                    {
                        prev_postfix = postfix;
                        if (check_ins)
                            proj = canProjectBy(alpha, cleared, patterns[entry.Key].Item1, ent_id, proj.Key, proj.Value.Value, last_time);
                        else
                            proj = canProjectBy(alpha, entry.Value, patterns[entry.Key].Item1, ent_id, proj.Key, proj.Value.Value, last_time);
                        postfix = proj.Value.Key;
                        if (postfix != null)
                        {
                            prev_entry = ent_id + "." + index;
                            projDB.Add(prev_entry, postfix);
                            updated = getUpdatedEndTime(ent_id, last_time, proj.Value.Value);
                            projPatterns.Add(prev_entry, new Tuple<string, int>(patterns[entry.Key].Item1 + proj.Value.Value, updated));
                            if (!patIDs.Contains(ent_id))
                                patIDs.Add(ent_id);
                        }
                        else
                            stop = true;
                        index++;
                    }
                    projSubids[ent_id] = index;
                }
            }
            instance_counter = patIDs.Count;
            return new TransformedDB(projDB, projPatterns, projSubids, alpha, instance_counter);
        }

        //
        private int getUpdatedEndTime(string id, int last_time, string slc)
        {
            EventSequence es = CCMiner.mainDB.seqs[id];
            if (slc[0] == Constants.MEET_REP)
                slc = slc.Substring(1);
            if (slc[0] == Constants.CO_REP)
                slc = slc.Substring(1);
            string tosearch = slc.Substring(0, slc.Length - 1);
            EventInterval ei2 = es.intsdic[tosearch];
            int time = ei2.fin_time;
            if (last_time < 0 || last_time > time)
                return time;
            return last_time;
        }

        //The CBackScan technique - including the the backward extension mechanism call
        public bool CBackScan(List<string> BS, string cur_slc)
        {
            //Add the current backward extension slices
            List<string> be = backwardExtensionCheck(cur_slc);
            foreach (string slc in BS)
                if (!be.Contains(slc))
                    be.Add(slc);
            foreach (string slc in be)
            {
                //For each starting backward extension slice
                if (slc.IndexOf(Constants.ST_REP) > 0)
                {
                    string slice = slc;
                    if (slc[0] == Constants.MEET_REP)
                        slice = slc.Substring(1);
                    if (slc[0] == Constants.CO_REP)
                        slice = slc.Substring(1);
                    //Check for inclusion of the corresponding finishing slice
                    if (be.Contains(slice.Replace(Constants.ST_REP, Constants.FIN_REP)))
                        return true;
                }
            }
            return false;
        }

        //Finding backward extension slices
        public List<string> backwardExtensionCheck(string cur_slc)
        {
            List<string> bef = ithBefore(cur_slc);
            List<string> co = ithCo(cur_slc);
            foreach (string coco in co)
            {
                if (!bef.Contains(coco))
                    bef.Add(coco);
            }
            return bef;
        }

        //Returns the slices that appears in the i-th before-checking period in the DB for the last slice
        private List<string> ithBefore(string slice)
        {
            Dictionary<string, int> ith_bef = new Dictionary<string, int>();
            Dictionary<string, int> meet_ith_bef = new Dictionary<string, int>();
            int app_counter = 0;
            foreach (KeyValuePair<string, List<string>> entry in trans_db)
            {
                List<string> seq_coes = entry.Value; //Split entry to coincidences
                int last_time = patterns[entry.Key].Item2;
                KeyValuePair<int, KeyValuePair<List<string>, string>> proj = 
                    canProjectBy(slice, seq_coes, patterns[entry.Key].Item1, getID(entry.Key), 0, "", last_time); //Returns co index and postfix
                int cpb = proj.Key;
                //bool skip_one;
                List<string> suspect;
                List<string> meet_suspect;
                while (cpb >= 0)
                {
                    //skip_one = (seq_coes.Count > 0 && seq_coes[0].IndexOf(Constants.CO_REP) >= 0);
                    suspect = new List<string>();
                    meet_suspect = new List<string>();
                    app_counter++;
                    List<string> cur_coes = new List<string>();
                    for (int i = 0; i < cpb; i++)
                        cur_coes.Add("");
                    string pref;
                    for (int i = 0; i < cpb; i++)
                    {
                        /*if (skip_one)
                        {
                            skip_one = false;
                            continue;
                        }*/
                        pref = "";
                        if (i == 0)
                        {
                            if (CoincidenceManager.isPartialCo(seq_coes[i]))
                            {
                                pref = Constants.CO_REP + "";
                            }
                            else if (seq_coes[i].IndexOf(Constants.MEET_REP) >= 0)
                            {
                                pref = Constants.MEET_REP + "";
                            }
                        }
                        else if (i == 1)
                        {
                            if (seq_coes[i].IndexOf(Constants.MEET_REP) >= 0 && 
                                CoincidenceManager.isPartialCo(seq_coes[i - 1]))
                            {
                                pref = Constants.MEET_REP + "";
                            }
                        }
                        bool b1 = i == cpb - 1 && seq_coes[cpb].IndexOf(Constants.MEET_REP) >= 0;
                        bool b2 = slice[0] == Constants.MEET_REP;
                        if (b1 != b2)
                        {
                            List<string> toAdd1 = CoincidenceManager.getSymPartOfSlices
                                (CoincidenceManager.splitCoToSlices(seq_coes[i]));
                            List<string> conc_toAdd1 = CoincidenceManager.splitCoToSlices(seq_coes[i]);
                            int ind1 = 0;
                            foreach (string s in toAdd1)
                            {
                                string stmp = s[0] == Constants.CO_REP || s[0] == Constants.MEET_REP ?
                                    s.Substring(1) : s;
                                string cs = conc_toAdd1[ind1];
                                string cstmp = cs[0] == Constants.CO_REP || cs[0] == Constants.MEET_REP ?
                                    cs.Substring(1) : cs;
                                if (!meet_suspect.Contains(pref + stmp) &&
                                    (stmp.IndexOf(Constants.FIN_REP) >= 0 ||
                                    (maxGapHolds(getID(entry.Key), last_time, cstmp)
                                    && maxGapHolds(getID(entry.Key), getInt(getID(entry.Key), cstmp).
                                    st_time, proj.Value.Value))))
                                {
                                    meet_suspect.Add(pref + stmp);
                                }
                                ind1++;
                            }
                            continue;
                        }
                        List<string> toAdd = CoincidenceManager.getSymPartOfSlices
                            (CoincidenceManager.splitCoToSlices(seq_coes[i]));
                        List<string> conc_toAdd = CoincidenceManager.splitCoToSlices(seq_coes[i]);
                        int ind = 0;
                        foreach (string s in toAdd)
                        {
                            string stmp = s[0] == Constants.CO_REP || s[0] == Constants.MEET_REP ?
                                s.Substring(1) : s;
                            string cs = conc_toAdd[ind];
                            string cstmp = cs[0] == Constants.CO_REP || cs[0] == Constants.MEET_REP ?
                                cs.Substring(1) : cs;
                            if (!suspect.Contains(pref + stmp) &&
                                (stmp.IndexOf(Constants.FIN_REP) >= 0 || 
                                (maxGapHolds(getID(entry.Key), last_time, cstmp)
                                && maxGapHolds(getID(entry.Key), getInt(getID(entry.Key), cstmp).st_time, 
                                proj.Value.Value))))
                            {
                                suspect.Add(pref + stmp);
                            }
                            ind++;
                        }
                        /*
                        List<string> toAdd = CoincidenceManager.getSymPartOfSlices
                            (CoincidenceManager.splitCoToSlices(seq_coes[i]));
                        cur_coes[i] = seq_coes[i]; 
                        fillSliceList(suspect, toAdd, cur_coes, patterns[entry.Key].Item1, getID(entry.Key), last_time);
                        */
                    }
                    //Add occurences of the slices that appeared in the current occurence of 'slice'
                    foreach (string sus in suspect)
                    {
                        if (ith_bef.ContainsKey(sus))
                            ith_bef[sus]++;
                        else
                            ith_bef.Add(sus, 1);
                    }
                    foreach (string sus in meet_suspect)
                    {
                        if (meet_ith_bef.ContainsKey(sus))
                            meet_ith_bef[sus]++;
                        else
                            meet_ith_bef.Add(sus, 1);
                    }
                    proj = canProjectBy(slice, seq_coes, patterns[entry.Key].Item1, getID(entry.Key), proj.Key, proj.Value.Value, last_time);
                    cpb = proj.Key;
                }
            }
            //Take those that always appear
            List<string> ib = new List<string>();
            foreach (KeyValuePair<string, int> entry in ith_bef)
            {
                if (entry.Value == app_counter)
                {
                    string tmp = entry.Key[0] == Constants.MEET_REP || entry.Key[0] == Constants.CO_REP ?
                        entry.Key.Substring(1) : entry.Key;
                    if (!ib.Contains(tmp))
                    {
                        ib.Add(tmp);
                    }
                }
            }
            foreach (KeyValuePair<string, int> entry in meet_ith_bef)
            {
                if (entry.Value == app_counter)
                {
                    string tmp = entry.Key[0] == Constants.MEET_REP || entry.Key[0] == Constants.CO_REP ?
                        entry.Key.Substring(1) : entry.Key;
                    if (!ib.Contains(tmp))
                    {
                        ib.Add(tmp);
                    }
                }
            }
            return ib;
        }


        //Returns the slices that appears in the i-th co-occuring-checking period in the DB for the last slice
        private List<string> ithCo(string slice)
        {
            string slicetmp = slice[0] == Constants.CO_REP || slice[0] == Constants.MEET_REP ?
                    slice.Substring(1) : slice;
            Dictionary<string, int> ith_co = new Dictionary<string, int>();
            int app_counter = 0;
            foreach (KeyValuePair<string, List<string>> entry in trans_db)
            {
                List<string> seq_coes = entry.Value;  //Split entry to coincidences
                int last_time = patterns[entry.Key].Item2;
                KeyValuePair<int, KeyValuePair<List<string>, string>> proj = 
                    canProjectBy(slice, seq_coes, patterns[entry.Key].Item1, getID(entry.Key), 0, "", last_time);
                int cpb = proj.Key;
                List<string> suspect;
                while (cpb >= 0)
                {
                    app_counter++;
                    string pref = "";
                    suspect = new List<string>();
                    if (CoincidenceManager.isPartialCo(seq_coes[cpb]))
                    {
                        pref = Constants.CO_REP + "";
                    }
                    else if (seq_coes[cpb].IndexOf(Constants.MEET_REP) >= 0 && (cpb == 0
                        || (cpb == 1 && CoincidenceManager.isPartialCo(seq_coes[cpb - 1]))))
                    {
                        pref = Constants.MEET_REP + "";
                    }
                    List<string> toAdd = CoincidenceManager.getSymPartOfSlices
                        (CoincidenceManager.splitCoToSlices(seq_coes[cpb]));
                    foreach (string s in toAdd)
                    {
                        string stmp = s[0] == Constants.CO_REP || s[0] == Constants.MEET_REP ?
                            s.Substring(1) : s;
                        if (stmp.Equals(slicetmp))
                        {
                            break;
                        }
                        if (!suspect.Contains(pref + stmp))
                        {
                            suspect.Add(pref + stmp);
                        }
                    }
                    //suspect = getSlicesBeforeInCo(slice, seq_coes, cpb, patterns[entry.Key].Item1, getID(entry.Key), last_time);
                    //Add occurences of the slices that appeared in the current occurence of 'slice'
                    foreach (string sus in suspect)
                    {
                        if (ith_co.ContainsKey(sus))
                            ith_co[sus]++;
                        else
                            ith_co.Add(sus, 1);
                    }
                    proj = canProjectBy(slice, seq_coes, patterns[entry.Key].Item1, getID(entry.Key), proj.Key, proj.Value.Value, last_time);
                    cpb = proj.Key;
                }
            }
            //Take those that always appear
            List<string> ic = new List<string>();
            foreach (KeyValuePair<string, int> entry in ith_co)
            {
                if (entry.Value == app_counter)
                {
                    string tmp = entry.Key[0] == Constants.MEET_REP || entry.Key[0] == Constants.CO_REP ?
                        entry.Key.Substring(1) : entry.Key;
                    if (!ic.Contains(tmp))
                    {
                        ic.Add(tmp);
                    }
                }
            }
            return ic;
        }
        //Returns the slices that appear before slice in the given coincidence
        //Also the slices that appear in the previous co, if exists and partial are added
        private List<string> getSlicesBeforeInCo(string slc, List<string> coes, int index, string pat, string id, int last_time)
        {
            List<string> befSlices = new List<string>();
            List<string> prev_slices = new List<string>();
            if (index > 0 && CoincidenceManager.isPartialCo(coes[index - 1]))
                prev_slices = CoincidenceManager.getSymPartOfSlices(CoincidenceManager.splitCoToSlices(coes[index - 1]));
            List<string> cur_coes = new List<string>();
            for (int i = 0; i < index + 1; i++)
                cur_coes.Add(coes[i]);
            cur_coes[index] = "";
            fillSliceList(befSlices, prev_slices, cur_coes, pat, id, last_time);
            if (slc[0] == Constants.MEET_REP) return befSlices;
            //Added slices that appears in the previous coincidence after the previous slice if exist
            string cur_slc = slc;
            if (slc[0] == Constants.MEET_REP) cur_slc = cur_slc.Substring(1);
            cur_coes[index] = CoincidenceManager.getPartBefore(coes[index], cur_slc);
            List<string> allSlices = CoincidenceManager.splitCoToSlices(cur_coes[index]);
            //List<string> allSlices = CoincidenceManager.getSymPartOfSlices(CoincidenceManager.splitCoToSlices(cur_coes[index]));
            fillSliceList(befSlices, allSlices, cur_coes, pat, id, last_time);
            return befSlices;
        }
        //Fills the list by new slices that can be projected from the db that appears in the other list
        private void fillSliceList(List<string> befSlices, List<string> prev_slices, List<string> coes, 
            string pat, string id, int last_time) 
        {
            foreach (string slice in prev_slices)
            {
                if (slice[0] == Constants.MEET_REP)
                {
                    if (!befSlices.Contains(slice) && canProjectBy(slice, coes, pat, id, 0, "", last_time).Key >= 0)
                        befSlices.Add(slice);
                    if (!befSlices.Contains(slice.Substring(1)) && canProjectBy(slice.Substring(1), coes, pat, id, 0, "", last_time).Key >= 0)
                        befSlices.Add(slice.Substring(1));
                    continue;
                }
                if (!befSlices.Contains(slice))
                    befSlices.Add(slice);
            }
        }

        //Returns the index of co in sequence in which slc appears and the corresponding postfix
        private static KeyValuePair<int, KeyValuePair<List<string>, string>> canProjectBy
            (string slice, List<string> fullCoes, string curr_pattern, string id, int co_to_start, string prev_oc, int time)
        {
            int i = 0;
            bool found = false;
            List<string> seqRest = new List<string>();
            string rest;
            string app = "";
            KeyValuePair<string, string> ret;
            for (i = co_to_start; i < fullCoes.Count && !found; i++)
            {
                string prv = i == co_to_start ? prev_oc : "";
                ret = getRestCo(slice, i, fullCoes, curr_pattern, id, prv, time);
                rest = ret.Key;
                app = ret.Value;
                if (rest != null)
                {
                    found = true;
                    if (rest.Length > 0)
                        seqRest.Add(rest);
                }
            }
            if (found)
            {
                for (int k = i; k < fullCoes.Count; k++)
                    seqRest.Add(fullCoes[k]);
                return new KeyValuePair<int, KeyValuePair<List<string>, string>>(i - 1, 
                    new KeyValuePair<List<string>, string>(seqRest, app));
            }
            return new KeyValuePair<int, KeyValuePair<List<string>, string>>(-1, 
                new KeyValuePair<List<string>, string>(null, null));
        }

        //
        private static KeyValuePair<string, string> getRestCo(string slice, int co_index, List<string> fullCoes, 
            string pattern, string id, string prev_oc, int time)
        {
            string slc = slice.Substring(0, slice.Length - 1);
            if (slice[0] == Constants.MEET_REP)
                slc = slc.Substring(1);
            if (slice[0] == Constants.CO_REP)
                slc = slc.Substring(1);
            if (fullCoes[co_index].IndexOf(slc) < 0) return new KeyValuePair<string, string>(null, null);
            //co_slices includes only one slice
            string rest = "";
            int j = 0;
            List<string> fullCo_slices = null;
            string app = "";
            string last_pat_slice = lastPatSlice(pattern);
            bool passed_prev = prev_oc.Equals("");
            if (fullCoes[co_index].Length > 0 && fullCoes[co_index][0] == Constants.MEET_REP) 
            //coincidence has @ inside of it
            {
                if (co_index == 0 || (co_index == 1 && CoincidenceManager.isPartialCo(fullCoes[0])))
                {
                    if (last_pat_slice.IndexOf(Constants.FIN_REP) >= 0)
                    {
                        //slice is pos: has to be @x+ (don't keep the @)
                        if (slice.IndexOf(Constants.ST_REP) >= 0 && slice.IndexOf(Constants.MEET_REP) >= 0)
                        {
                            //slice = @x+ 
                            fullCo_slices = CoincidenceManager.splitCoToSlices(fullCoes[co_index]);
                            for (int i = 0; i < fullCo_slices.Count; i++)
                            {
                                if (slice.Equals(fullCo_slices[i].Substring(0, fullCo_slices[i]
                                    .IndexOf(Constants.DUP_SYM)) + fullCo_slices[i][fullCo_slices[i].Length - 1])
                                    && maxGapHolds(id, time, fullCo_slices[i]))
                                {
                                    app = fullCo_slices[i];
                                    if (app.Equals(prev_oc))
                                    {
                                        passed_prev = true;
                                        continue;
                                    }
                                    if (!passed_prev) continue;
                                    j++;
                                    if (i < fullCo_slices.Count - 1)
                                    {
                                        for (int k = i + 1; k < fullCo_slices.Count; k++)
                                        {
                                            if (fullCo_slices[k][0] == Constants.MEET_REP)
                                            {
                                                string add = "";
                                                if (fullCo_slices[k].Substring(1)[0] != Constants.CO_REP)
                                                    add += Constants.CO_REP;
                                                rest += add + fullCo_slices[k].Substring(1);
                                            }
                                            else
                                            {
                                                string add = "";
                                                if (fullCo_slices[k][0] != Constants.CO_REP)
                                                    add += Constants.CO_REP;
                                                rest += add + fullCo_slices[k];
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                            if (j == 1)
                                return new KeyValuePair<string, string>(rest, app);
                        }
                        return new KeyValuePair<string, string>(null, null);
                    }
                    return new KeyValuePair<string, string>(null, null);
                }
                //has to be without @
                if (slice.IndexOf(Constants.MEET_REP) < 0)
                {
                    fullCo_slices = CoincidenceManager.splitCoToSlices(fullCoes[co_index]);
                    for (int i = 0; i < fullCo_slices.Count; i++)
                    {
                        if ((slice.Equals(fullCo_slices[i].Substring(0, fullCo_slices[i]
                            .IndexOf(Constants.DUP_SYM)) + fullCo_slices[i][fullCo_slices[i].Length - 1]) ||
                            (fullCo_slices[i][0] == Constants.MEET_REP && 
                                slice.Equals(fullCo_slices[i].Substring(1, fullCo_slices[i]
                                .IndexOf(Constants.DUP_SYM) - 1) + fullCo_slices[i][fullCo_slices[i].Length - 1])))
                                && maxGapHolds(id, time, fullCo_slices[i]))
                        {
                            app = fullCo_slices[i];
                            if (app[0] == Constants.MEET_REP)
                                app = app.Substring(1);
                            if (app.Equals(prev_oc)) {
                                passed_prev = true;
                                continue;
                            }
                            if (!passed_prev) continue;
                            if (app.IndexOf(Constants.FIN_REP) >= 0 &&
                                !CoincidenceManager.checkNoEndBeforeStart(app, pattern))
                                continue;
                            j++;
                            if (i < fullCo_slices.Count - 1)
                            {
                                for (int k = i + 1; k < fullCo_slices.Count; k++)
                                {
                                    if (fullCo_slices[k][0] == Constants.MEET_REP)
                                    {
                                        string add = "";
                                        if (fullCo_slices[k].Substring(1)[0] != Constants.CO_REP)
                                            add += Constants.CO_REP;
                                        rest += add + fullCo_slices[k].Substring(1);
                                    }
                                    else
                                    {
                                        string add = "";
                                        if (fullCo_slices[k][0] != Constants.CO_REP)
                                            add += Constants.CO_REP;
                                        rest += add + fullCo_slices[k];
                                    }
                                }
                            }
                            break;
                        }
                    }
                    if (j == 1)
                        return new KeyValuePair<string, string>(rest, app);
                }
                return new KeyValuePair<string, string>(null, null);
            }
            //coincidence has no @ in it
            fullCo_slices = CoincidenceManager.splitCoToSlices(fullCoes[co_index]);
            for (int i = 0; i < fullCo_slices.Count; i++)
            {
                if (slice.Equals(fullCo_slices[i].Substring(0, fullCo_slices[i]
                    .IndexOf(Constants.DUP_SYM)) + fullCo_slices[i][fullCo_slices[i].Length - 1])
                    && maxGapHolds(id, time, fullCo_slices[i]))
                {
                    app = fullCo_slices[i];
                    if (app.Equals(prev_oc)) {
                        passed_prev = true;
                        continue;
                    }
                    if (!passed_prev) continue;
                    if (app.IndexOf(Constants.FIN_REP) >= 0 &&
                        !CoincidenceManager.checkNoEndBeforeStart(app, pattern))
                        continue;
                    j++;
                    if (i < fullCo_slices.Count - 1)
                    {
                        for (int k = i + 1; k < fullCo_slices.Count; k++)
                        {
                            string add = "";
                            if (fullCo_slices[k][0] != Constants.CO_REP)
                                add += Constants.CO_REP;
                            rest += add + fullCo_slices[k];
                        }
                    }
                    break;
                }
            }
            if (j == 1)
                return new KeyValuePair<string, string>(rest, app);
            return new KeyValuePair<string, string>(null, null);
        }

        //Checks if next co meet slice can be preserved, if exists
        private static bool checkNextCoMeetOption(List<string> co, string pat)
        {
            if(lastPatSlice(pat).IndexOf('-') >= 0 && co.Count > 0)
                return CoincidenceManager.isPartialCo(co[0]);
            return false;
        } 
    }
}