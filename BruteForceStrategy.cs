using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Brute-forces the puzzle until it breaks or a limit is reached
    /// </summary>
    internal class BruteForceStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            var firstCellWithTwoOptions = puzzle.GetRows().SelectMany(c => c).FirstOrDefault(c => c.Value.NumberOfOptions == 2);

            // we only attempt brute force if we have a good shot
            if (firstCellWithTwoOptions == default)
            {
                return (false, puzzle);
            }

            // just pick the first option and try solve with that
            var options = firstCellWithTwoOptions.Value.GetOptions().ToList();

            var newCell = firstCellWithTwoOptions.SetValue(options[0]);
            var newPuzzle = puzzle.UpdateCell(newCell);

            var solver = new Solver(puzzle: newPuzzle, maxSteps: 100, logging: false);

            var (result, resultPuzzle) = solver.Solve();

            if (result == SolveResult.Success)
            {
                return (true, resultPuzzle);
            }
            else if (result == SolveResult.Invalid)
            {
                // it must be the other option
                var correctCell = firstCellWithTwoOptions.SetValue(options[1]);
                return (true, puzzle.UpdateCell(correctCell));
            }
            else
            {
                return (false, puzzle);
            }
        }
    }
}
