namespace Sudoku
{
    public class MutablePuzzle : IChangeSet
    {
        private readonly RegionQueue _queue;
        private readonly IndexablePosition[] _cells;

        public Puzzle Puzzle { get; }

        public bool IsEmpty => _queue.IsEmpty;

        public SudokuValues ModifiedDigits { get; set; }

        public MutablePuzzle(IndexablePosition[] cells)
        {
            _cells = cells;
            _queue = RegionQueue.GetEmpty();
            Puzzle = new Puzzle(_cells);
        }

        public SudokuValues RemoveOptions(Coordinate coordinate, SudokuValues removedOptions)
        {
            var row = coordinate.GlobalRowIndex;
            var col = coordinate.GlobalColumnIndex;
            var box = coordinate.GlobalBoxIndex;

            var currentValue = _cells[row].RowValue;
            var newValue = currentValue.RemoveOptions(removedOptions);

            _cells[row] = _cells[row].SetRowValue(newValue);
            _cells[col] = _cells[col].SetColumnValue(newValue);
            _cells[box] = _cells[box].SetBoxValue(newValue);

            _queue.Enqueue(RegionType.Row, row / Puzzle.LineLength);
            _queue.Enqueue(RegionType.Column, col / Puzzle.LineLength);
            _queue.Enqueue(RegionType.Box, box / Puzzle.LineLength);

            var change = new SudokuValues(currentValue.Values & removedOptions.Values);
            ModifiedDigits = ModifiedDigits.AddOptions(change);

            return change;
        }

        public SudokuValues RemoveOptions(CellUpdate update)
        {
            return RemoveOptions(update.Coordinate, update.RemovedOptions);
        }

        public void MarkAllAsChanged()
        {
            _queue.EnqueueAll(RegionQueue.GetFull());
        }

        public Puzzle ApplyToPuzzle(Puzzle puzzle)
        {
            return Puzzle;
        }

        public void ApplyToRegionQueue(RegionQueue queue)
        {
            queue.EnqueueAll(_queue);
        }

        public override string ToString()
        {
            return _queue.ToString() + ", digits: " + ModifiedDigits;
        }

        public SudokuValues AddModifiedDigits(SudokuValues values)
        {
            return values.AddOptions(ModifiedDigits);
        }
    }
}
