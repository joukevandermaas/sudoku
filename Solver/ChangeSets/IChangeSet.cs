namespace Sudoku
{
    public interface IChangeSet
    {
        bool IsEmpty { get; }
        Puzzle ApplyToPuzzle(Puzzle puzzle);
        void ApplyToRegionQueue(RegionQueue queue);
        SudokuValues AddModifiedDigits(SudokuValues values);
    }
}
