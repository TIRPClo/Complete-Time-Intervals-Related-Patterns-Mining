using System;
using System.Collections.Generic;
using System.Text;

namespace VertTIRP.ti
{
    public class TI_node
    {

        public TI ti;
        public TI_node next;
        public TI_node ant;

        public TI_node(string sym = "", Nullable<int> start = null, Nullable<int> end = null)
        {
            this.ti = new TI(sym, start, end);
            this.next = null;
            this.ant = null;
        }

        public override string ToString()
        {
            return "{sym: " + this.ti.sym.ToString() + ", start: " + this.ti.start.ToString() + ", end: " + this.ti.end.ToString() + "}";
        }

        public bool less_than(TI_node node)
        {
            if (this.ti.start < node.ti.start)
            {
                return true;
            }
            else
            {
                if (this.ti.start == node.ti.start)
                {
                    if (this.ti.end < node.ti.end)
                    {
                        return true;
                    }
                    else
                    {
                        if (this.ti.end == node.ti.end)
                        {
                            return TI.compareSymbols(this.ti.sym, node.ti.sym) < 0;
                        }
                        else
                        {
                            return false;
                        } 
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
