namespace Sudoku
{
    public interface ISolveStrategy
    {
        IChangeSet Apply(in Puzzle puzzle, RegionQueue changedRegions, SudokuValues changedDigits);
    }
}