namespace Sudoku
{
    public interface ISolveStrategy
    {
        void Apply(MutablePuzzle puzzle, RegionQueue changedRegions);
    }
}