namespace KarmaLego.TIRPs
{
    /// <summary>
    /// Class representing an instance of a Symbolic Time Interval, containing all the intervals defining a certain TIRP
    /// </summary>
    public class StiInstance
    {
        public int entityIdx;
        public SymbolicTimeInterval[] sti;

        /// <summary>
        /// Create a two sized TIRP between two symbolic time intervals
        /// </summary>
        /// <param name="setFirst"></param>
        /// <param name="setSecond"></param>
        /// <param name="setEntityIdx"></param>
        public StiInstance(SymbolicTimeInterval setFirst, SymbolicTimeInterval setSecond, int setEntityIdx)
        {
            entityIdx = setEntityIdx;
            sti = new SymbolicTimeInterval[2];
            sti[0] = setFirst;
            sti[1] = setSecond;
        }

        /// <summary>
        /// Extends the instance stiVec by adding an additional symbolic time interval
        /// </summary>
        /// <param name="size"></param>
        /// <param name="stiVec"></param>
        /// <param name="lastTis"></param>
        /// <param name="setEntityIdx"></param>
        public StiInstance(int size, SymbolicTimeInterval[] stiVec, SymbolicTimeInterval lastTis, int setEntityIdx)
        {
            entityIdx = setEntityIdx;
            sti = new SymbolicTimeInterval[size];
            for (var i = 0; i < size - 1; i++)
                sti[i] = stiVec[i];
            sti[size - 1] = lastTis;
        }
    }
}