using System;
using System.Collections.Generic;
using System.Linq;

namespace KarmaLego
{
    /// <summary>
    /// Class representing a symbol (also defined as toncept in the rest of the code)
    /// </summary>
    public class Symbol : IComparable
    {
        public int symbolID;
        public int SymbolINDEX;
        int totalHorizontalSupport;

        // A mapping between entities to all symbolic time intervals of the symbol in the entity
        private Dictionary<int, List<SymbolicTimeInterval>> supportingInstancesDictionary = new Dictionary<int, List<SymbolicTimeInterval>>(); 

        /// <summary>
        /// Adds an interval for a certain entity, modifying its horizontal and vertical support in the process
        /// </summary>
        /// <param name="entityIdx"></param>
        /// <param name="instance"></param>
        public void AddIntervalToEntity(int entityIdx, SymbolicTimeInterval instance) 
        {
            if (!supportingInstancesDictionary.ContainsKey(entityIdx))
            {
                List<SymbolicTimeInterval> strList = new List<SymbolicTimeInterval>();
                strList.Add(instance);
                supportingInstancesDictionary.Add(entityIdx, strList);
            }

            else
            {
                if(!supportingInstancesDictionary[entityIdx].Contains(instance))
                    supportingInstancesDictionary[entityIdx].Add(instance);
            }
            totalHorizontalSupport++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the vertical support of a symbol</returns>
        public int getSymbolVerticalSupport() { return supportingInstancesDictionary.Count(); }
        public int getMeanHorizontalSupport() { return totalHorizontalSupport / supportingInstancesDictionary.Count; }
        /// <summary>
        ///
        /// </summary>
        /// <returns>A dictionary mapped between entitiy id to symbolic time intervals of the symbol</returns>
        public Dictionary<int, List<SymbolicTimeInterval>> getTonceptHorizontalDic() { return supportingInstancesDictionary; } 
        public Symbol(int symbol, int setIndex)
        {
            symbolID = symbol;
            SymbolINDEX = setIndex;
        }

        /// <summary>
        /// Compares between two symbols
        /// </summary>
        /// <param name="obj">The symbol used for comparsion</param>
        /// <returns>1 if the vertical support of the original support is bigger than the recieved symbol</returns>
        public int CompareTo(object obj)
        {
            if (obj is Symbol)
            {
                Symbol other = obj as Symbol;
                return supportingInstancesDictionary.Count.CompareTo(other.getSymbolVerticalSupport());
            }
            else
                throw new ArgumentException("Object is not a Symbol");
        }
    }
}