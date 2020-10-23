using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class BruteForceStrategy2 : ISolveStrategy
    {
        private Solver _solver;
        private int _currentBruteForceDepth = 0;

        public BruteForceStrategy2(Solver solver)
        {
            _solver = solver;
        }

        public IChangeSet Apply(in Puzzle puzzle, RegionQueue changedRegions, SudokuValues changedDigits)
        {
            if (_currentBruteForceDepth >= _solver.MaxBruteForceDepth)
            {
                return new EmptyChangeSet();
            }

            var options = ArrayPool<SudokuValues>.Shared.Rent(Puzzle.LineLength);
            while (changedRegions.TryDequeue(puzzle, out var region))
            {
                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var cell = region[i];
                    var count = cell.CopyValues(options);
                    if (count != 2)
                    {
                        continue;
                    }

                    var changeSet = EvaluateOptions(options, region, i);

                    if (!changeSet.IsEmpty)
                    {
                        return changeSet;
                    }
                }
            }

            return new EmptyChangeSet();
        }

        private IChangeSet EvaluateOptions(SudokuValues[] options, Region region, int index)
        {
            var newCell = region.UpdateCell(index, options[0]);

#if DEBUG
            var coord = region.GetCoordinate(index);
            Program.AddDebugText($"Attempting {options[1]} in {coord}");
#endif

            var changeSet = new SingleCellChangeSet(newCell);

            var currentSolver = _solver;
            var otherSolver = new Solver(changeSet, _solver);
            _solver = otherSolver;
            _currentBruteForceDepth += 1;

            var (result, resultPuzzle) = otherSolver.Solve();

            _currentBruteForceDepth -= 1;
            _solver = currentSolver;

#if DEBUG
            Program.AddDebugText($"Result: {result}");
            Program.HighlightDigit = 0;
#endif

            if (result == SolveResult.Success)
            {
                var mutablePuzzle = resultPuzzle.AsMutable();
                mutablePuzzle.MarkAllAsChanged();
                return mutablePuzzle;
            }
            else if (result == SolveResult.Invalid)
            {
                // it must be the other option
                newCell = region.UpdateCell(index, options[1]);
                return new SingleCellChangeSet(newCell);
            }

            return new EmptyChangeSet();
        }
    }
}
