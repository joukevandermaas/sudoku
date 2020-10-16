namespace Sudoku
{
    public interface ISolveStrategy
    {
        ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions);
    }
}