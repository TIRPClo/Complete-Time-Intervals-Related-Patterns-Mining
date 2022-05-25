using System;
using System.Collections.Generic;
using System.IO;
namespace KarmaLego
{
    /// <summary>
    /// Class containing constants and relation styles, as well as defining the transition table between the different relations
    /// </summary>
    public class KLC
    {
        //
        public const bool nonParallelMining = false;
        public const bool parallelMining = true;
        public const int KL_PRINT_NO = -2;
        public const int KL_PRINT_TONCEPTANDTIRPS = -1;
        //Mining Direction
        public static int forwardMining = 1;
        public static int backwardsMining = 0;
        //Dharma index hashing level
        public const int dharma_relVecSymVecSymDic = 0;
        public const int dharma_relVecSymSymDics = 1;
        public const int dharma_relSymSymDics = 2;
        //Transitivity
        public static bool KL_TRANS_YES           = true;
        public static bool KL_TRANS_NO            = false;
        //Maximal number of symbols (does not matter when using hashed index)
        public static int NUM_OF_SYMBOLS          = 1000;
        //Mximal number of entities
        public const int NUM_OF_ENTITIES          = 50000; //0;
        public static int MAX_TIRP_TONCEPTS_SIZE  = int.MaxValue;
        public static int MAX_TIRP_RELATIONS_SIZE = int.MaxValue;

        public static int ERROR_NO_RELATION       = -1;
        //Allen's 7 relations
        public const int RELSTYLE_ALLEN7          = 7;
        public const int ALLEN_BEFORE             = 0; // <
        public const int ALLEN_MEET               = 1; // m
        public const int ALLEN_OVERLAP            = 2; // o
        public const int ALLEN_FINISHBY           = 3; // f
        public const int ALLEN_CONTAIN            = 4; // c 
        public const int ALLEN_EQUAL              = 5; // =	
        public const int ALLEN_STARTS             = 6; // S	
        public const string ALLEN7_RELCHARS       = "<mofc=s-";


