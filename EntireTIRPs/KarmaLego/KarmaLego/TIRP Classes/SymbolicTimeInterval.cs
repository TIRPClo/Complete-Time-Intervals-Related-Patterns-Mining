using System;

namespace KarmaLego
{
    /// <summary>
    /// Class representing a symbolic time interval, defined by start time, end time and symbol.
    /// </summary>
    public class SymbolicTimeInterval : IComparable
    {
        public int startTime;
        public int endTime;
        public int symbol;

        public SymbolicTimeInterval(int setStartTime, int setEndTime, int setSymbol)
        {
            startTime = setStartTime;
            endTime = setEndTime;
            symbol = setSymbol;
        }

        /// <summary>
        /// Compare symbolic time interavl with another and return the result
        /// </summary>
        /// <param name="otherTimeInterval"></param>
        /// <returns>-1 if the other time interval starts after the original time interval or if they start at the same time and the other time interval finishes after the original one
        /// If both time intervals occur at the same time, compare according to the symbol number
        /// Return 1 otherwise</returns>
        public int CompareTo(object otherTimeInterval)
        {
            if (otherTimeInterval is SymbolicTimeInterval)
            {
                SymbolicTimeInterval other = otherTimeInterval as SymbolicTimeInterval;
                if (startTime < other.startTime)
                    return -1;
                else if (startTime == other.startTime && endTime < other.endTime)
                    return -1;
                else if (startTime == other.startTime && endTime == other.endTime && symbol < other.symbol)
                    return -1;
                else
                    return 1;
            }
            else
                return 0;
        }
    }
}