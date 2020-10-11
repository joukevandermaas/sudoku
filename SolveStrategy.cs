namespace Sudoku
{
    interface ISolveStrategy
    {
        (bool, Puzzle) Apply(in Puzzle puzzle);
    }
}