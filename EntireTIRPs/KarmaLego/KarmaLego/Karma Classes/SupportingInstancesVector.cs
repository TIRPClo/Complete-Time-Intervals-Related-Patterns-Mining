using System.Collections.Generic;
using System.Linq;

namespace KarmaLego.Karma_Classes
{
    /// <summary>
    /// For a ceration relation between two symbols, this class contains a map between a Symbolic Time Interval to all Symbolic Time Intervals for which the relation takes place for them.
    /// </summary>
    public class SupportingInstancesVector
    {
        Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>>[] stiMapsInArrayIndexedByEntityID;
        private List<int> verticalSupport = new List<int>(); 
        public int horizontalSupport;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entitieSize">Entity size limit</param>
        public SupportingInstancesVector(int entitieSize)
        { 

            stiMapsInArrayIndexedByEntityID = new Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>>[entitieSize];
            for (int i = 0; i < entitieSize; i++)
                stiMapsInArrayIndexedByEntityID[i] = new Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>>();
        }

        /// <summary>
        /// For a certain entity, this method adds a symbolic time interval to the list of symbolic time intervals the key symbolic time interval is in relation with.
        /// </summary>
        /// <param name="eIdx">Entity ID</param>
        /// <param name="tisKey">The key Symbolic Time Interval</param>
        /// <param name="tisVal">The symbolic time interval to add</param>
        public void AddStiToStiMapForEntity(int eIdx, SymbolicTimeInterval tisKey, SymbolicTimeInterval tisVal)
        {
            if (stiMapsInArrayIndexedByEntityID[eIdx].ContainsKey(tisKey))
                stiMapsInArrayIndexedByEntityID[eIdx][tisKey].Add(tisVal);
            else
            {
                List<SymbolicTimeInterval> stiList = new List<SymbolicTimeInterval>();
                stiList.Add(tisVal);
                stiMapsInArrayIndexedByEntityID[eIdx].Add(tisKey, stiList);
                AddEntity(eIdx);
                horizontalSupport++;
            }
        }

        /// <summary>
        /// Adds entity to the vertical support if it does not already exist
        /// </summary>
        /// <param name="entityIdx"></param>
        private void AddEntity(int entityIdx) 
        {
            if (!verticalSupport.Contains(entityIdx))
                verticalSupport.Add(entityIdx); 
        } 
        public int GetVerticalSupport() { return verticalSupport.Count(); }
        /// <summary>
        /// For an entity and a predefined relation, gets the mapping between key symbolic time intervals to the symbolic time intervals they are in relation with.
        /// </summary>
        /// <returns></returns>
        public Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>>[] GetStiMapsInArrayIndexedByEntityId() { return stiMapsInArrayIndexedByEntityID; }
        /// <summary>
        /// For an entity and a predefined relation, Gets all symbolic time intervals in relation with the key symbolic time interval 
        /// </summary>
        /// <param name="eIdx">Entity id</param>
        /// <param name="tis">Key symbolic time interval</param>
        /// <returns></returns>
        public List<SymbolicTimeInterval> GetStisInRelationWithKeyStiForEntity(int eIdx, SymbolicTimeInterval tis) { if(stiMapsInArrayIndexedByEntityID[eIdx].ContainsKey(tis)) return stiMapsInArrayIndexedByEntityID[eIdx][tis]; else return null; }
        
    }
}