using System;
using System.Collections.Generic;
using System.Text;

namespace VertTIRP.ti
{
    public class TI : IComparable
    {

        public string sym;
        public Nullable<int> start;
        public Nullable<int> end;

        public TI(string sym = "", Nullable<int> start = null, Nullable<int> end = null)
        {
            this.sym = sym;
            this.start = start;
            this.end = end;
        }

        public override string ToString()
        {
            return this.sym + " " + this.start.ToString() + " " + this.end.ToString();
        }
        
        public bool equals(TI node)
        {
            if (!this.sym.Equals(node.sym))
            {
                return false;
            }
            else
            {
                if (this.start != node.start)
                {
                    return false;
                }
                else
                {
                    return this.end == node.end;
                }
            }
        }

        public bool less_than(TI node)
        {
            if (this.start < node.start)
            {
                return true;
            }
            else
            {
                if (this.start == node.start)
                {
                    if (this.end < node.end)
                    {
                        return true;
                    }
                    else
                    {
                        if(this.end == node.end)
                        {
                            return compareSymbols(this.sym, node.sym) < 0;
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

        public string printInterval()
        {
            string res = "[" + start + "-" + end + "]";
            return res;
        }

        public int CompareTo(object otherTimeInterval)
        {
            TI other = otherTimeInterval as TI;
            int st_dif = (int)(this.start - other.start);
            if (st_dif != 0)
            {
                return st_dif;
            }
            int end_dif = (int)(this.end - other.end);
            if (end_dif != 0)
            {
                return end_dif;
            }
            return compareSymbols(this.sym, other.sym);
        }

        public static int compareSymbols(string sym, string other_sym)
        {
            try
            {
                return int.Parse(sym) - int.Parse(other_sym);
            }
            catch
            {
                return sym.CompareTo(other_sym);
            }
        }
    }
}
