using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Represents an end time entry in the endtime list
    public class EndTime
    {
        //The symbols in the entry
        public string syms;
        //Entry time
        public int time;
        //Entry type - start or finish
        public bool type;

        public EndTime(string sym_par, int time_par, bool type_par)
        {
            syms=sym_par;
            time = time_par;
            type = type_par;
        }

        //Adds a new symbol to the symbols in the entry
        //entry with 'x' becomes -> 'x^new_sym'
        public void addSymToSyms(string new_sym)
        {
            syms += Constants.SEP_SYM + new_sym;
        }
    }
}
