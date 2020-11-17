using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Represents an event interval
    public class EventInterval
    {
        //The symbol
        public string sym;
        //Starts by
        public int st_time;
        //Finishes by
        public int fin_time;

        public EventInterval(string sym_par, int st, int fin)
        {
            sym = sym_par;
            st_time = st;
            fin_time = fin;
        }

        //compares two event intervals
        public int Compare(EventInterval other)
        {
            int st_dif = st_time - other.st_time;
            if (st_dif != 0) return st_dif;
            int fin_dif = fin_time - other.fin_time;
            if (fin_dif != 0) return fin_dif;
            int sym_dif = IncisionStrategy.compareSyms(sym, other.sym);
            return sym_dif;
        }
    }
}
