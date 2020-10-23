﻿using System.Buffers;

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

        public IChangeSet Apply(in Puzzle puzzle, RegionQueue changedRegions, SudokuValues changedDigits)
        {
            if (_currentBruteForceDepth >= _solver.MaxBruteForceDepth)
            {
                return new EmptyChangeSet();
            }

            var optionsArray = ArrayPool<int>.Shared.Rent(Puzzle.LineLength);

            while (changedRegions.TryDequeue(puzzle, out var region))
            {
                var placedDigits = region.GetPlacedDigits();

                for (int i = 1; i < Puzzle.LineLength; i++)
                {
                    var value = SudokuValues.FromHumanValue(i);

                    if (placedDigits.HasAnyOptions(value) || !changedDigits.HasAnyOptions(value))
                    {
                        continue;
                    }

                    var changeSet = EvaluateOptions(optionsArray, region, value);

                    if (!changeSet.IsEmpty)
                    {
                        ArrayPool<int>.Shared.Return(optionsArray);

                        return changeSet;
                    }
                }
            }

            ArrayPool<int>.Shared.Return(optionsArray);
            return new EmptyChangeSet();
        }

        private IChangeSet EvaluateOptions(int[] optionsArray, Region region, SudokuValues value)
        {
            var options = region.GetPositions(value);
            var count = options.CopyIndices(optionsArray);

            if (count != 2)
            {
                return new EmptyChangeSet();
            }

            var newCell = region.UpdateCell(optionsArray[0], value.Invert());

#if DEBUG
            var coord = region.GetCoordinate(optionsArray[0]);
            Program.AddDebugText($"Attempting {value} in {coord}");
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
                newCell = region.UpdateCell(optionsArray[1], value.Invert());
                return new SingleCellChangeSet(newCell);
            }

            return new EmptyChangeSet();
        }
    }
}
