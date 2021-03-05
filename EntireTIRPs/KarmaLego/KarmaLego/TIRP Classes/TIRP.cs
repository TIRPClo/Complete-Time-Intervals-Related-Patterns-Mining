using System.Collections.Generic;
using System.IO;
using KarmaLego.TIRPs;

namespace KarmaLego
{

    /// <summary>
    /// A data structure containing all the symbolic time interval instances, with predefined symbols and relation between them.
    /// </summary>
    public class TIRP
    {
        public int size;
        public int[] symbols;
        public int[] rels;
        public List<StiInstance> tinstancesList = new List<StiInstance>();
        public List<int> supprtingEntities = new List<int>();
        public void AddEntity(int entityIdx)
        {
            if (!supprtingEntities.Contains(entityIdx))
                supprtingEntities.Add(entityIdx);
        }
        public double meanHorizontalSupport;
        public override int GetHashCode()
        {
            int hash = 13, relSize = (size*(size-1))/2;
            for (int i = 0; i < relSize; i++)
                hash = (hash * 7) + rels[i].GetHashCode();
            return hash;
        }

        /// <summary>
        /// Extends a TIRP by adding a new symbol
        /// </summary>
        /// <param name="tirp">TIRP to extend</param>
        /// <param name="setNewSymbol"></param>
        /// <param name="seedNewRleation"></param>
        /// <param name="setCandRels"></param>
        public TIRP(TIRP tirp, int setNewSymbol, int seedNewRleation, int[] setCandRels)
        {
            size = tirp.size + 1;
            int relsSize = size * (size - 1) / 2;
            int btmRelIdx = relsSize - 1; // Bottom of the newest column in the TIRP relation half matrix
            int topRelIdx = (size - 1) * (size - 2) / 2; // Top of the newest column in the TIRP relation half matrix
            
            symbols = new int[size];
            for (int tIdx = 0; tIdx < (size - 1); tIdx++)
                symbols[tIdx] = tirp.symbols[tIdx];
            symbols[size - 1] = setNewSymbol;
            rels = new int[relsSize];
            for (int relIdx = 0; relIdx < topRelIdx ; relIdx++)
                rels[relIdx] = tirp.rels[relIdx];
            for (int relIdx = topRelIdx; relIdx < btmRelIdx; relIdx++)
                rels[relIdx] = setCandRels[relIdx - topRelIdx];
            rels[(relsSize - 1)] = seedNewRleation;
        }

        /// <summary>
        /// Creates a 2 sized TIRP 
        /// </summary>
        /// <param name="setFirstSymbol"></param>
        /// <param name="setSecondSymbol"></param>
        /// <param name="setFirstRelation">Relation between the symbols</param>
        public TIRP(int setFirstSymbol, int setSecondSymbol, int setFirstRelation)
        {
            size        = 2;
            symbols    = new int[2];
            symbols[0] = setFirstSymbol;
            symbols[1] = setSecondSymbol;
            rels        = new int[1];
            rels[0]     = setFirstRelation;
        }
        //Write the TIRP to file
        public void WriteTIRPtoFile(string outFile, int[] entitiesVec, int relationStyle)
        {
            using (FileStream fileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
            {
                TextWriter tw = new StreamWriter(fileStream);
                string writeLine = size + " ";
                string relChars = "";
                relChars = KLC.ALLEN7_RELCHARS;
                for (int i = 0; i < size; i++)
                    writeLine = writeLine + symbols[i] + "-";
                writeLine = writeLine + " ";
                for (int rIdx = 0; rIdx < size * (size - 1) / 2; rIdx++)
                    writeLine = writeLine + relChars[rels[rIdx]] + ".";
                writeLine = writeLine + " " + supprtingEntities.Count + " " + System.Math.Round((double)tinstancesList.Count / (double)supprtingEntities.Count, 2) + " "; // tinstancesList.Count + " " + supprtingEntities.Count + " ";
                for (int ins = 0; ins < tinstancesList.Count; ins++)
                {
                    writeLine = writeLine + entitiesVec[tinstancesList[ins].entityIdx] + " ";
                    for (int ti = 0; ti < size; ti++)
                        writeLine = writeLine + "[" + tinstancesList[ins].sti[ti].startTime + "-" + tinstancesList[ins].sti[ti].endTime + "]";
                    writeLine = writeLine + " ";
                }
                tw.WriteLine(writeLine);
            }
        }
        //
        public void WriteTIRPtoFile(TextWriter tw, entityKarma[] entitiesVec, int karmalegologi, int[][] logiRelsInxs, int print, int relationStyle)
        {
            string writeLine = size + " ";
            string relChars = "";
            if (relationStyle == KLC.RELSTYLE_ALLEN7)
                relChars = KLC.ALLEN7_RELCHARS;

            if (karmalegologi == KLC.forwardMining) // .KarmaLego)
                for (int i = 0; i < size; i++)
                    writeLine = writeLine + symbols[i] + "-";
            else
                for (int i = size - 1; i >= 0; i--)
                    writeLine = writeLine + symbols[i] + "-";
            writeLine = writeLine + " ";
            if (karmalegologi == KLC.forwardMining) // .KarmaLego)
                for (int rIdx = 0; rIdx < size * (size - 1) / 2; rIdx++)
                    writeLine = writeLine + relChars[rels[rIdx]] + "."; //writeLine = writeLine + KLC.ALLEN7_RELCHARS[rels[rIdx]] + ".";
            else
                for (int rIdx = 0; rIdx < size * (size - 1) / 2; rIdx++)
                {
                    int logiRelIdx = logiRelsInxs[size - 2/*3*/][rIdx];
                    writeLine = writeLine + relChars[rels[logiRelIdx]] + ".";  //writeLine = writeLine + KLC.ALLEN7_RELCHARS[rels[logiRelIdx]] + ".";
                }

            writeLine = writeLine + " " + supprtingEntities.Count + " " + (double)tinstancesList.Count / (double)supprtingEntities.Count + " "; // tinstancesList.Count + " " + entitieVerticalSupport.Count + " ";
            for (int ins = 0; ins < tinstancesList.Count; ins++)
            {
                writeLine = writeLine + entitiesVec[tinstancesList[ins].entityIdx].entityID + " ";
                if (karmalegologi == KLC.forwardMining) // .KarmaLego)
                    for (int ti = 0; ti < size; ti++)
                        writeLine = writeLine + "[" + tinstancesList[ins].sti[ti].startTime + "-" + tinstancesList[ins].sti[ti].endTime + "]";
                else
                    for (int ti = size - 1; ti >= 0; ti--)
                        writeLine = writeLine + "[" + tinstancesList[ins].sti[ti].startTime + "-" + tinstancesList[ins].sti[ti].endTime + "]";
                writeLine = writeLine + " ";
            }
            tw.WriteLine(writeLine);
        }
    }
}