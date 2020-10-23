using System.Collections;
using System.Collections.Generic;

namespace Sudoku
{
    public class BasicChangeSet : IChangeSet, IEnumerable<CellUpdate>
    {
        // 3 pairs of 9 bits
        // 0-8   represent rows 1-9
        // 9-17  represent columns 1-9
        // 18-26 represent boxes 1-9
        // when a bit is set that means the queue already
        // contains that region.
        private readonly List<CellUpdate> _changedCells = new List<CellUpdate>();
        private readonly RegionQueue _regionQueue = RegionQueue.GetEmpty();
        private SudokuValues _modifiedDigits = SudokuValues.None;

        public void Add(CellUpdate update)
        {
            _changedCells.Add(update);

            _regionQueue.Enqueue(RegionType.Row, update.Coordinate.Row);
            _regionQueue.Enqueue(RegionType.Column, update.Coordinate.Column);
            _regionQueue.Enqueue(RegionType.Box, update.Coordinate.Box);

            _modifiedDigits = _modifiedDigits.AddOptions(update.RemovedOptions);
        }

        public void Clear()
        {
            _changedCells.Clear();
            _regionQueue.Clear();
            _modifiedDigits = SudokuValues.None;
        }

        public bool IsEmpty => _changedCells.Count == 0;

        public Puzzle ApplyToPuzzle(Puzzle puzzle)
        {
            return puzzle.UpdateCells(_changedCells);
        }

        public void ApplyToRegionQueue(RegionQueue regionQueue)
        {
            regionQueue.EnqueueAll(_regionQueue);
        }

        public SudokuValues AddModifiedDigits(SudokuValues values)
        {
            return values.AddOptions(_modifiedDigits);
        }

        public override string ToString()
        {
            return _regionQueue.ToString() + ", digits: " + _modifiedDigits;
        }

        public IEnumerator<CellUpdate> GetEnumerator() => _changedCells.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _changedCells.GetEnumerator();

    }
}
