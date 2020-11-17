using System.Collections.Generic;

namespace KarmaLego.Karma_Classes
{
    /// <summary>
    /// A class containing an array indexed by a relation number which contains all supporting instances for a relation between two predefined symbols
    /// </summary>
    public class TindexRelationEntry
    {
        int entitySizeLimit; 

        public SupportingInstancesVector[] supportingInstancesIndexedByRelation;

        /// <summary>
        /// Creates a new entry in a matrix of Symbols x Symbols, where each cell contains this entry which is an array of all supporting instances of the two symbols indexed by the realtion between them 
        /// </summary>
        /// <param name="seTentitieSizeLimit"></param>
        /// <param name="seTrelSize"></param>
        public TindexRelationEntry(int seTentitieSizeLimit, int seTrelSize)
        {
            entitySizeLimit = seTentitieSizeLimit;
            supportingInstancesIndexedByRelation = new SupportingInstancesVector[seTrelSize];
            for (int i = 0; i < seTrelSize; i++)
                supportingInstancesIndexedByRelation[i] = new SupportingInstancesVector(entitySizeLimit);
        }
        /// <summary>
        /// Gets an array containing the mapping between key symbolic time intervals to symbolic time intervals for which the relation takes place, indexed by entity id.
        /// </summary>
        /// <param name="rel"></param>
        /// <returns></returns>
        public Dictionary<SymbolicTimeInterval, List<SymbolicTimeInterval>>[] GetStiMapsInArrayIndexedByEntityIdForRelation(int rel) { return supportingInstancesIndexedByRelation[rel].GetStiMapsInArrayIndexedByEntityId(); }
        /// <summary>
        /// Adds a symbolic time interval to the mapping between key symbolic time intervals and symbolic time intervals for which the relation takes placed, indexed by entity id
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="eIdx"></param>
        /// <param name="tisKey"></param>
        /// <param name="tisVal"></param>
        public void AddStiToStiMapForRelationByEntityID(int rel, int eIdx, SymbolicTimeInterval tisKey, SymbolicTimeInterval tisVal)
        {
            supportingInstancesIndexedByRelation[rel].AddStiToStiMapForEntity(eIdx, tisKey, tisVal);
        }
    }
}