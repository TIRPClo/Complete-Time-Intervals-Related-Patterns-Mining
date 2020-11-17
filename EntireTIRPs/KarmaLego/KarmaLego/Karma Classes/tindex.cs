using System.Collections.Generic;

namespace KarmaLego.Karma_Classes
{
    /// <summary>
    /// A class containing a matrix of Symbol x Symbol, where each cell contains an array for which every cell represents a certain relation and contains an array where the mapping between symbolic time intervals
    /// to other symbolic time intervals is stored, indexed by entity id. <para />
    /// The symbols of the symbolic time intervals in question and the relation between them is defined by the location of the cell in the axes (Symbol, Symbol, Relation)
    /// </summary>
    public class Tindex
    {
        //two dimensional square array, NUM_OF_SYMBOLS is max amount of symbols
        public TindexRelationEntry[,] Kindex = new TindexRelationEntry[KLC.NUM_OF_SYMBOLS, KLC.NUM_OF_SYMBOLS];
        public Tindex(int seTentitieSize, int seTrelSize)
        {
            for (int i = 0; i < KLC.NUM_OF_SYMBOLS; i++)
                for (int j = 0; j < KLC.NUM_OF_SYMBOLS; j++)
                    Kindex[i, j] = new TindexRelationEntry(seTentitieSize, seTrelSize);
        }

        /// <summary>
        /// Get vertical support of 2 sized TIRPs between two symbols for a certain relation
        /// </summary>
        /// <param name="frstTndx">First symbol index</param>
        /// <param name="scndTndx">Second symbol index</param>
        /// <param name="rel">Relation between the two symbols</param>
        /// <returns>Vertical support of the two sized TIRP between the two symbols for the relation</returns>
        public int GetVerticalSupportOfSymbolToSymbolRelation(int frstTndx, int scndTndx, int rel) 
        { 
            return Kindex[frstTndx, scndTndx].supportingInstancesIndexedByRelation[rel].GetVerticalSupport(); 
        }
        public List<SymbolicTimeInterval> GetStisInRelationWithKeyStiForEntityByRelationAndSymbols(int frstTndx, int scndTndx, int rel, int eIdx, SymbolicTimeInterval tis) { return Kindex[frstTndx, scndTndx].supportingInstancesIndexedByRelation[rel].GetStisInRelationWithKeyStiForEntity(eIdx, tis); }
      
        /// <summary>
        /// Adds a symbolic time interval to the tindexRelationEntry according to the symbols given.
        /// </summary>
        /// <param name="frstTncptIdx">First symbol index</param>
        /// <param name="scndTncptIdx">Second symbol index</param>
        /// <param name="rel">Relation between the two symbols</param>
        /// <param name="eIdx">Entity id</param>
        /// <param name="tisKey">Symbolic time interval key for the map</param>
        /// <param name="tisVal">Symbolic time interval to add</param>
        public void AddStiToTindexRelationEntryBySymbols(int frstTncptIdx, int scndTncptIdx, int rel, int eIdx, SymbolicTimeInterval tisKey, SymbolicTimeInterval tisVal)
        {
            Kindex[frstTncptIdx, scndTncptIdx].AddStiToStiMapForRelationByEntityID(rel, eIdx, tisKey, tisVal);
        }
    }
}