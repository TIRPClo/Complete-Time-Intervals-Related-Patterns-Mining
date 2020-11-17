using System;
using System.Collections.Generic;
using System.Text;

namespace TIRPClo
{
    public class Constants
    {
        //Coincidence Representation
        public const bool START = true; //starting tiep
        public const bool FINISH = false; //finishing tiep
        public const char ST_REP = '+'; //starting tiep rep.
        public const char FIN_REP = '-'; //finishing tiep rep.
        public const char MEET_REP = '@'; //meet tiep rep.
        //Running Example Credentials
        public static int MINSUP = 0; //Minimum vertical support 
        public static int MAX_GAP = 0; //Maximal gap
        //For the coincidences creation at the final stage
        public const char CO_REP = '_';
        //Input & Output files 
        public static string FILE_NAME = "Smarthome_3EWD_NoCluster_KL_sorted_fixed.csv";
        public static string OUT_FILE = "Smarthome_3EWD_NoCluster_KL_sorted_fixed";
        public const string FILE_START = "startToncepts";
        public const string FILE_NUM = "numberOfEntities";
        public const string FILE_FORMAT_ERR = "incorrect file format";
        //Allen's tmporal relations
        public const char ALLEN_BEFORE = '<';
        public const char ALLEN_MEET = 'm';
        public const char ALLEN_OVERLAP = 'o';
        public const char ALLEN_FINISHBY = 'f';
        public const char ALLEN_CONTAIN = 'c';
        public const char ALLEN_EQUAL = '=';
        public const char ALLEN_STARTS = 'S';
    }
}