        /// <summary>
        /// Initializes the transition table for transitive candidate generation using Allens 7 relations
        /// </summary>
        /// <param name="transition">The transition table to be initialized</param>
        public static void LoadTransitionTableALLEN7(ref int[][][] transition)
        {
            int left, top;
            transition = new int[KLC.RELSTYLE_ALLEN7][][];
            for (left = 0; left < KLC.RELSTYLE_ALLEN7; left++)
            {
                transition[left] = new int[KLC.RELSTYLE_ALLEN7][];
                for (top = 0; top < KLC.RELSTYLE_ALLEN7; top++)
                    transition[left][top] = new int[2]; // The array represent which range of relations fits for the transition (from x to y if array is [x,y])
            }

            left = KLC.ALLEN_BEFORE; // 0
            for (top = KLC.ALLEN_BEFORE;  top <= KLC.ALLEN_STARTS; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_BEFORE, KLC.ALLEN_BEFORE);
         
            left = KLC.ALLEN_MEET; // 1;
            for (top = KLC.ALLEN_BEFORE;  top <= KLC.ALLEN_CONTAIN; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_BEFORE, KLC.ALLEN_BEFORE);
            for (top = KLC.ALLEN_EQUAL;   top <= KLC.ALLEN_STARTS; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_MEET, KLC.ALLEN_MEET);

            left = KLC.ALLEN_OVERLAP; // 2;
            setTransitions(ref transition, left, KLC.ALLEN_BEFORE,   KLC.ALLEN_BEFORE,  KLC.ALLEN_BEFORE);
            setTransitions(ref transition, left, KLC.ALLEN_MEET,     KLC.ALLEN_BEFORE,  KLC.ALLEN_BEFORE);
            setTransitions(ref transition, left, KLC.ALLEN_OVERLAP,  KLC.ALLEN_BEFORE,  KLC.ALLEN_OVERLAP);
            setTransitions(ref transition, left, KLC.ALLEN_FINISHBY, KLC.ALLEN_BEFORE,  KLC.ALLEN_OVERLAP);
            setTransitions(ref transition, left, KLC.ALLEN_CONTAIN,  KLC.ALLEN_BEFORE,  KLC.ALLEN_CONTAIN);
            setTransitions(ref transition, left, KLC.ALLEN_EQUAL,    KLC.ALLEN_OVERLAP, KLC.ALLEN_OVERLAP);
            setTransitions(ref transition, left, KLC.ALLEN_STARTS,   KLC.ALLEN_OVERLAP, KLC.ALLEN_OVERLAP);

            left = KLC.ALLEN_FINISHBY; // 3;
            for (top = KLC.ALLEN_BEFORE; top <=  KLC.ALLEN_CONTAIN; top++)
                setTransitions(ref transition, left, top, top, top);
            setTransitions(ref transition, left, KLC.ALLEN_EQUAL,    KLC.ALLEN_FINISHBY, KLC.ALLEN_FINISHBY);
            setTransitions(ref transition, left, KLC.ALLEN_STARTS,   KLC.ALLEN_OVERLAP, KLC.ALLEN_OVERLAP);

            left = KLC.ALLEN_CONTAIN; // 4;
            setTransitions(ref transition, left, KLC.ALLEN_BEFORE,   KLC.ALLEN_BEFORE,  KLC.ALLEN_CONTAIN);
            setTransitions(ref transition, left, KLC.ALLEN_MEET,     KLC.ALLEN_OVERLAP, KLC.ALLEN_CONTAIN);
            setTransitions(ref transition, left, KLC.ALLEN_OVERLAP,  KLC.ALLEN_OVERLAP, KLC.ALLEN_CONTAIN);
            for (top = KLC.ALLEN_FINISHBY;top <= KLC.ALLEN_EQUAL; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_CONTAIN, KLC.ALLEN_CONTAIN);
            setTransitions(ref transition, left, KLC.ALLEN_STARTS,   KLC.ALLEN_OVERLAP, KLC.ALLEN_CONTAIN);

            left = KLC.ALLEN_EQUAL; // 5;
            for (top = KLC.ALLEN_BEFORE; top <=  KLC.ALLEN_STARTS; top++)
                setTransitions(ref transition, left, top, top, top);

            left = KLC.ALLEN_STARTS; // 6;
            for (top = KLC.ALLEN_BEFORE; top <=  KLC.ALLEN_MEET; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_BEFORE, KLC.ALLEN_BEFORE);
            for (top = KLC.ALLEN_OVERLAP; top <= KLC.ALLEN_FINISHBY; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_BEFORE, KLC.ALLEN_OVERLAP);
            setTransitions(ref transition, left, KLC.ALLEN_CONTAIN,  KLC.ALLEN_BEFORE, KLC.ALLEN_CONTAIN);
            for (top = KLC.ALLEN_EQUAL; top <=   KLC.ALLEN_STARTS; top++)
                setTransitions(ref transition, left, top, KLC.ALLEN_STARTS, KLC.ALLEN_STARTS);

        }

        /// <summary>
        /// Sets the transition table according to the parameters
        /// </summary>
        /// <param name="transition">The transition table to be modified</param>
        /// <param name="left">Row index in the table</param>
        /// <param name="top">Column index in the table</param>
        /// <param name="set0">First possible transitive relation</param>
        /// <param name="set1">Second possible transitive relation</param>
        private static void setTransitions(ref int[][][] transition, int left, int top, int set0, int set1)
        {
            transition[left][top][0] = set0; transition[left][top][1] = set1;
        }

