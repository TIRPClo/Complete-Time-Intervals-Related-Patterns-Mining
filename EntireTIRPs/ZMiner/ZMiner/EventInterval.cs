using System;
using System.Collections.Generic;
using System.Text;

namespace ZMiner
{
    public class EventInterval : IComparable
    {
        int label;
        int start;
        int end;

        public EventInterval(int label, int start, int end)
        {
            this.label = label;
            this.start = start;
            this.end = end;
        }

        public int getDuration()
        {
            return this.end - this.start;
        }

        public string getDescription()
        {
            return "(" + this.label + ", " + this.start + ", " + this.end;
        }

        public int CompareTo(object obj)
        {
            EventInterval other = (EventInterval)obj;
            if(this.start == other.start)
            {
                if (this.end == other.end)
                {
                    return smallerThan(this.label, other.label);
                }
                else
                {
                    return smallerThan(this.end, other.end);
                }
            }
            else
            {
                return smallerThan(this.start, other.start);
            }
        }

        static int smallerThan(int n1, int n2)
        {
            if (n1 < n2)
            {
                return -1;
            }
            else if (n1 > n2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public int getLabel()
        {
            return this.label;
        }

        public int getStart()
        {
            return this.start;
        }

        public int getEnd()
        {
            return this.end;
        }
    }
}