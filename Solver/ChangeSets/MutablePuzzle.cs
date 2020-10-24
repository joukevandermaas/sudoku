using System;
using System.Collections.Generic;

namespace Sudoku
{
    public class MutablePuzzle
    {
        private IndexablePosition[] _cells;
        private ChangeSet _currentChangeSet = new ChangeSet();

        public Puzzle Puzzle { get; }

        public MutablePuzzle(IndexablePosition[] cells)
        {
            _cells = cells;
            Puzzle = new Puzzle(_cells);
        }

        public IChangeSet Snapshot()
        {
            _currentChangeSet.Clear();

            return _currentChangeSet;
        }

        public Puzzle Clone()
        {
            var newCells = new IndexablePosition[_cells.Length];
            Array.Copy(_cells, newCells, _cells.Length);

            return new Puzzle(newCells);
        }

        public void Absorb(MutablePuzzle other)
        {
            Array.Copy(other._cells, _cells, _cells.Length);
            _currentChangeSet.Queue.EnqueueAll(RegionQueue.GetFull());
        }

        public void RemoveOptions(Coordinate coordinate, SudokuValues removedOptions)
        {
            var row = coordinate.GlobalRowIndex;
            var col = coordinate.GlobalColumnIndex;
            var box = coordinate.GlobalBoxIndex;

            var currentValue = _cells[row].RowValue;
            var newValue = currentValue.RemoveOptions(removedOptions);

            _cells[row] = _cells[row].SetRowValue(newValue);
            _cells[col] = _cells[col].SetColumnValue(newValue);
            _cells[box] = _cells[box].SetBoxValue(newValue);

            _currentChangeSet.Queue.Enqueue(RegionType.Row, row / Puzzle.LineLength);
            _currentChangeSet.Queue.Enqueue(RegionType.Column, col / Puzzle.LineLength);
            _currentChangeSet.Queue.Enqueue(RegionType.Box, box / Puzzle.LineLength);
        }

        public void RemoveOptions(CellUpdate update) => RemoveOptions(update.Coordinate, update.RemovedOptions);

        public void RemoveOptions(IEnumerable<CellUpdate> newValues)
        {
            foreach (var cell in newValues)
            {
                RemoveOptions(cell);
            }
        }

        public override string ToString()
        {
            return Puzzle.ToString();
        }

        private class ChangeSet : IChangeSet
        {
            public RegionQueue Queue { get; } = RegionQueue.GetEmpty();

            public bool IsEmpty => Queue.IsEmpty;

            public void ApplyToPuzzle(MutablePuzzle puzzle)
            {
                throw new NotImplementedException();
            }

            public void ApplyToRegionQueue(RegionQueue queue)
            {
                queue.EnqueueAll(Queue);
            }

            public override string ToString()
            {
                return $"{Queue}";
            }

            public void Clear()
            {
                Queue.Clear();
            }
        }
    }
}