        /// <summary>
        /// An utility function to test if two symbolic time intervals are defined under a specific relation rule
        /// </summary>
        /// <param name="A">First symbolic time interval</param>
        /// <param name="B">Second symbolic time interval</param>
        /// <param name="relation"></param>
        /// <param name="epsilon"></param>
        /// <param name="max_gap"></param>
        /// <returns>Returns true only if the relation rule is properly carried out for the two symbolic time intervals</returns>
        public static bool checkRelationAmongTwoTIs(SymbolicTimeInterval A, SymbolicTimeInterval B, int relation, int epsilon, int max_gap)
        {
            switch (relation)
            {
                case KLC.ALLEN_BEFORE: // ALBEFORE: //'b':
                    {
                        int BsMnsAe = B.startTime - A.endTime;
                        return (BsMnsAe > epsilon && BsMnsAe < max_gap);
                    }
                case KLC.ALLEN_MEET: // ALMEET: //'m':
                    {
                        int BsMnsAs = B.startTime - A.startTime, AeMnsBs = A.endTime - B.startTime, AeMnsBe = A.endTime - B.endTime;
                        return (BsMnsAs > epsilon && EpsiloneExtendedisEqual(AeMnsBs, epsilon) && AeMnsBe < epsilon);
                    }
                case KLC.ALLEN_OVERLAP: // ALOVERLAP: //'o':
                    {
                        int BsMnsAs = B.startTime - A.startTime, AeMnsBs = A.endTime - B.startTime, AeMnsBe = A.endTime - B.endTime;
                        return (BsMnsAs > epsilon && AeMnsBs > epsilon && AeMnsBe < epsilon);
                    }
                case KLC.ALLEN_FINISHBY: // ALFINISHBY: //'F(fi)':
                    {
                        int BsMnsAs = B.startTime - A.startTime, AeMnsBe = A.endTime - B.endTime;
                        return (BsMnsAs > epsilon && EpsiloneExtendedisEqual(AeMnsBe, epsilon));
                    }
                case KLC.ALLEN_CONTAIN: // ALCONTAIN: //'c':
                    {
                        int BsMnsAs = B.startTime - A.startTime, AeMnsBe = A.endTime - B.endTime;
                        return (BsMnsAs > epsilon && AeMnsBe > epsilon);
                    }
                case KLC.ALLEN_EQUAL: // ALEQUAL: //'e':
                    {
                        int BsMnsAs = B.startTime - A.startTime, AeMnsBe = A.endTime - B.endTime;
                        return (EpsiloneExtendedisEqual(BsMnsAs, epsilon) && EpsiloneExtendedisEqual(AeMnsBe, epsilon));
                    }
                case KLC.ALLEN_STARTS: // ALSTART: //'s':
                    {
                        int BsMnsAs = B.startTime - A.startTime, AeMnsBe = A.endTime - B.endTime;
                        return (EpsiloneExtendedisEqual(BsMnsAs, epsilon) && AeMnsBe < epsilon);
                    }
            }
            return false;
        }
        /// <summary>
        /// An utility function to find the relation between two symbolic time intervals
        /// </summary>
        /// <param name="A">First symbolic time interval</param>
        /// <param name="B">Second symbolic time interval</param>
        /// <param name="epsilon"></param>
        /// <param name="max_gap"></param>
        /// <param name="relations_style">Type of relations used (Allens, etc.)</param>
        /// <returns>The relation constant as defined in KLC between the two time intervals or -1 if none fit.</returns>
        public static int WhichRelation(SymbolicTimeInterval A, SymbolicTimeInterval B, int epsilon, int max_gap, int relations_style) 
        { 
            int relation = -1;
            int AeMnsBs = A.endTime - B.startTime, BsMnsAs = B.startTime - A.startTime;
            int AeMnsBe = A.endTime - B.endTime, BsMnsAe = B.startTime - A.endTime;

            if (BsMnsAe > epsilon && BsMnsAe < max_gap)
                relation = KLC.ALLEN_BEFORE;   // ALBEFORE;
            else if (BsMnsAs > epsilon && EpsiloneExtendedisEqual(AeMnsBs, epsilon) && AeMnsBe < epsilon)
                relation = KLC.ALLEN_MEET;     // ALMEET;
            else if (BsMnsAs > epsilon && AeMnsBs > epsilon && AeMnsBe < epsilon)
                relation = KLC.ALLEN_OVERLAP;  // ALOVERLAP;
            else if (EpsiloneExtendedisEqual(BsMnsAs, epsilon) && AeMnsBe < epsilon)
                relation = KLC.ALLEN_STARTS;   // ALSTARTS;
            else if (BsMnsAs > epsilon && AeMnsBe > epsilon)
                relation = KLC.ALLEN_CONTAIN;  // ALCONTAIN;
            else if (EpsiloneExtendedisEqual(BsMnsAs, epsilon) && EpsiloneExtendedisEqual(AeMnsBe, epsilon))
                relation = KLC.ALLEN_EQUAL;    // ALEQUAL;
            else if (BsMnsAs > epsilon && EpsiloneExtendedisEqual(AeMnsBe, epsilon))
                relation = KLC.ALLEN_FINISHBY; // ALFINISHBY;

            return relation; 
        }
        
