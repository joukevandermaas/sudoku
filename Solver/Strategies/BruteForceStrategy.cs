using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class BruteForceStrategy : ISolveStrategy
    {
        private Solver _solver;
        private int _currentBruteForceDepth = 0;

        public BruteForceStrategy(Solver solver)
        {
            _solver = solver;
        }

        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            if (_currentBruteForceDepth > _solver.MaxBruteForceDepth)
            {
                return ChangeSet.Empty;
            }

            var optionsArray = ArrayPool<int>.Shared.Rent(Puzzle.LineLength);

            while (unprocessedRegions.TryDequeue(puzzle, out var region))
            {
                var placedDigits = region.GetPlacedDigits();

                for (int i = 1; i < Puzzle.LineLength; i++)
                {
                    var value = SudokuValues.FromHumanValue(i);

                    if (placedDigits.HasAnyOptions(value))
                    {
                        continue;
                    }

                    var changeSet = EvaluateOptions(optionsArray, region, value);

                    if (changeSet != ChangeSet.Empty)
                    {
                        ArrayPool<int>.Shared.Return(optionsArray);

                        return changeSet;
                    }
                }
            }

            ArrayPool<int>.Shared.Return(optionsArray);
            return ChangeSet.Empty;
        }

        private ChangeSet EvaluateOptions(int[] optionsArray, Region region, SudokuValues value)
        {
            var options = region.GetPositions(value);
            var count = options.CopyIndices(optionsArray);

            if (count != 2)
            {
                return ChangeSet.Empty;
            }

            var newCell = region[optionsArray[0]].SetValue(value);

#if DEBUG
            Program.AddDebugText($"Attempting {newCell.Value} in R{newCell.Row + 1}C{newCell.Column + 1}");
#endif

            var changeSet = new ChangeSet(new List<Cell> { newCell });

            var currentSolver = _solver;
            var otherSolver = new Solver(changeSet, _solver);
            _solver = otherSolver;
            _currentBruteForceDepth += 1;

            var (result, resultPuzzle) = otherSolver.Solve();

            _currentBruteForceDepth -= 1;
            _solver = currentSolver;

#if DEBUG
            Program.AddDebugText($"Result: {result}");
#endif

            if (result == SolveResult.Success)
            {
                return new ChangeSet(resultPuzzle.Cells.ToList());
            }
            else if (result == SolveResult.Invalid)
            {
                // it must be the other option
                return new ChangeSet(new List<Cell> { region[optionsArray[1]].SetValue(value) });
            }

            return ChangeSet.Empty;
        }
    }
}
