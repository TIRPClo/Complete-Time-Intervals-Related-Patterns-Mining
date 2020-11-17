using KarmaLego.TIRPs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarmaLego
{
    /// <summary>
    /// The lego part of the KarmaLego algorithm that matches the dharma hashed index rather than the karma index
    /// </summary>
    public class DLego
    {
        Dharma karma;

        public DLego(Dharma setKarma)
        {
            karma = setKarma;
        }
        /// <summary>
        /// Runs lego and prints output to file
        /// </summary>
        /// <param name="k"></param>
        /// <param name="outFile"></param>
        /// <param name="seTrans"></param>
        /// <param name="relStyle"></param>
        public static void RunLego(Dharma k, string outFile, bool seTrans, int relStyle)
        {
            int counter = 1;
            Dictionary<int, Symbol>.ValueCollection dic = k.getSymbolicTimeIntervals();
            int symbCount = dic.Count;
            //For each symbol
            foreach (Symbol symbol1 in dic)
            {
                Console.WriteLine(counter + "/" + symbCount + ", symbol: " + symbol1.symbolID);
                counter++;
                //long t = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                //If it is frequent
                if (k.getSymbolByIDVerticalSupport(symbol1.symbolID) >= k.getMinVerSup())
                {
                    int symbolID = symbol1.symbolID;
                    int t1Idx = symbol1.SymbolINDEX;
                    using (FileStream fileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                    {
                        //Print one-sized TIRP to file
                        #region initial output formatting
                        TextWriter tw = new StreamWriter(fileStream);
                        string instances = " ";
                        for (int i = 0; i < k.getSymbolByIDVerticalSupport(symbolID); i++)
                        {
                            KeyValuePair<int, List<SymbolicTimeInterval>> userInstances = k.getSymbolByID(symbolID).getTonceptHorizontalDic().ElementAt(i);
                            for (int j = 0; j < userInstances.Value.Count; j++)
                                instances += k.getEntityByIdx(userInstances.Key) + " [" + userInstances.Value.ElementAt(j).startTime + "-" + userInstances.Value.ElementAt(j).endTime + "] ";
                        }
                        tw.WriteLine("1 " + symbolID + "- -. " + k.getSymbolByIDVerticalSupport(symbolID) + " " + Math.Round((double)k.getSymbolByIDVerticalSupport(symbolID), 2) + instances);
                        #endregion
                        //For each second symbol
                        foreach (Symbol symbol2 in k.getSymbolicTimeIntervals())
                        {
                            //And for each relation
                            for (int rel = 0; rel < relStyle; rel++)
                            {
                                //If the <symbol1,symbol2,relation> entry in the index is frequent
                                if (k.getIndex().GetVerticalSupportOfSymbolToSymbolRelation(t1Idx, symbol2.SymbolINDEX, rel) >= k.getMinVerSup())
                                {
                                    //Print the 2-sized TIRP to file
                                    TIRP twoSzdTIRP = k.GetTwoSizedTirpForSymbolsAndRel(symbolID, symbol2.symbolID, rel);
                                    DLego lego = new DLego(k);
                                    twoSzdTIRP.WriteTIRPtoFile(tw, k.getEntitiesVec(), relStyle);
                                    //Extend it recursively
                                    lego.DoLego(twoSzdTIRP, tw, relStyle);
                                    lego = null;
                                }
                            }
                        }
                        tw.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Runs the Lego algorithm on every 2 sized TIRP with enough vertical support and prints every extended TIRP with enough vertical support to file
        /// </summary>
        /// <param name="tirp"></param>
        /// <param name="tw"></param>
        /// <param name="relStyle"></param>
        private void DoLego(TIRP tirp, TextWriter tw, int relStyle)
        {
            List<int[]> candidates = null;
            foreach (Symbol symbol in karma.getSymbolicTimeIntervals()) // for every symbol...
                for (int seedRelation = 0; seedRelation < relStyle; seedRelation++) // ... and every relation
                    if (karma.getIndex().GetVerticalSupportOfSymbolToSymbolRelation(karma.getSymbolIndexByID(tirp.symbols[tirp.size - 1]), symbol.SymbolINDEX, seedRelation) >= karma.getMinVerSup()) // if vertical support is high enough
                    {
                        candidates = CandidatesGeneration(tirp, seedRelation); // Generate candidate and extend the TIRP
                        for (int cand = 0; cand < candidates.Count; cand++)
                        {
                            TIRP tirpNew = new TIRP(tirp, symbol.symbolID, seedRelation, candidates.ElementAt(cand));
                            bool seedRelCnndtsEmpty = SearchSupportingInstances(ref tirpNew, tirp.tinstancesList); // Also sets the TIRP supportingEntities field
                            if (tirpNew.supprtingEntities.Count >= karma.getMinVerSup()) // If the TIRPs vertical support is big enough, write the TIRP and attempt to further extend it
                            {
                                tirpNew.WriteTIRPtoFile(tw, karma.getEntitiesVec(), relStyle);
                                DoLego(tirpNew, tw, relStyle);
                            }
                        }
                    }
        }
        /// <summary>
        /// Gets all suporting instances for the newly extended TIRP
        /// </summary>
        /// <param name="tNew"></param>
        /// <param name="tinstances"></param>
        /// <returns></returns>
        private bool SearchSupportingInstances(ref TIRP tNew, List<StiInstance> tinstances)
        {
            bool seedRelEmpty = true;
            bool[] entitieSupport = new bool[KLC.NUM_OF_ENTITIES];
            int topRelIdx = ((tNew.size - 1) * (tNew.size - 2) / 2), seedRelIdx = (tNew.size * (tNew.size - 1) / 2 - 1);
            int seedRelation = tNew.rels[seedRelIdx];
            int tncptLst = tNew.symbols[tNew.size - 2], tncptNew = tNew.symbols[tNew.size - 1];

            int lsTncptIdx = karma.getSymbolIndexByID(tncptLst);
            int nxTncptIdx = karma.getSymbolIndexByID(tncptNew);
            for (int tins = 0; tins < tinstances.Count; tins++)
            {
                seedRelEmpty = false;
                //Get all intervals where the relation takes place for the last STI added to the TIRP
                List<SymbolicTimeInterval> tisList = karma.getIndex().GetStisInRelationWithKeyStiForEntityByRelationAndSymbols(lsTncptIdx, nxTncptIdx, seedRelation, tinstances[tins].entityIdx, tinstances[tins].sti[tNew.size - 2]);
                if (tisList != null)
                {
                    //Add all the found instances to the TIRP
                    for (int i = 0; (karma.getHS1() == false && i < tisList.Count) || (karma.getHS1() == true && i < 1); i++)
                    {
                        int relIdx = 0;
                        for (relIdx = topRelIdx; relIdx < seedRelIdx; relIdx++)
                            if (!KLC.checkRelationAmongTwoTIs(tinstances[tins].sti[relIdx - topRelIdx], tisList.ElementAt(i), tNew.rels[relIdx], karma.getEpsilon(), karma.getMaxGap()))
                                break;
                        if (relIdx == seedRelIdx)
                        {
                            StiInstance newIns = new StiInstance(tNew.size, tinstances[tins].sti, tisList.ElementAt(i), tinstances[tins].entityIdx);
                            tNew.AddEntity(newIns.entityIdx);
                            tNew.tinstancesList.Add(newIns);
                        }
                    }
                }
            }
            return seedRelEmpty;
        }
        /// <summary>
        /// Generates candidates for the TIRP extension using the transitivty properties of the relations and returns them in a list.
        /// </summary>
        /// <param name="tirp">TIRP to extend</param>
        /// <param name="seedRel">Relation to extend the TIRP with</param>
        /// <returns></returns>
        private List<int[]> CandidatesGeneration(TIRP tirp, int seedRel)
        {
            int columSize = tirp.size;
            int topCndRelIdx = 0;
            int btmRelIdx = columSize - 2; // Second to bottom relation index, since the bottom cell is filled by seedRel
            List<int[]> candidatesList = new List<int[]>();
            int[] candidate = new int[columSize];
            candidate[columSize - 1] = seedRel;
            candidatesList.Add(candidate);
            for (int relIdxToSet = btmRelIdx; relIdxToSet >= topCndRelIdx; relIdxToSet--) // The current cell we are filling in the column
            {
                int leftTirpIdx = ((relIdxToSet + 1) * relIdxToSet / 2) + relIdxToSet;
                int belowCndIdx = relIdxToSet + 1;
                int candListSize = candidatesList.Count;
                for (int candIdx = 0; candIdx < candListSize; candIdx++) // For every candidate we have created so far
                {
                    candidate = candidatesList.ElementAt(candIdx);
                    int fromRel = karma.getFromRelation(tirp.rels[leftTirpIdx], candidate[belowCndIdx]);
                    int toRel = karma.getToRelation(tirp.rels[leftTirpIdx], candidate[belowCndIdx]);
                    for (int rel = fromRel; rel <= toRel; rel++) // Generate all possible relations using the transitive properties of the relations
                    {
                        if (rel > fromRel)
                        {
                            int[] newCandidate = new int[columSize];
                            for (int rIdx = columSize - 1; rIdx > relIdxToSet; rIdx--)
                                newCandidate[rIdx] = candidate[rIdx];
                            newCandidate[relIdxToSet] = rel;
                            candidatesList.Add(newCandidate);
                        }
                        else
                            candidate[relIdxToSet] = rel;
                    }
                }
            }
            return candidatesList;
        }
    }
}