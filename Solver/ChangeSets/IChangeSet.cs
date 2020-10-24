using System.Collections.Generic;

namespace Sudoku
{
    public interface IChangeSet
    {
        bool IsEmpty { get; }
        void ApplyToRegionQueue(RegionQueue queue);
        void ApplyToPuzzle(MutablePuzzle puzzle);
    }
}
