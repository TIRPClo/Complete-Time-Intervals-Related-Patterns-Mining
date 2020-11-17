using System.Collections.Generic;

namespace TIRPClo
{
    //This class is responsible for the main process of TIRPClo algorithm for the purpose of discovering entire set of TIRPs
    public class TIRPCloAlg
    {
        public static void runTIRPClo(SequenceDB tdb)
        {
            TIRPsWriter.setUp();
            //int count = 0;
            List<string> rks = new List<string>();
            foreach (KeyValuePair<string, MasterTiep> mtiep in TiepsHandler.master_tieps)
            {
                if (mtiep.Value.supporting_entities.Count < Constants.MINSUP)
                {
                    rks.Add(mtiep.Key);
                }
            }
            foreach(string rk in rks)
            {
                TiepsHandler.master_tieps.Remove(rk);
            }
            tdb.filterInfrequentTiepsFromInitialSDB();
            foreach (KeyValuePair<string, MasterTiep> mtiep in TiepsHandler.master_tieps)
            {
                //count++;
                //For each frequent start tiep
                string t = mtiep.Key;
                //Console.WriteLine(t + ", " + count + "/" + TiepsHandler.master_tieps.Count);
                if (t[t.Length - 1] == Constants.ST_REP)
                {
                    //Project the sequence db by the tiep
                    SequenceDB projDB = tdb.first_projectDB(t, mtiep.Value.supporting_entities);
                    extendTIRP(projDB, t, null);
                }
            }
        }
        //Extend frequent sequences recursively  
        private static void extendTIRP(SequenceDB projDB, string last, Dictionary<string, TiepProjector> sf)
        {
            Dictionary<string, TiepProjector> LFs = projDB.tiepsFreq_alt(last, sf);
            //Check if the grown sequence is indeed a TIRP
            if (SequenceDB.allInPairs(projDB.trans_db))
            {
                TIRPsWriter.addPattern(projDB);
            }
            //For each frequent tiep
            foreach(KeyValuePair<string, TiepProjector> z in LFs)
            {
                if (z.Value.sup_entities.Count < Constants.MINSUP)
                {
                    continue;
                }
                SequenceDB alpha_projDB = projDB.projectDB(z.Key, LFs[z.Key]);
                if (alpha_projDB.sup >= Constants.MINSUP)
                {
                    extendTIRP(alpha_projDB, z.Key, LFs);
                }
            }
        }
    }
}