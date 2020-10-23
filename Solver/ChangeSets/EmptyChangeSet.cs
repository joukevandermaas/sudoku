namespace Sudoku
{
    public readonly struct EmptyChangeSet : IChangeSet
    {
        public bool IsEmpty => true;

        public SudokuValues AddModifiedDigits(SudokuValues values)
        {
            return values;
        }

        public Puzzle ApplyToPuzzle(Puzzle puzzle)
        {
            return puzzle;
        }

        public void ApplyToRegionQueue(RegionQueue queue)
        { }
    }
}
