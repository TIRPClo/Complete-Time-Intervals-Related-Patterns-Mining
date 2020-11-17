using System.Collections.Generic;
using System.Linq;
using KarmaLego.Karma_Classes;
using KarmaLego.TIRPs;

namespace KarmaLego
{
    /// <summary>
    /// Class containing all the relevant parts to run the Karma algorithm, as well as some used by its lego counterpart
    /// </summary>
    public class Karma
    {
        bool HS1;             public bool   getHS1()                { return HS1;   }
        int  epsilon;         public int    getEpsilon()            { return epsilon; }
        //the exact value of the treshold in number of entities
        double min_ver_sup;   public double getMinVerSup()          { return min_ver_sup; }       
        int max_gap;         public int    getMaxGap()             { return max_gap; }
        //tranistion table- used for candidate generation
        int[][][] transition; public int    getFromRelation(int leftIdx, int topIdx) { return transition[leftIdx][topIdx][0]; }
                              public int    getToRelation(int leftIdx, int topIdx)   { return transition[leftIdx][topIdx][1]; }
        int entitieSize;      //amount of entities
        int[] entitiesVec;    public int[]  getEntitiesVec()        { return entitiesVec; }
                              public int    getEntityByIdx(int idx) { return entitiesVec[idx]; }
        // entityTISs maps entities ids to the entity's symbolic time intervals
        public Dictionary<int, List<SymbolicTimeInterval>> entityTISs=new Dictionary<int, List<SymbolicTimeInterval>>();
        // Symbols maps Symbol to their object
        Dictionary<int, Symbol> Symbols = new Dictionary<int, Symbol>();
                              public Dictionary<int, Symbol>.ValueCollection getSymbolicTimeIntervals() { return Symbols.Values; }
                              public int getSymbolByIDVerticalSupport(int t_id) { return Symbols[t_id].getSymbolVerticalSupport(); } 
                              public int getSymbolIndexByID(int t_id) { if (Symbols.ContainsKey(t_id)) return Symbols[t_id].SymbolINDEX; else return -1; }
                              public Symbol getSymbolByID(int t_id) { if (containsSymbolID(t_id)) return Symbols[t_id]; else return null; }
                              public bool containsSymbolID(int t_id) { return Symbols.ContainsKey(t_id); }
        public Tindex karma;
        private int relations_style = KLC.RELSTYLE_ALLEN7; //KLC.RELSTYLE_KL3;
        public int getRelStyle()
        {
            return relations_style;
        }

        /// <summary>
        /// Gets all the two sized instances for a certain relation and certain symbols.
        /// </summary>
        /// <param name="tTrgtID">First symbol id</param>
        /// <param name="tErlyID">Second symbol id</param>
        /// <param name="rel">Relation id</param>
        /// <returns></returns>
        public TIRP GetTwoSizedTirpForSymbolsAndRel(int tTrgtID, int tErlyID, int rel)
        {
            TIRP twoSzdTIRP = new TIRP(tTrgtID, tErlyID, rel);
            Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>>[] tiListDicsVec = karma.Kindex[Symbols[tTrgtID].SymbolINDEX, Symbols[tErlyID].SymbolINDEX].GetStiMapsInArrayIndexedByEntityIdForRelation(rel);
            for(int eIdx = 0; eIdx < tiListDicsVec.Count(); eIdx++) // for every entity...
            {
                Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>> tiListTiDic = tiListDicsVec[eIdx];     
                foreach (KeyValuePair<SymbolicTimeInterval, List<SymbolicTimeInterval>> tiListTi in tiListTiDic) // ...and every pair of mappings between symbolic time interavls to the list of stis they are in relation with
                    foreach (SymbolicTimeInterval tis in tiListTi.Value) // add all the two sized instances to twoSzdTIRP
                    {
                        StiInstance tisInsNew = new StiInstance(new SymbolicTimeInterval(tiListTi.Key.startTime, tiListTi.Key.endTime, tiListTi.Key.symbol), tis, eIdx);
                        twoSzdTIRP.AddEntity(eIdx); 
                        twoSzdTIRP.tinstancesList.Add(tisInsNew);
                        twoSzdTIRP.meanHorizontalSupport++;
                    }
            }
            return twoSzdTIRP;
        }
        //constructor for karma
        public Karma(int setEpsilon, int setMaxGap, double setMinVerSup, string dsFilePath, bool setHS1 , int entitieSizeLimit = KLC.NUM_OF_ENTITIES)
        {
            //set variables
            epsilon = setEpsilon;
            max_gap = setMaxGap;
            HS1 = setHS1;
            
            //read input           
            KLC.read_tids_file(dsFilePath, ref entityTISs, ref Symbols);

            entitieSize = entityTISs.Count;
            min_ver_sup = ((double)setMinVerSup / 100) * entitieSize;
            entitiesVec = new int[entitieSize];
            for (int eIdx = 0; eIdx < entitieSize; eIdx++)
                entitiesVec[eIdx] = entityTISs.Keys.ElementAt(eIdx);
            karma = new Tindex(entitieSize, relations_style);
            //load transition table
            KLC.LoadTransitionTableALLEN7(ref transition);
            RunKarma(entitieSizeLimit);
        }
        /// <summary>
        /// Runs karma for all entites up to a certain entity id
        /// </summary>
        /// <param name="to_eIdx">Entity id max non-inclusive</param>
        public void RunKarma(int to_eIdx)
        {
            int index = 0;
            int[] countOfEachRelation = new int[7];
            for (int eIdx = 0; eIdx < to_eIdx &&  eIdx < entityTISs.Count() && eIdx < entitieSize; eIdx++)
            {
                List<SymbolicTimeInterval> tisList = entityTISs[entitiesVec[eIdx]];
                for (int ti1Idx = 0; ti1Idx < tisList.Count; ti1Idx++) // time interval 1
                {
                    SymbolicTimeInterval firsTis = tisList.ElementAt(ti1Idx);
                    AddSymbol(eIdx, firsTis, ref index); // update symbol entity support
                    for (int ti2Idx = ti1Idx + 1; ti2Idx < tisList.Count; ti2Idx++) // time interval 2
                    {
                        SymbolicTimeInterval secondTis = tisList.ElementAt(ti2Idx);
                        //if (firsTis.symbol != secondTis.symbol)
                        //{
                            AddSymbol(eIdx, secondTis, ref index); // DONT update symbol entity support
                            int relation = KLC.WhichRelation(firsTis, secondTis, epsilon, max_gap, relations_style);
                            if (relation > -1)
                            {
                                countOfEachRelation[relation]++;
                                karma.AddStiToTindexRelationEntryBySymbols(Symbols[firsTis.symbol].SymbolINDEX, Symbols[secondTis.symbol].SymbolINDEX, relation, eIdx, firsTis, secondTis);
                             }
                             else
                               break;
                         //}
                     }
                }
            }  
        }

        /// <summary>
        /// Creates new Symbol only if didnt exist before. if the symbol exsits , adds supporting instance
        /// </summary>
        /// <param name="entityIdx"></param>
        /// <param name="tis"></param>
        /// <param name="index"></param>
        private void AddSymbol(int entityIdx, SymbolicTimeInterval tis, ref int index)
        {
            Symbol tc;
            if (!Symbols.ContainsKey(tis.symbol))
            {
                tc = new Symbol(tis.symbol, Symbols.Count());
                index++;
                Symbols.Add(tis.symbol, tc);
            }
            else
                tc = Symbols[tis.symbol];
            tc.AddIntervalToEntity(entityIdx, tis);
        }
    }
}