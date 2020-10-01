namespace Sudoku
{
    interface ISolveStrategy
    {
        (bool, Puzzle) Apply(Puzzle puzzle);
    }
}