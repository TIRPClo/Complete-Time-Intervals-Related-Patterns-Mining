using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ZMiner
{

    class IntListEqualityComparer : IEqualityComparer<List<int>>
    {
        public bool Equals(List<int> le1, List<int> le2)
        {
            if (le1 == null && le2 == null)
                return true;
            else if (le1 == null || le2 == null)
                return false;
            else if (le1.Count != le2.Count)
                return false;
            for (int i = 0; i < le1.Count; i++)
            {
                if (le1[i].CompareTo(le2[i]) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(List<int> obj)
        {
            int hCode = 0;
            for (int i = 0; i < obj.Count; i++)
            {
                hCode ^= obj[i].GetHashCode();
            }
            return hCode.GetHashCode();
        }
    }

    public class ZMiner
    {
        object mode;
        object oldFL;
        int comparisoncount;
        int totalfrequency;
        Dictionary<string, double> constraints;
        Database database;
        bool forgettable;
        Dictionary<int, Dictionary<List<int>, Dictionary<string, Dictionary<string, Dictionary<EventInterval, 
            List<EventInterval>>>>>> FL2;
        Dictionary<int, Dictionary<List<int>, Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>>>> FLk;
        long t1, timeout;

        public ZMiner(Database database, Dictionary<string, double> constraints, long t1, bool forgettable=true, 
            object mode=null, object oldFL=null)
        {
            this.FL2 = new Dictionary<int, Dictionary<List<int>, Dictionary<string, Dictionary<string,
                Dictionary<EventInterval, List<EventInterval>>>>>>();
            this.FLk = new Dictionary<int, Dictionary<List<int>, Dictionary<string, Dictionary<string, 
                List<Tuple<List<EventInterval>, int>>>>>>();
            this.mode = mode;
            this.oldFL = oldFL;
            this.comparisoncount = 0;
            this.totalfrequency = 0;
            this.constraints = constraints;
            this.database = database;
            this.forgettable = forgettable;
            this.t1 = t1;
            this.timeout = t1 + (long)this.constraints["timeoutseconds"] * 1000;
        }

        public void pruneWithMinSup()
        {
            Dictionary<int, int> copiedEvents = new Dictionary<int, int>(this.database.getInitialSupport());
            foreach(KeyValuePair<int, int> label2support in this.database.getInitialSupport())
            {
                int label = label2support.Key;
                int support = label2support.Value;
                if(support < this.constraints["minSup"])
                {
                    copiedEvents.Remove(label);
                }
            }
            this.database.setInitialSupport(copiedEvents);
            foreach (EventSequence seq in this.database.getSequences())
            {
                List<EventInterval> prunedSequences = new List<EventInterval>();
                foreach(EventInterval eve in seq.getSequences())
                {
                    if(this.database.getInitialSupport().ContainsKey(eve.getLabel()) &&
                        this.database.getInitialSupport()[eve.getLabel()] >= this.constraints["minSup"])
                    {
                        prunedSequences.Add(eve);
                    }
                }
                seq.setSequences(prunedSequences);
            }
            return;
        }

        public void createZTable(string output)
        {
            this.FL2.Add(1, new Dictionary<List<int>, Dictionary<string, Dictionary<string, 
                Dictionary<EventInterval, List<EventInterval>>>>>(new IntListEqualityComparer()));
            this.FL2.Add(2, new Dictionary<List<int>, Dictionary<string, Dictionary<string, 
                Dictionary<EventInterval, List<EventInterval>>>>>(new IntListEqualityComparer()));
            foreach (EventSequence S in this.database.getSequences())
            {
                foreach (EventInterval s1 in S.getSequences())
                {
                    List<int> E1 = new List<int>();
                    E1.Add(s1.getLabel());
                    if (!this.FL2[1].ContainsKey(E1))
                    {
                        this.FL2[1].Add(E1, new Dictionary<string, Dictionary<string, 
                            Dictionary<EventInterval, List<EventInterval>>>>());
                        this.FL2[1][E1].Add("", new Dictionary<string, Dictionary<EventInterval, List<EventInterval>>>());
                        this.FL2[1][E1][""].Add(S.getID(), new Dictionary<EventInterval,
                            List<EventInterval>>());
                        this.FL2[1][E1][""][S.getID()].Add(s1, new List<EventInterval>());
                    }
                    else if (!this.FL2[1][E1][""].ContainsKey(S.getID()))
                    {
                        this.FL2[1][E1][""].Add(S.getID(), new Dictionary<EventInterval, 
                            List<EventInterval>>());
                        this.FL2[1][E1][""][S.getID()].Add(s1, new List<EventInterval>());
                    }
                    else if (!this.FL2[1][E1][""][S.getID()].ContainsKey(s1))
                    {
                        this.FL2[1][E1][""][S.getID()].Add(s1, new List<EventInterval>());
                    }
                    foreach (EventInterval s2 in S.getSequences())
                    {
                        if (s1.CompareTo(s2) < 0)
                        {
                            string R2 = Utils.getRelation(s1, s2, this.constraints);
                            if (R2 != null)
                            {
                                List<int> E2 = new List<int>();
                                E2.Add(s1.getLabel());
                                E2.Add(s2.getLabel());
                                if (!this.FL2[2].ContainsKey(E2))
                                {
                                    this.FL2[2].Add(E2, new Dictionary<string, Dictionary<string, 
                                        Dictionary<EventInterval, List<EventInterval>>>>());
                                    this.FL2[2][E2].Add(R2, new Dictionary<string, 
                                        Dictionary<EventInterval, List<EventInterval>>>());
                                    this.FL2[2][E2][R2].Add(S.getID(), new Dictionary<EventInterval, 
                                        List<EventInterval>>());
                                    this.FL2[2][E2][R2][S.getID()].Add(s1, new List<EventInterval>());
                                }
                                else if (!this.FL2[2][E2].ContainsKey(R2))
                                {
                                    this.FL2[2][E2].Add(R2, new Dictionary<string, Dictionary<EventInterval, 
                                        List<EventInterval>>>());
                                    this.FL2[2][E2][R2].Add(S.getID(), new Dictionary<EventInterval, 
                                        List<EventInterval>>());
                                    this.FL2[2][E2][R2][S.getID()].Add(s1, new List<EventInterval>());
                                }
                                else if (!this.FL2[2][E2][R2].ContainsKey(S.getID()))
                                {
                                    this.FL2[2][E2][R2].Add(S.getID(), new Dictionary<EventInterval, 
                                        List<EventInterval>>());
                                    this.FL2[2][E2][R2][S.getID()].Add(s1, new List<EventInterval>());
                                }
                                else if (!this.FL2[2][E2][R2][S.getID()].ContainsKey(s1))
                                {
                                    this.FL2[2][E2][R2][S.getID()].Add(s1, new List<EventInterval>());
                                }
                                this.FL2[2][E2][R2][S.getID()][s1].Add(s2);
                            }
                        }
                    }
                }
            }
            foreach (List<int> E2 in new List<List<int>>(this.FL2[2].Keys))
            {
                foreach (string R2 in new List<string>(this.FL2[2][E2].Keys))
                {
                    if (this.FL2[2][E2][R2].Count < this.constraints["minSup"])
                    {
                        this.FL2[2][E2].Remove(R2);
                    }
                    else
                    {
                        this.totalfrequency = totalfrequency + 1;
                    }
                }
                if (this.FL2[2][E2].Count == 0)
                {
                    this.FL2[2].Remove(E2);
                }
            }
            if (this.constraints["printF"] == 1)
            {
                foreach (List<int> one_sized in this.FL2[1].Keys)
                {
                    if (this.FL2[1][one_sized][""].Keys.Count < this.constraints["minSup"])
                    {
                        continue;
                    }
                    using (FileStream fileStream = new FileStream(output, FileMode.Append, FileAccess.Write))
                    {
                        TextWriter tw = new StreamWriter(fileStream);
                        string instances = " ";
                        if (this.constraints["printL"] == 1)
                        {
                            for (int i = 0; i < this.FL2[1][one_sized][""].Keys.Count; i++)
                            {

                                Dictionary<EventInterval, List<EventInterval>> eInstances = 
                                    this.FL2[1][one_sized][""][this.FL2[1][one_sized][""].Keys.ElementAt(i)];
                                for (int j = 0; j < eInstances.Keys.Count; j++)
                                    instances += this.FL2[1][one_sized][""].Keys.ElementAt(i) + " [" +
                                        eInstances.Keys.ElementAt(j).getStart() + "-" + 
                                        eInstances.Keys.ElementAt(j).getEnd() + "] ";
                            }
                        }
                        tw.WriteLine("1 " + one_sized[0] + "- -. " + this.FL2[1][one_sized][""].Keys.Count + " " +
                            this.FL2[1][one_sized][""].Keys.Count + instances);
                        tw.Close();
                    }
                }
            }
        }

        public List<EventInterval> getZTableSecondTable(List<int> E, string R, string Si, EventInterval s1)
        {
            if (this.FL2[2].ContainsKey(E) && this.FL2[2][E].ContainsKey(R) && this.FL2[2][E][R].ContainsKey(Si) && 
                this.FL2[2][E][R][Si].ContainsKey(s1))
            {
                return this.FL2[2][E][R][Si][s1];
            }
            return null;
            /*try{
                return this.FL2[2][E][R][Si][s1];
            }
            catch
            {
                return null;
            }*/
        }

        public void initZ(EventInterval s1, EventInterval s2, int ek, string Rnew, string Si, string Rprev, 
            Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>> Ak)
        {
            List<int> to_check = new List<int>();
            to_check.Add(s2.getLabel());
            to_check.Add(ek);
            List<EventInterval> candidates = this.getZTableSecondTable(to_check, Rnew, Si, s2);
            if(candidates == null)
            {
                return;
            }
            foreach (EventInterval s3 in candidates)
            {
                if(s3.getStart() - Math.Min(s1.getEnd(), s2.getEnd()) >= this.constraints["gap"])
                {
                    continue;
                }
                string Rk = Rprev;
                List<int> pair = new List<int>();
                pair.Add(s1.getLabel());
                pair.Add(s3.getLabel());
                bool foundRelation = false;
                foreach (string Rcand in this.FL2[2][pair].Keys)
                {
                    this.comparisoncount += 1;
                    List<EventInterval> relatArr = this.getZTableSecondTable(pair, Rcand, Si, s1);
                    if(relatArr != null && relatArr.Contains(s3))
                    {
                        Rk += "_" + Rcand;
                        foundRelation = true;
                        break;
                    }
                }
                if (foundRelation == false)
                {
                    continue;
                }
                Rk += "_" + Rnew;
                if(!Ak.ContainsKey(Rk))
                {
                    Ak.Add(Rk, new Dictionary<string, List<Tuple<List<EventInterval>, int>>>());
                    Ak[Rk].Add(Si, new List<Tuple<List<EventInterval>, int>>());
                }
                else if (!Ak[Rk].ContainsKey(Si))
                {
                    Ak[Rk].Add(Si, new List<Tuple<List<EventInterval>, int>>());
                }
                List<EventInterval> intervals = new List<EventInterval>();
                intervals.Add(s1);
                intervals.Add(s2);
                intervals.Add(s3);
                Tuple<List<EventInterval>, int> z = new Tuple<List<EventInterval>, int>(intervals, 
                    Math.Min(Math.Min(s1.getEnd(), s2.getEnd()), s3.getEnd()));
                Ak[Rk][Si].Add(z);
            }
        }

        public Tuple<List<EventInterval>, int> moveFollowsToEarlierIntervals(Tuple<List<EventInterval>, int> z, 
            EventInterval sk, int k)
        {
            int fft = Math.Min(sk.getEnd(), z.Item2);
            List<EventInterval> newl = new List<EventInterval>(z.Item1);
            newl.Add(sk);
            Tuple<List<EventInterval>, int> znew = new Tuple<List<EventInterval>, int>(newl, fft);
            return znew;
        }

        public void extendZ(Tuple<List<EventInterval>, int>  z, int ek, string Rnew, string Si, string Rprev,
             Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>> Ak, int k)
        {
            List<int> to_check = new List<int>();
            to_check.Add(z.Item1[z.Item1.Count - 1].getLabel());
            to_check.Add(ek);
            List<EventInterval> candidates = this.getZTableSecondTable(to_check, Rnew, Si, z.Item1[z.Item1.Count - 1]);
            if(candidates == null)
            {
                return;
            }
            bool foundRelation = false;
            foreach (EventInterval sk in candidates)
            {
                if(sk.getStart() - z.Item2 >= this.constraints["gap"])
                {
                    continue;
                }
                string[] Rprevst = Rprev.Split("_").TakeLast(k - 2).ToArray();
                List<string> Rk = new List<string>();
                for (int idx = 0; idx < z.Item1.Count - 1; idx++)
                {
                    EventInterval si = z.Item1[idx];
                    foundRelation = false;
                    List<int> sik = new List<int>();
                    sik.Add(si.getLabel());
                    sik.Add(sk.getLabel());
                    if (!this.FL2[2].ContainsKey(sik))
                    {
                        break;
                    }
                    if (Rprevst[idx] == "<")
                    {
                        Rk.Add("<");
                        foundRelation = true;
                        continue;
                    }
                    foreach (string Rcand in this.FL2[2][sik].Keys)
                    {
                        this.comparisoncount += 1;
                        List<EventInterval> relatArr = this.getZTableSecondTable(sik, Rcand, Si, si);
                        if (relatArr != null && relatArr.Contains(sk))
                        {
                            Rk.Add(Rcand);
                            foundRelation = true;
                            break;
                        }
                    }
                    if (foundRelation == false)
                    {
                        break;
                    }
                }
                if (foundRelation == false)
                {
                    continue;
                }
                Tuple<List<EventInterval>, int> znew = this.moveFollowsToEarlierIntervals(z, sk, k);
                string Rk_str = Rprev;
                foreach (string R in Rk)
                {
                    Rk_str = Rk_str + "_" + R;
                }
                Rk_str = Rk_str + "_" + Rnew;
                if(!Ak.ContainsKey(Rk_str))
                {
                    Ak.Add(Rk_str, new Dictionary<string, List<Tuple<List<EventInterval>, int>>>());
                    Ak[Rk_str].Add(Si, new List<Tuple<List<EventInterval>, int>>());
                }
                else if (!Ak[Rk_str].ContainsKey(Si))
                {
                    Ak[Rk_str].Add(Si, new List<Tuple<List<EventInterval>, int>>());
                }
                Ak[Rk_str][Si].Add(znew);
            }   
        }

        public bool isFrequent(Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>>  fp, 
            string relation)
        {
            return fp[relation].Keys.Count >= this.constraints["minSup"];
        }

        public List<Tuple<List<EventInterval>, int>> getZArrangement(int k, List<int> E, string R, string Si)
        {
            return this.FLk[k][E][R][Si];
        }

        public void WriteTIRPtoFile(TextWriter tw, List<int> Eprev, string Rprev)
        {
            int[] symbols = Eprev.ToArray();
            int size = symbols.Length;
            string writeLine = size + " ";
            for (int i = 0; i < size; i++)
                writeLine = writeLine + symbols[i] + "-";
            writeLine = writeLine + " ";
            string[] rels = Rprev.Split("_");
            for (int rIdx = 0; rIdx < size * (size - 1) / 2; rIdx++)
                writeLine = writeLine + rels[rIdx] + ".";
            int horizontal_support = 0;
            string instances = "";
            if(size == 2)
            {
                foreach (KeyValuePair<string, Dictionary<EventInterval, List<EventInterval>>> eInstances in 
                    this.FL2[size][Eprev][Rprev])
                {
                    foreach (KeyValuePair<EventInterval, List<EventInterval>> edkInstances in eInstances.Value)
                    {
                        foreach (EventInterval second in edkInstances.Value)
                        {
                            horizontal_support = horizontal_support + 1;
                            if (this.constraints["printL"] == 1)
                            {
                                instances = instances + eInstances.Key + " ";
                                instances = instances + "[" + edkInstances.Key.getStart() + "-" + 
                                    edkInstances.Key.getEnd() + "]";
                                instances = instances + "[" + second.getStart() + "-" + second.getEnd() + "]";
                                instances = instances + " ";
                            }
                        }
                    }
                }
                writeLine = writeLine + " " + this.FL2[size][Eprev][Rprev].Keys.Count + " " +
                    Math.Round((double)horizontal_support / (double)this.FL2[size][Eprev][Rprev].Keys.Count, 2) + " ";
            }
            else
            {
                foreach (KeyValuePair<string, List<Tuple<List<EventInterval>, int>>> eInstances in this.FLk[size][Eprev][Rprev])
                {
                    foreach (Tuple<List<EventInterval>, int> edkInstances in eInstances.Value)
                    {
                        horizontal_support = horizontal_support + 1;
                        if (this.constraints["printL"] == 1)
                        {
                            instances = instances + eInstances.Key + " ";
                            for (int i = 0; i < edkInstances.Item1.Count; i++)
                            {
                                instances = instances + "[" + edkInstances.Item1[i].getStart() + "-" +
                                    edkInstances.Item1[i].getEnd() + "]";
                            }
                            instances = instances + " ";
                        }
                    }
                }
                writeLine = writeLine + " " + this.FLk[size][Eprev][Rprev].Keys.Count + " " +
                    Math.Round((double)horizontal_support / (double)this.FLk[size][Eprev][Rprev].Keys.Count, 2) + " ";
            }
            writeLine = writeLine + instances;
            tw.WriteLine(writeLine);
        }

        public void growZ(List<int> Eprev, string Rprev, int k, string output)
        {
            if (this.constraints["printF"] == 1)
            {
                using (FileStream fileStream = new FileStream(output, FileMode.Append, FileAccess.Write))
                {
                    TextWriter tw = new StreamWriter(fileStream);
                    this.WriteTIRPtoFile(tw, Eprev, Rprev);
                    tw.Close();
                }
            }
            if (this.constraints["timeoutseconds"] * 1000 < DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - this.t1)
            {
                Console.WriteLine("Reached timeout");
                return;
            }
            if (!this.FLk.ContainsKey(k))
            {
                this.FLk.Add(k, new Dictionary<List<int>, Dictionary<string, 
                    Dictionary<string, List<Tuple<List<EventInterval>, int>>>>>(new IntListEqualityComparer()));
            }
            foreach(int ek in this.database.getInitialSupport().Keys)
            {
                List<int> Ek = new List<int>(Eprev);
                Ek.Add(ek);
                List<int> Ek_last2 = new List<int>();
                Ek_last2.Add(Ek[Ek.Count - 2]);
                Ek_last2.Add(Ek[Ek.Count - 1]);
                List<int> Ek_firstlast = new List<int>();
                Ek_firstlast.Add(Ek[0]);
                Ek_firstlast.Add(Ek[Ek.Count - 1]);
                if (!this.FL2[2].ContainsKey(Ek_last2) || !this.FL2[2].ContainsKey(Ek_firstlast))
                {
                    continue;
                }
                if (!this.FLk[k].ContainsKey(Ek))
                {
                    this.FLk[k].Add(Ek, new Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>>());
                }
                Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>>  Ak = 
                    new Dictionary<string, Dictionary<string, List<Tuple<List<EventInterval>, int>>>>();
                foreach (string Rnew in this.FL2[2][Ek_last2].Keys)
                {
                    List<string> eIntersect = null;
                    if (k == 3)
                    {
                        eIntersect = 
                            this.FL2[2][Ek_last2][Rnew].Keys.Intersect(this.FL2[k - 1][Eprev][Rprev].Keys.ToList()).ToList();
                    }
                    else
                    {
                        eIntersect =
                            this.FL2[2][Ek_last2][Rnew].Keys.Intersect(this.FLk[k - 1][Eprev][Rprev].Keys.ToList()).ToList();
                    }
                    foreach (string Si in eIntersect)
                    {
                        if (k == 3)
                        {
                            foreach (EventInterval s1 in this.FL2[2][Eprev][Rprev][Si].Keys)
                            {
                                foreach (EventInterval s2 in this.FL2[2][Eprev][Rprev][Si][s1])
                                {
                                    this.initZ(s1, s2, ek, Rnew, Si, Rprev, Ak);
                                }
                            }
                        }
                        else
                        {
                            foreach (Tuple<List<EventInterval>, int> z in this.getZArrangement(k - 1, Eprev, Rprev, Si))
                            {
                                this.extendZ(z, ek, Rnew, Si, Rprev, Ak, k);
                            }
                        }
                    }
                }
                foreach(string Rk in Ak.Keys)
                {
                    if(this.isFrequent(Ak, Rk) == true)
                    {
                        this.FLk[k][Ek][Rk] = new Dictionary<string, List<Tuple<List<EventInterval>, int>>>();
                        foreach (KeyValuePair<string, List<Tuple<List<EventInterval>, int>>> z in Ak[Rk])
                        {
                            this.FLk[k][Ek][Rk].Add(z.Key, z.Value);
                        }
                        this.totalfrequency += 1;
                        this.growZ(Ek, Rk, k + 1, output);
                        if (forgettable == true)
                        {
                            this.FLk[k][Ek][Rk] = null;
                        }
                    }
                }
                if(this.FLk[k][Ek].Count == 0)
                {
                    this.FLk[k].Remove(Ek);
                }
            }
        }

        public static Tuple<int, int, long, bool, Dictionary<int, Dictionary<List<int>, Dictionary<string, 
            Dictionary<string, Dictionary<EventInterval, List<EventInterval>>>>>>,
            Dictionary<int, Dictionary<List<int>, Dictionary<string, Dictionary<string, 
                List<Tuple<List<EventInterval>, int>>>>>>> runZMiner(string dataname, long t1, string output, int num_entities, 
            double support, double epsilon, double gap, double timeout=int.MaxValue, int max_level=int.MaxValue, 
            bool printTIRPs=true, bool printInstances=true)
        {
            FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write);
            fileStream.Close();
            string[] args = {support + "", epsilon + "", gap + "", timeout + "",
                max_level + "", printTIRPs + "", printInstances + ""};
            string filename = dataname.Split("/")[dataname.Split("/").Length - 1].Split(".")[0];
            Console.WriteLine("TEST WITH: " + dataname + " DATASET");
            Tuple<int, int, int, double, double, Dictionary<string, List<Tuple<int, int, int>>>> tup = Utils.preprocess(dataname);
            int tseq = tup.Item1;
            int tdis = tup.Item2;
            int tintv = tup.Item3;
            double aintv = tup.Item4;
            double avgtime = tup.Item5;
            Dictionary<string, List<Tuple<int, int, int>>> dataset = tup.Item6;
            Console.WriteLine("TOTAL SEQUENCE: " + tseq);
            Console.WriteLine("TOTAL DISTINCT EVENTS: " + tdis);
            Console.WriteLine("TOTAL INTERVALS: " + tintv);
            Console.WriteLine("AVERAGE INTERVAL PER SEQ: " + aintv);
            Console.WriteLine("AVERAGE TIMESPAN: " + avgtime);
            Console.WriteLine("TEST WITH " + dataname + " DATASET");
            Database database = new Database(dataset);
            Dictionary<string, double> constraints = Utils.makeConstraints(args, dataset);
            ZMiner algorithm = new ZMiner(database, constraints, t1);
            Console.WriteLine("########## Z-MINER ##########");
            Console.WriteLine("1-1. MINIMUM SUPPORT: " + algorithm.constraints["minSup"]);
            Console.WriteLine("1-2. EPSILON CONSTRAINT: " + algorithm.constraints["epsilon"]);
            Console.WriteLine("1-3. GAP CONSTRAINT: " + algorithm.constraints["gap"]);
            Console.WriteLine("1-4. TIMEOUT: " + algorithm.constraints["timeoutseconds"]);
            Console.WriteLine("1-5. LEVEL: " + algorithm.constraints["level"]);
            Console.WriteLine("2. NUMBER OF E-SEQUENCES: " + algorithm.database.getSequences().Count);
            if(algorithm.constraints["minSup"] != 0)
            {
                algorithm.pruneWithMinSup();
            }
            algorithm.createZTable(output);
            algorithm.FLk.Add(3, new Dictionary<List<int>, Dictionary<string, 
                Dictionary<string, List<Tuple<List<EventInterval>, int>>>>>(new IntListEqualityComparer()));
            foreach (List<int> pair in algorithm.FL2[2].Keys)
            {
                foreach (string Rpair in algorithm.FL2[2][pair].Keys)
                {
                    Console.WriteLine(pair[0] + "," + pair[1] + "," + Rpair);
                    algorithm.growZ(pair, Rpair, 3, output);
                }
            }
            if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > algorithm.timeout)
            {
                Console.WriteLine("Reached timeout");
                return new Tuple<int, int, long, bool, Dictionary<int, Dictionary<List<int>, 
                    Dictionary<string, Dictionary<string, Dictionary<EventInterval, List<EventInterval>>>>>>, 
                    Dictionary<int, Dictionary<List<int>, Dictionary<string, Dictionary<string, 
                    List<Tuple<List<EventInterval>, int>>>>>>>(0, 0, 
                    DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond, true, null, null);
            }
            long t2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("3. TOTAL COMPARISON COUNTS: " + algorithm.comparisoncount);
            Console.WriteLine("4. TOTAL FREQUENT ARRANGEMENTS: " + algorithm.totalfrequency);
            Console.WriteLine("5. TOTAL TIME CONSUMED: " + (t2 - algorithm.t1));
            return new Tuple<int, int, long, bool, Dictionary<int, Dictionary<List<int>, Dictionary<string, 
                Dictionary<string, Dictionary<EventInterval, List<EventInterval>>>>>>, Dictionary<int, 
                Dictionary<List<int>, Dictionary<string, Dictionary<string, 
                List<Tuple<List<EventInterval>, int>>>>>>>(algorithm.comparisoncount, algorithm.totalfrequency, 
                t2 = algorithm.t1, false, algorithm.FL2, algorithm.FLk);
        }

        private static void notifyOutput(Dictionary<string, double> constraints, string output)
        {
            if (constraints["printF"] == 1)
            {
                Console.WriteLine("EXPORTING F ...");
                Console.WriteLine("CHECK THE FILE: " + output);
            }
            if (constraints["printL"] == 1)
            {
                Console.WriteLine("EXPORTING F ...");
                Console.WriteLine("CHECK THE FILE: " + output);
            }
        } 
    }
}
