using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    public static class Constants
    {
        //Coincidence Representation
        public const bool START = true; //starting slice
        public const bool FINISH = false; //finishing slice
        public const char ST_REP = '+'; //starting slice rep.
        public const char FIN_REP = '-'; //finishing slice rep.
        public const char MEET_REP = '@'; //meet slice rep.
        public const char SEP_SYM = '^'; //seperates between symbols in the same end time list entry
        public const char DUP_SYM = '#'; //seperates symbol from its index
        public static int MINSUP = 0; //the minimum support 
        public static int MAX_GAP = 0; //max gap for < relation
        //For the coincidences creation at the final stage
        public const char CO_REP = '_';
        //For the input file 
        public static string FILE_NAME = "";
        public static string OUT_FILE = "";
        public const string FILE_START = "startToncepts";
        public const string FILE_NUM = "numberOfEntities";
        public const string FILE_FORMAT_ERR = "incorrect file format";
        //For Allen's tmporal relations
        public const string ALLEN_BEFORE = "<";
        public const string ALLEN_MEET = "m";
        public const string ALLEN_OVERLAP = "o";
        public const string ALLEN_FINISHBY = "fi";
        public const string ALLEN_CONTAIN = "c";
        public const string ALLEN_EQUAL = "=";
        public const string ALLEN_STARTS = "S";	
    }
}
