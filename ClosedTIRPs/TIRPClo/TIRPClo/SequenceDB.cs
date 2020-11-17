using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace TIRPClo
{
    //Represents a DB that has been transformed to sequence rep.
    public class SequenceDB
    {
        //Input transformation
        public static SequenceDB createSequencesKLF(string filePath)
        {
            TextReader tr = new StreamReader(filePath);
            string readLine = tr.ReadLine();
            //Move on until the significant start
            while (readLine != null && !readLine.StartsWith(Constants.FILE_START))
            {
                readLine = tr.ReadLine();
            }
            if (!(readLine == Constants.FILE_START &&
                tr.ReadLine().StartsWith(Constants.FILE_NUM)))
            {
                throw new InvalidOperationException(Constants.FILE_FORMAT_ERR);
            }
            //Start reading the entities
            List<Tuple<CoincidenceSequence, PatternInstance>> trans_db =
                new List<Tuple<CoincidenceSequence, PatternInstance>>();
            while (tr.Peek() >= 0)
            {
                sequenceTransformer.emptyEndTimes();
                readLine = tr.ReadLine();
                string[] mainDelimited = readLine.Split(';');
                string entityID = mainDelimited[0].Split(',')[0];
                readLine = tr.ReadLine();
                mainDelimited = readLine.Split(';');
                for (int i = 0; i < mainDelimited.Length - 1; i++)
                {
                    string[] tisDelimited = mainDelimited[i].Split(',');
                    int symbol = int.Parse(tisDelimited[2]);
                    STI ei = new STI(symbol, int.Parse(tisDelimited[0]), int.Parse(tisDelimited[1]));
                    sequenceTransformer.addIntervalToEndTimes(ei);
                }
                CoincidenceSequence cs = sequenceTransformer.eventSeqToCoincidenceSeq(entityID);
                PatternInstance pi = new PatternInstance();
                cs.entity = entityID;
                trans_db.Add(new Tuple<CoincidenceSequence, PatternInstance>(cs, pi));
            }
            tr.Close();
            return new SequenceDB(trans_db, null, 0, new List<string>());
        }
        public void filterInfrequentTiepsFromInitialSDB()
        {
            foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
            {
                Coincidence current = entry.Item1.coes;
                Coincidence previous = null;
                int removed_co_tieps;
                int removed_coincidences = 0;
                bool removed_recent = false;
                while (current != null)
                {
                    List<Tiep> tieps = current.tieps;
                    removed_co_tieps = 0;
                    for (int i = 0; i < tieps.Count + removed_co_tieps; i++)
                    {
                        if (!TiepsHandler.master_tieps.ContainsKey(tieps[i - removed_co_tieps].premitive_rep))
                        {
                            tieps.Remove(tieps[i - removed_co_tieps]);
                            removed_co_tieps++;
                        }
                    }
                    if (tieps.Count == 0)
                    {
                        removed_coincidences++;
                        removed_recent = true;
                        if (previous != null)
                        {
                            previous.next = current.next;
                        }
                    }
                    else
                    {
                        current.index -= removed_coincidences;
                        if (removed_recent)
                        {
                            current.isMeet = false;
                        }
                        if (previous == null)
                        {
                            entry.Item1.coes = current;
                        }
                        removed_recent = false;
                        previous = current;
                    }
                    current = current.next;
                }
            }
        }
        //Returns true iff the last added finishing tiep has a concrete starting one before
        public static bool checkNoEndBeforeStart(Tiep t, PatternInstance prev_pattern)
        {
            return prev_pattern.pre_matched.Contains(t.e);
        }
        //Check if all tieps appear in pairs in the given coincidence sequence
        public static bool allInPairs(List<Tuple<CoincidenceSequence, PatternInstance>> trans_db)
        {
            return trans_db[0].Item2.pre_matched.Count == 0;
        }
        //The mapping between IDs to coincidence sequences
        public List<Tuple<CoincidenceSequence, PatternInstance>> trans_db;
        public List<int> entries_prev_indexes;
        //The support of the accumulated coincidence sequence in the projected DB
        public int sup;
        //List of pre-matched tieps the db was projected by
        public List<string> pre_matched;

        public SequenceDB(List<Tuple<CoincidenceSequence, PatternInstance>> tdb, List<int> epi, int support, List<string> prem)
        {
            trans_db = tdb;
            entries_prev_indexes = epi;
            sup = support;
            pre_matched = prem;
        }

        //Returns true if the max gap constraint holds
        public static bool maxGapHolds(int time, Tiep t)
        {
            STI ei2 = t.e;
            return Constants.MAX_GAP > ei2.st_time - time;
        }
        //Extend tiep projectors
        public Dictionary<string, TiepProjector> tiepsFreq_alt(string last_t, Dictionary<string, TiepProjector> sf)
        {
            if (sf == null)
            {
                return tiepsFreq(last_t);
            }
            if (last_t[0] == Constants.CO_REP || last_t[0] == Constants.MEET_REP)
            {
                last_t = last_t.Substring(1);
            }
            Dictionary<string, TiepProjector> tieps_instances = new Dictionary<string, TiepProjector>();
            //For each tiep projector
            int spare = trans_db.Count - Constants.MINSUP;
            foreach (KeyValuePair<string, TiepProjector> si_entry in sf)
            {
                //If it is frequent and not special 
                if (si_entry.Value.sup_entities.Count < Constants.MINSUP)
                {
                    continue;
                }
                if (si_entry.Key[0] == Constants.CO_REP || si_entry.Key[0] == Constants.MEET_REP)
                {
                    continue;
                }
                bool finish = si_entry.Key[si_entry.Key.Length - 1] == Constants.FIN_REP;
                if (finish && last_t.Equals(si_entry.Key))
                {
                    continue;
                }
                bool pre = !finish;
                int tuple_index = 0;
                MasterTiep ms = TiepsHandler.master_tieps[si_entry.Key];
                //For each entry in the sequence DB
                int lack = 0;
                foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
                {
                    if (lack > spare)
                    {
                        break;
                    }
                    //Take the first non special coincidence
                    string ent_id = entry.Item1.entity;
                    Coincidence curr = entry.Item1.coes;
                    if (curr == null)
                    {
                        lack++;
                        tuple_index++;
                        continue;
                    }
                    if (curr.isCo)
                    {
                        curr = curr.next;
                        if (curr == null)
                        {
                            lack++;
                            tuple_index++;
                            continue;
                        }
                    }
                    if (curr.isMeet)
                    {
                        curr = curr.next;
                        if (curr == null)
                        {
                            lack++;
                            tuple_index++;
                            continue;
                        }
                    }
                    int start_co_index = curr.index;
                    int sp_index = entries_prev_indexes[tuple_index];
                    //If the entry is in the tiep projector
                    if (!si_entry.Value.co_starts.ContainsKey(sp_index))
                    {
                        lack++;
                        tuple_index++;
                        continue;
                    }
                    List<Tiep> ms_entity = ms.tiep_occurrences[ent_id];
                    //If it is of type finish, check if the postfix contains the specific instance corresponding to the start tiep in the 
                    //pattern instance
                    //Removed for closed TIRPs discovery
                    /*if (!pre)
                    {
                        int index = entry.Item2.sym_mss[ms_entity[0].sym];
                        if (ms_entity[index].c.index >= start_co_index)
                        {
                            add_to_instances(tiep_entry.Key, ent_id, tuple_index, tieps_instances, index);
                        }
                        else
                        {
                            lack++;
                        }
                        tuple_index++;
                        continue;
                    }*/
                    //For start tiep, look for the first instance in the postfix if exist
                    int time = entry.Item2.last;
                    int prev_start_index = si_entry.Value.co_starts[sp_index];
                    bool s = false;
                    for (int i = prev_start_index; i < ms_entity.Count; i++)
                    {
                        if (pre && !maxGapHolds(time, ms_entity[i]))
                        {
                            break;
                        }
                        int co_index = ms_entity[i].c.index;
                        if (co_index >= start_co_index)
                        {
                            add_to_instances(si_entry.Key, ent_id, tuple_index, tieps_instances, i);
                            s = true;
                            break;
                        }
                    }
                    if (!s)
                    {
                        lack++;
                    }
                    tuple_index++;
                }
            }
            //If the last tiep was start, we need to consider its finish tiep
            if (last_t[last_t.Length - 1] == Constants.ST_REP)
            {
                string fin_tp = last_t.Replace(Constants.ST_REP, Constants.FIN_REP);
                int tuple_index = 0;
                int lack = 0;
                MasterTiep ms = TiepsHandler.master_tieps[fin_tp];
                //For each entry in the sequence DB
                foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
                {
                    if (lack > spare)
                    {
                        break;
                    }
                    //Get the first non special coincidence
                    string ent_id = entry.Item1.entity;
                    Coincidence curr = entry.Item1.coes;
                    if (curr == null)
                    {
                        lack++;
                        tuple_index++;
                        continue;
                    }
                    if (curr.isCo)
                    {
                        curr = curr.next;
                        if (curr == null)
                        {
                            lack++;
                            tuple_index++;
                            continue;
                        }
                    }
                    //Add the specific corresponding finish tiep for the recently added start tiep for each record
                    int start_co_index = curr.index;
                    List<Tiep> ms_entity = ms.tiep_occurrences[ent_id];
                    bool s = false;
                    for (int i = 0; i < ms_entity.Count; i++)
                    {
                        int co_index = ms_entity[i].c.index;
                        if (co_index >= start_co_index)
                        {
                            add_to_instances(fin_tp, ent_id, tuple_index, tieps_instances, i);
                            s = true;
                            break;
                        }
                    }
                    if (!s)
                    {
                        lack++;
                    }
                    //Removed for closed TIRPs discovery
                    /*int index = entry.Item2.sym_mss[ms_entity[0].sym];
                    if (ms_entity[index].c.index >= start_co_index)
                    {
                        add_to_instances(fin_tiep, ent_id, tuple_index, tieps_instances, index);
                    }
                    else
                    {
                        lack++;
                    }*/
                    tuple_index++;
                }
            }
            //If we still did not handle the special ones we do it now
            int tup_index = 0;
            foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
            {
                string ent_id = entry.Item1.entity;
                Coincidence curr = entry.Item1.coes;
                if (curr == null)
                {
                    tup_index++;
                    continue;
                }
                handle_meet_co(curr, ent_id, tup_index, tieps_instances);
                tup_index++;
            }
            return tieps_instances;
        }
        //Collect special tieps from special coincidences
        private void handle_meet_co(Coincidence curr, string ent_id, int tuple_index,
            Dictionary<string, TiepProjector> tieps_instances)
        {
            List<Tiep> tieps = curr.tieps;
            if (curr.isCo)
            {
                for (int i = 0; i < tieps.Count; i++)
                {
                    string tiep = Constants.CO_REP + "" + tieps[i].premitive_rep;
                    Tiep tmp = tieps[i].orig;
                    add_to_instances(tiep, ent_id, tuple_index, tieps_instances, tmp.ms_index);
                }
                if (curr.next != null && curr.next.isMeet)
                {
                    curr = curr.next;
                    tieps = curr.tieps;
                    for (int i = 0; i < tieps.Count; i++)
                    {
                        string tiep = Constants.MEET_REP + "" + tieps[i].premitive_rep;
                        add_to_instances(tiep, ent_id, tuple_index, tieps_instances, tieps[i].ms_index);
                    }
                }
            }
            else if (curr.isMeet)
            {
                for (int i = 0; i < tieps.Count; i++)
                {
                    string tiep = Constants.MEET_REP + "" + tieps[i].premitive_rep;
                    add_to_instances(tiep, ent_id, tuple_index, tieps_instances, tieps[i].ms_index);
                }
            }
        }
        //Collect a tiep's instance
        private void add_to_instances(string t, string ent_id, int entry_index, 
            Dictionary<string, TiepProjector> tieps_instances, int to_add)
        {
            if (tieps_instances.ContainsKey(t))
            {
                //Not the first time we meet the tiep
                if (!tieps_instances[t].sup_entities.Contains(ent_id))
                {
                    tieps_instances[t].sup_entities.Add(ent_id);
                }
            }
            else
            {
                //First time we see the tiep
                tieps_instances.Add(t, new TiepProjector());
                tieps_instances[t].sup_entities.Add(ent_id);
            }
            if (!tieps_instances[t].co_starts.ContainsKey(entry_index))
            {
                tieps_instances[t].co_starts.Add(entry_index, to_add);
            }
        }
        //Extend tiep projectors for the first time 
        public Dictionary<string, TiepProjector> tiepsFreq(string s)
        {
            Dictionary<string, TiepProjector> tieps_instances = new Dictionary<string, TiepProjector>();
            //For each record's postfix in the current db
            int entry_index = 0;
            foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
            {
                string ent_id = entry.Item1.entity;
                int time = entry.Item2.last;
                Coincidence curr = entry.Item1.coes;
                while (curr != null)
                {
                    List<Tiep> tieps = curr.tieps;
                    //For each tiep
                    bool co = curr.isCo;
                    for (int i = 0; i < tieps.Count; i++)
                    {
                        bool finish = tieps[i].type == Constants.FIN_REP;
                        //Removed for closed TIRPs discovery
                        /*if (finish)
                        {
                            if (s.Equals(tieps[i].premitive_rep.Replace(Constants.FIN_REP, Constants.ST_REP)))
                            {
                                int index = entry.Item2.tieps[0].ms_index;
                                add_to_instances(tieps[i].premitive_rep, ent_id, entry_index, tieps_instances, index);
                            }
                            continue;
                        }*/
                        if (!finish && !maxGapHolds(time, tieps[i]))
                        {
                            break;
                        }
                        string tiep = "";
                        tiep += co ? Constants.CO_REP + "" : "";
                        tiep += tieps[i].premitive_rep;
                        Tiep tmp = tieps[i].orig == null ? tieps[i] : tieps[i].orig;
                        if (!tieps_instances.ContainsKey(tiep))
                        {
                            tieps_instances.Add(tiep, new TiepProjector());
                            tieps_instances[tiep].sup_entities.Add(ent_id);
                            //Not the first time we meet the tiep
                        }
                        else
                        {
                            if (!tieps_instances[tiep].sup_entities.Contains(ent_id))
                            {
                                tieps_instances[tiep].sup_entities.Add(ent_id);
                            }
                        }
                        if (!tieps_instances[tiep].co_starts.ContainsKey(entry_index))
                        {
                            tieps_instances[tiep].co_starts.Add(entry_index, tmp.ms_index);
                        }
                    }
                    curr = curr.next;
                }
                entry_index++;
            }
            return tieps_instances;
        }
        //True iff curr is meet and it is the first of coes or it is the second of coes when the first is partial 
        private static bool mayMeet(Coincidence curr, Coincidence coes)
        {
            return curr.isMeet && 
                ((curr == coes && !curr.isCo) || (coes.isCo && curr == coes.next));
        }
        //Get updated last time for a given pattern
        private int getUpdatedEndTime(int last_time, Tiep t)
        {
            STI ei2 = t.e;
            int time = ei2.fin_time;
            return last_time < 0 ? time : Math.Min(last_time, time);
        }
        //Project the DB for the first time
        public SequenceDB first_projectDB(string alpha, List<string> tieps_instances, ref bool is_closed, ref Dictionary<string, List<BETiep>> f_acc)
        {
            Dictionary<string, BETiep> acc = null;
            Dictionary<string, BETiep> rem;
            CoincidenceSequence proj;
            //New db records
            List<Tuple<CoincidenceSequence, PatternInstance>> projDB =
                new List<Tuple<CoincidenceSequence, PatternInstance>>();
            //New Pattern Instance
            PatternInstance newpi = null;
            //Corresponding Master Tiep
            bool is_meet = false;
            MasterTiep ms = TiepsHandler.master_tieps[alpha];
            int counter = 0;
            int entity_idx = 0;
            //For each co sequence in db
            foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
            {
                string ent_id = entry.Item1.entity;
                if (!tieps_instances.Contains(ent_id))
                {
                    continue;
                }
                List<Tiep> ms_entity = ms.tiep_occurrences[ent_id];
                int cur_co_start_index = entry.Item1.coes.index;
                rem = new Dictionary<string, BETiep>();
                for (int i = 0; i < ms_entity.Count; i++)
                {
                    Tiep occ = ms_entity[i];
                    proj = projectBy(occ, alpha, entry.Item1, occ.c, entry.Item2, is_meet);
                    if (proj != null)
                    {
                        newpi = new PatternInstance(entry.Item1.coes);
                        newpi.extendPatternInstance(occ, proj.coes);
                        int updated = occ.e.fin_time;
                        newpi.last = updated;
                        projDB.Add(new Tuple<CoincidenceSequence, PatternInstance>(proj, newpi));
                        //For ith before detection
                        Coincidence curr = i == 0 ? entry.Item1.coes : ms_entity[i - 1].c;
                        if (i > 0)
                        {
                            foreach (KeyValuePair<string, BETiep> kvp in rem)
                            {
                                if (kvp.Key[0] != Constants.CO_REP && kvp.Key[0] != Constants.MEET_REP)
                                {
                                    if (!kvp.Value.indexes.ContainsKey(counter - 1))
                                    {
                                        continue;
                                    }
                                    foreach (STI sti in kvp.Value.indexes[counter - 1])
                                    {
                                        if (maxGapHolds(sti.fin_time, occ))
                                        {
                                            rem[kvp.Key].addOcc(counter, sti);
                                        }
                                    }
                                }
                            }
                        }
                        string tmp;
                        while (curr.index != occ.c.index)
                        {
                            bool meet = curr.index == occ.c.index - 1 && occ.c.isMeet;
                            if (meet)
                            {
                                foreach (Tiep s in curr.tieps)
                                {
                                    tmp = Constants.MEET_REP + s.premitive_rep;
                                    if (entity_idx == 0 || acc.ContainsKey(tmp))
                                    {
                                        if (!rem.ContainsKey(tmp))
                                        {
                                            if (entity_idx == 0)
                                            {
                                                rem.Add(tmp, new BETiep());
                                            }
                                            else
                                            {
                                                rem.Add(tmp, acc[tmp]);
                                            }
                                        }
                                        rem[tmp].addOcc(counter, s.e);
                                    }
                                }
                            }
                            else
                            {
                                foreach (Tiep s in curr.tieps)
                                {
                                    tmp = "*" + s.premitive_rep;
                                    if ((entity_idx == 0 || acc.ContainsKey(tmp)) && maxGapHolds(s.e.fin_time, occ))
                                    {
                                        if (!rem.ContainsKey(tmp))
                                        {
                                            if (entity_idx == 0)
                                            {
                                                rem.Add(tmp, new BETiep());
                                            }
                                            else
                                            {
                                                rem.Add(tmp, acc[tmp]);
                                            }
                                        }
                                        rem[tmp].addOcc(counter, s.e);
                                    }
                                }
                            }
                            curr = curr.next;
                        }
                        //For ith co detection
                        foreach (Tiep s in curr.tieps)
                        {
                            if (s.Equals(occ))
                            {
                                break;
                            }
                            tmp = Constants.CO_REP + s.premitive_rep;
                            if (entity_idx == 0 || acc.ContainsKey(tmp))
                            {
                                if (!rem.ContainsKey(tmp))
                                {
                                    if (entity_idx == 0)
                                    {
                                        rem.Add(tmp, new BETiep());
                                    }
                                    else
                                    {
                                        rem.Add(tmp, acc[tmp]);
                                    }
                                }
                                rem[tmp].addOcc(counter, s.e);
                            }
                        }
                        counter++;
                    }
                }
                acc = rem;
                entity_idx++;
            }
            //Fill the backward extension tieps and look for a finish tiep whose existence is equivalent to being surely unclosed 
            foreach (KeyValuePair<string, BETiep> t in acc)
            {
                string tiep = t.Key.Substring(1);
                if (tiep[tiep.Length - 1] == Constants.ST_REP)
                {
                    if (!f_acc.ContainsKey(tiep))
                    {
                        f_acc.Add(tiep, new List<BETiep>());
                    }
                    f_acc[tiep].Add(t.Value);
                }
                else
                {
                    is_closed = false;
                    break;
                }
            }
            List<string> prem = new List<string>();
            prem.Add(alpha.Replace(Constants.ST_REP, Constants.FIN_REP));
            return new SequenceDB(projDB, null, ms.supporting_entities.Count, prem);
        }
        //Perform closure checking
        public bool back_scan(ref Dictionary<string, List<BETiep>> f_acc)
        {
            //For each i
            for (int i = 0; i < trans_db[0].Item2.tieps.Count; i++)
            {
                Dictionary<string, BETiep> acc = null;
                Dictionary<string, BETiep> rem = null;
                int counter = 0;
                int entity_idx = -1;
                foreach (Tuple<CoincidenceSequence, PatternInstance> entry in trans_db)
                {
                    PatternInstance pi = entry.Item2;
                    Tiep occ = pi.tieps[i];
                    Coincidence curr = pi.nexts[i];
                    if (counter == 0 || !entry.Item1.entity.Equals(trans_db[counter - 1].Item1.entity))
                    {
                        entity_idx++;
                        acc = rem;
                        if (acc != null && acc.Count == 0)
                        {
                            break;
                        }
                        rem = new Dictionary<string, BETiep>();
                    }
                    string p = curr.isCo ? Constants.CO_REP + "" : (curr.isMeet ? Constants.MEET_REP + "" : "*");
                    string tmp;
                    //For ith before detection
                    while (curr.index != occ.c.index)
                    {
                        bool meet = curr.index == occ.c.index - 1 && occ.c.isMeet;
                        if (meet)
                        {
                            foreach (Tiep s in curr.tieps)
                            {
                                tmp = p + Constants.MEET_REP + s.premitive_rep; 
                                if (entity_idx == 0 || acc.ContainsKey(tmp))
                                {
                                    if(!rem.ContainsKey(tmp))
                                    {
                                        if (entity_idx == 0)
                                        {
                                            rem.Add(tmp, new BETiep());
                                        }
                                        else
                                        {
                                            rem.Add(tmp, acc[tmp]);
                                        }
                                    }
                                    rem[tmp].addOcc(counter, s.e);
                                }
                            }
                        }
                        else
                        {
                            foreach (Tiep s in curr.tieps)
                            {
                                tmp = p + "*" + s.premitive_rep;
                                if ((entity_idx == 0 || acc.ContainsKey(tmp)) && maxGapHolds(s.e.fin_time, occ))
                                {
                                    if (!rem.ContainsKey(tmp))
                                    {
                                        if (entity_idx == 0)
                                        {
                                            rem.Add(tmp, new BETiep());
                                        }
                                        else
                                        {
                                            rem.Add(tmp, acc[tmp]);
                                        }
                                    }
                                    rem[tmp].addOcc(counter, s.e);
                                }
                            }
                        }
                        p = curr.isCo && curr.next.isMeet ? Constants.MEET_REP + "" : "*";
                        curr = curr.next;
                    }
                    //For ith co detection
                    foreach (Tiep s in curr.tieps)
                    {
                        if (s.Equals(occ) || occ.Equals(s.orig))
                        {
                            break;
                        }
                        tmp = p + Constants.CO_REP + s.premitive_rep;
                        if (entity_idx == 0 || acc.ContainsKey(tmp))
                        {
                            if (!rem.ContainsKey(tmp))
                            {
                                if (entity_idx == 0)
                                {
                                    rem.Add(tmp, new BETiep());
                                }
                                else
                                {
                                    rem.Add(tmp, acc[tmp]);
                                }
                            }
                            rem[tmp].addOcc(counter, s.e);
                        }
                    }
                    counter++;
                }
                acc = rem;
                if (acc.Count == 0)
                {
                    continue;
                }
                //Fill the backward extension tieps
                foreach (KeyValuePair<string, BETiep> t in acc)
                {
                    string tiep = t.Key.Substring(2);
                    if (tiep[tiep.Length - 1] == Constants.ST_REP)
                    {
                        if (!f_acc.ContainsKey(tiep))
                        {
                            f_acc.Add(tiep, new List<BETiep>());
                        }
                        f_acc[tiep].Add(t.Value);
                    }
                }
                //Look for a finish one whose start one is also there
                foreach (KeyValuePair<string, BETiep> t in acc)
                {
                    string tiep = t.Key.Substring(2);
                    if (tiep[tiep.Length - 1] == Constants.FIN_REP)
                    {
                        tiep = tiep.Replace(Constants.FIN_REP, Constants.ST_REP);
                        if(f_acc.ContainsKey(tiep) && checkStFinMatch(f_acc[tiep], t.Value))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        //Project the DB after the first time
        public SequenceDB projectDB(string alpha, TiepProjector tiep_instances)
        {
            CoincidenceSequence proj;
            //New db records
            List<Tuple<CoincidenceSequence, PatternInstance>> projDB =
                new List<Tuple<CoincidenceSequence, PatternInstance>>();
            List<int> proj_indexes = new List<int>();
            //New Pattern Instance
            PatternInstance newpi = null;
            //Corresponding Master Tiep
            string temp_alpha = alpha;
            bool is_meet = false, is_co = false;
            if (temp_alpha[0] == Constants.MEET_REP)
            {
                is_meet = true;
                temp_alpha = temp_alpha.Substring(1);
            }
            else if (temp_alpha[0] == Constants.CO_REP)
            {
                is_co = true;
                temp_alpha = temp_alpha.Substring(1);
            }
            MasterTiep ms = TiepsHandler.master_tieps[temp_alpha];
            //For each co sequence in db
            bool st = temp_alpha[temp_alpha.Length - 1] == Constants.ST_REP;
            Tiep occ;
            List<string> sup_ents = new List<string>();
            foreach (KeyValuePair<int,int> sp_entry in tiep_instances.co_starts)
            {
                Tuple<CoincidenceSequence, PatternInstance> entry = trans_db[sp_entry.Key];
                string ent_id = entry.Item1.entity;
                List<Tiep> ms_entity = ms.tiep_occurrences[ent_id];
                int last_time = entry.Item2.last;
                for (int i = sp_entry.Value; i < ms_entity.Count; i++)
                {
                    if (ms_entity[i].time > entry.Item2.ptime)
                    {
                        continue;
                    }
                    if (st && !maxGapHolds(last_time, ms_entity[i]))
                    {
                        break;
                    }
                    occ = ms_entity[i];
                    proj = projectBy(occ, alpha, entry.Item1, occ.c, entry.Item2, is_meet);
                    if (proj != null)
                    {
                        if (!sup_ents.Contains(ent_id))
                        {
                            sup_ents.Add(ent_id);
                        }
                        newpi = new PatternInstance();
                        newpi.copyPatternToExtend(entry.Item2);
                        newpi.extendPatternInstance(occ, proj.coes);
                        int updated = getUpdatedEndTime(last_time, occ);
                        newpi.last = updated;
                        projDB.Add(new Tuple<CoincidenceSequence, PatternInstance>(proj, newpi));
                        proj_indexes.Add(sp_entry.Key);
                    }
                    if (is_co || is_meet || !st)
                    {
                        break;
                    }
                }
            }
            List<string> prem = new List<string>(pre_matched);
            if (temp_alpha[temp_alpha.Length - 1] == Constants.ST_REP)
            {
                prem.Add(temp_alpha.Replace(Constants.ST_REP, Constants.FIN_REP));
            }
            else
            {
                prem.Remove(temp_alpha);
            }
            return new SequenceDB(projDB, proj_indexes, sup_ents.Count, prem);
        }
        //Project the coincidence by the tiep
        public static CoincidenceSequence projectBy(Tiep occ, string tiep,
            CoincidenceSequence coseq, Coincidence c, PatternInstance pi, bool is_meet)
        {
            KeyValuePair<Coincidence, bool> ret = getCoRest(occ, tiep, coseq, c, pi, is_meet);
            if(ret.Value == false)
            {
                return null;
            }
            CoincidenceSequence cs = new CoincidenceSequence();
            cs.coes = ret.Key;
            cs.entity = coseq.entity;
            cs.partial = ret.Key != null && ret.Key.index == c.index ? ret.Key : null;
            return cs;
        }
        //Get the postfix of the projection
        //@ is meet
        public static KeyValuePair<Coincidence, bool> getCoRest(Tiep occ, string tiep,
            CoincidenceSequence coseq, Coincidence c, PatternInstance pi, bool is_meet)
        {
            Coincidence curr = coseq.partial != null && coseq.partial.index == c.index ? coseq.partial : c;
            return getCoRest(tiep, curr, occ, pi);
        }
        //Get the postfix of the projection
        public static KeyValuePair<Coincidence, bool> getCoRest(string srep, Coincidence curr, Tiep occ, PatternInstance pi)
        {
            Tiep app;
            Coincidence rest = null;
            List<Tiep> fullCo_tieps = curr.tieps;
            for (int i = 0; i < fullCo_tieps.Count; i++)
            {
                bool pred = srep[0] == Constants.CO_REP ? occ == fullCo_tieps[i].orig : 
                    fullCo_tieps[i] == occ;
                if (pred)
                {
                    app = fullCo_tieps[i];
                    if (app.type == Constants.FIN_REP && !checkNoEndBeforeStart(app, pi))
                    {
                        return new KeyValuePair<Coincidence, bool>(null, false);
                    }
                    rest = new Coincidence();
                    rest.index = curr.index;
                    rest.isCo = true;
                    if (i < fullCo_tieps.Count - 1)
                    {
                        for (int k = i + 1; k < fullCo_tieps.Count; k++)
                        {
                            Tiep toAdd = null;
                            if (curr.isCo)
                            {
                                toAdd = fullCo_tieps[k];
                            }
                            else
                            {
                                toAdd = new Tiep(fullCo_tieps[k]);
                            }
                            rest.tieps.Add(toAdd);
                        }
                    }
                    rest.next = curr.next;
                    if(rest.tieps.Count == 0)
                    {
                        rest = rest.next;
                    }
                    return new KeyValuePair<Coincidence, bool>(rest, true);
                }
            }
            return new KeyValuePair<Coincidence, bool>(null, false);
        }

        //Look for a match between some start be-tiep to a corresponding finish be-tiep 
        public bool checkStFinMatch(List<BETiep> sts, BETiep fin)
        {
            foreach (BETiep st in sts)
            {
                if (checkStFinMatch(st, fin))
                {
                    return true;
                }
            }
            return false;
        }
        //Look for a match between a start be-tiep to a corresponding finish be-tiep 
        public bool checkStFinMatch(BETiep st, BETiep fin)
        {
            List<string> ents = new List<string>();
            foreach (KeyValuePair<int, List<STI>> entry in fin.indexes)
            {
                if (st.indexes.ContainsKey(entry.Key) && checkStFinMatch(st.indexes[entry.Key], entry.Value))
                {
                    ents.Add(trans_db[entry.Key].Item1.entity);
                }
            }
            bool closed = sup == ents.Count;
            return closed;
        }
        //Look for a match between the start stis to the corresponding finish stis 
        public static bool checkStFinMatch(List<STI> sts, List<STI> fins)
        {
            foreach (STI f in fins)
            {
                if (sts.Contains(f))
                {
                    return true;
                }
            }
            return false;
        }
        //Look for a match between some start be-tiep to a corresponding finish tiep projector 
        public bool checkStFinMatch(List<BETiep> sts, TiepProjector fin)
        {
            foreach (BETiep st in sts)
            {
                if (checkStFinMatch(st, fin))
                {
                    return true;
                }
            }
            return false;
        }
        //Look for a match between a start be-tiep to a corresponding finish tiep projector 
        public bool checkStFinMatch(BETiep st, TiepProjector fin)
        {
            List<string> ents = new List<string>();
            foreach (KeyValuePair<int, int> sp_entry in fin.co_starts)
            {
                if (st.indexes.ContainsKey(sp_entry.Key) && checkStFinMatch(st.indexes[sp_entry.Key], sp_entry.Value))
                {
                    ents.Add(trans_db[sp_entry.Key].Item1.entity);
                }
            }
            bool closed = sup == ents.Count;
            return closed;
        }
        //Look for a match between start stis to a specific sti index 
        public static bool checkStFinMatch(List<STI> sts, int fin)
        {
            foreach (STI s in sts)
            {
                if (s.index >= fin)
                {
                    return true;
                }
            }
            return false;
        }
    }
}