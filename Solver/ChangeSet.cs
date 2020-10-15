using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class ChangeSet
    {
        public static ChangeSet Empty { get; } = new ChangeSet(new List<Cell>());
        public static ChangeSet All(Puzzle puzzle) => new ChangeSet(puzzle.Cells.ToList());

        // 3 pairs of 9 bits
        // 0-8   represent rows 1-9
        // 9-17  represent columns 1-9
        // 18-26 represent boxes 1-9
        // when a bit is set that means the queue already
        // contains that region.
        private readonly List<Cell> _changedCells;
        private readonly RegionQueue _regionQueue;

        public ChangeSet(List<Cell> changedCells)
        {
            _changedCells = changedCells;
            _regionQueue = RegionQueue.GetEmpty();

            foreach (var cell in _changedCells)
            {
                _regionQueue.Enqueue(RegionType.Row, cell.Row);
                _regionQueue.Enqueue(RegionType.Column, cell.Column);
                _regionQueue.Enqueue(RegionType.Box, cell.Box);
            }
        }

        public IEnumerable<Cell> ChangedCells => _changedCells;

        public Puzzle ApplyTo(Puzzle puzzle)
        {
            return puzzle.UpdateCells(_changedCells);
        }

        public void ApplyTo(RegionQueue regionQueue)
        {
            regionQueue.EnqueueAll(_regionQueue);
        }

        public override string ToString()
        {
            return _regionQueue.ToString();
        }
    }
}