        private static bool EpsiloneExtendedisEqual(int dif, int epsilon)
        {
            return (dif <= epsilon && dif >= -epsilon);
        }
        /// <summary>
        /// Adds a toncept to the toncept dictionary if it does not exist in it already.
        /// </summary>
        /// <param name="entityIdx"></param>
        /// <param name="tis"></param>
        /// <param name="toncepts"></param>
        public static void addToncept(int entityIdx, SymbolicTimeInterval tis, ref Dictionary<int, Symbol> toncepts)
        {
            Symbol tc;
            if (!toncepts.ContainsKey(tis.symbol))
            {
                tc = new Symbol(tis.symbol, toncepts.Count);
                toncepts.Add(tis.symbol, tc);
            }
            else
                tc = toncepts[tis.symbol];
            tc.AddIntervalToEntity(entityIdx, tis);  
        }
        /// <summary>
        /// Reads the input file for the purposes of initializing the program by reading all time interavls and adding all symbols
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="entityTISs"></param>
        /// <param name="toncepts"></param>
        public static void read_tids_file(string filePath, ref Dictionary<int, List<SymbolicTimeInterval>> entityTISs, ref Dictionary<int, Symbol> toncepts)
        {
            try
            {
                TextReader tr = new StreamReader(filePath);
                string readLine = tr.ReadLine();
                while (readLine != null && !readLine.StartsWith("startToncepts"))
                {
                    readLine = tr.ReadLine();
                }
                if (!(readLine == "startToncepts" && tr.Peek() >= 0 && tr.ReadLine().StartsWith("numberOfEntities")))
                    throw new System.InvalidOperationException("incorrect file format");
                int entitiesCounter = 0;
                while (tr.Peek() >= 0)  //read the entities and their symbolic time intervals
                {
                    readLine = tr.ReadLine();
                    if (entitiesCounter >= 0 && entitiesCounter < NUM_OF_ENTITIES) //entitiesCounter >= frstEntIdx)
                    {
                        string[] mainDelimited = readLine.Split(';');
                        string entityID = mainDelimited[0].Split(',')[0];
                        readLine = tr.ReadLine();
                        mainDelimited = readLine.Split(';');
                        List<SymbolicTimeInterval> tisList = new List<SymbolicTimeInterval>();
                        for (int i = 0; i < mainDelimited.Length - 1; i++)
                        {
                            string[] tisDelimited = mainDelimited[i].Split(',');
                            int symbol = int.Parse(tisDelimited[2]);
                            SymbolicTimeInterval tis = new SymbolicTimeInterval(int.Parse(tisDelimited[0]), int.Parse(tisDelimited[1]), symbol); 
                            addToncept(entityTISs.Count, tis, ref toncepts);
                            tisList.Add(tis);
                        }
                        long dt1 = DateTime.Now.Millisecond;
                        tisList.Sort(); ;
                        long dt2 = DateTime.Now.Millisecond - dt1;
                        entityTISs.Add(int.Parse(entityID), tisList);
                    }
                    else
                        readLine = tr.ReadLine();
                    entitiesCounter++;
                }
                tr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }
    }
}