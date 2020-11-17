using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCMiner
{
    //Responsible for the main process of CCMiner
    public class CCMiner
    {
        //Origin DB
        public static TemporalDB mainDB;

        //The CCMiner main algorithm
        public static CTP ccMiner(TemporalDB DB, bool closed)
        {
            CTP ctp = new CTP();
            mainDB = DB;
            //Transforms the db into cincidence representation
            TransformedDB tdb = DB.transformDB();
            //all frequent slices
            Dictionary<string, int> tp1 = tdb.slicesFreq();
            //backward extension slices 
            List<string> BS = null;
            //int tasks = 0;
            int count = 0;
            foreach (KeyValuePair<string, int> entry in tp1)
            {
                count++;
                Console.WriteLine(count + "/" + tp1.Count + "/" + entry.Key);
                //For each frequent starting slice
                if (entry.Value >= Constants.MINSUP && entry.Key.IndexOf(Constants.MEET_REP) < 0 && 
                    entry.Key.IndexOf(Constants.ST_REP) > 0)
                {
                    BS = new List<string>();
                    TransformedDB projDB = tdb.projectDB(entry.Key, null, false);
                    if (!closed || !tdb.CBackScan(BS, entry.Key))
                    {
                        if (closed)
                        {
                            BS.AddRange(tdb.backwardExtensionCheck(entry.Key));
                        }
                        CBIDE(projDB, entry.Key, projDB.sup, projDB.trans_db.Count, BS, ref ctp, closed);
                    }
                }
            }
            return ctp;
        }
        //The CBIDE algorithm  
        private static void CBIDE(TransformedDB projDB, string alpha, int alpha_support , 
            double totalsup, List<string> BS, ref CTP ctp, bool closed)
        {
            long dt1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Dictionary<string, int> LFs = projDB.slicesFreq();
            long t = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - dt1;
            List<string> FS = new List<string>();
            bool start = false;
            if (closed)
            {
                foreach (KeyValuePair<string, int> entry in LFs)
                {
                    //CCMiner wanted only finishing ones, but that's not enough for closed patterns recognition
                    if (/*entry.Key.IndexOf(Constants.FIN_REP) > 0 &&*/ totalsup == entry.Value)
                    {
                        if (entry.Key[entry.Key.Length - 1] == Constants.ST_REP)
                        {
                            start = true;
                            break;
                        }
                        string slice = entry.Key[0] == Constants.MEET_REP || entry.Key[0] == Constants.CO_REP ?
                            entry.Key.Substring(1) : entry.Key;
                        FS.Add(slice);
                    }
                }
            }
            //Check if none appear in pairs in BS U FS
            bool nip = closed && CoincidenceManager.noneInPairs(BS, FS);
            //Check if all appear in pairs in the accumulated coincidence
            bool aip = CoincidenceManager.allInPairs(alpha);
            //If So, there are no backward & forward extensions and this is a temporal pattern 
            if ((!closed || (!start && nip)) && aip)
            {
                //Thus - alpha is a closed temporal pattern
                ctp.addPattern(alpha, alpha_support, projDB);
            }
            //Now Continue Looking for possible extensions
            List<string> P = new List<string>();
            //For each frequent slice:
            foreach (KeyValuePair<string,int> z in LFs)
            {
                if (z.Value < Constants.MINSUP)
                    continue;
                //Finishing slices only
                if (z.Key.IndexOf(Constants.FIN_REP) >= 0)
                {
                    string tmp = z.Key[0] == Constants.CO_REP ? z.Key.Substring(1) : z.Key;
                    if (alpha.IndexOf(tmp.Replace(Constants.FIN_REP, Constants.ST_REP)) >= 0)
                    {//pre-pruning 
                        P.Add(z.Key);
                    }
                }
                else
                {
                    //Starting slices only
                    P.Add(z.Key);
                }
            }
            //For each possible extension we found:
            foreach (string z in P)
            {
                //post-pruning 
                //Notice that we project another time only by the recent added slice
                TransformedDB alpha_projDB = projDB.projectDB(z, LFs, true);
                //If the support of the extended pattern is above the minimum support requested
                if (alpha_projDB.sup >= Constants.MINSUP)
                {
                    //Copy Backward extension list
                    List<string> BStag = BS.Take(BS.Count).ToList(); 
                    //Check if extensible and update the backward extension slices
                    if (!closed || !projDB.CBackScan(BStag, z))
                    {
                        if (closed)
                        {
                            BStag.AddRange(projDB.backwardExtensionCheck(z));
                        }
                        CBIDE(alpha_projDB, alpha + z, alpha_projDB.sup, alpha_projDB.trans_db.Count, BStag, ref ctp, closed);
                    }
                }
            }
        }
    }
}
