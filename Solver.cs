using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace Sudoku
{
    enum SolveResult
    {
        None,
        Success,
        Failure,
        Invalid
    }

    class Solver
    {
        private readonly ISolveStrategy[] _strategies;
        private readonly int _maxSteps;
        private readonly int _maxBruteForceDepth;

        public Solver(int maxSteps, int maxBruteForceDepth, IEnumerable<ISolveStrategy> strategies)
        {
            _strategies = strategies.ToArray();
            _maxSteps = maxSteps;
            _maxBruteForceDepth = maxBruteForceDepth;
        }

        public (SolveResult, Puzzle) Solve(in Puzzle puzzle)
        {
            return Solve(puzzle, 1);
        }

        private (SolveResult, Puzzle) Solve(Puzzle puzzle, int bruteForceDepth)
        {
            var actualSteps = 0;

            while (!puzzle.IsSolved && puzzle.IsValid)
            {
                if (actualSteps >= _maxSteps)
                {
                    break;
                }

                var (success, newPuzzle, _) = Advance(puzzle);
                puzzle = newPuzzle;

                if (!success)
                {
                    (success, newPuzzle) = AttemptBruteForce(puzzle, bruteForceDepth);
                    puzzle = newPuzzle;

                    if (!success)
                    {
                        break;
                    }
                }
                actualSteps += 1;
            }

            SolveResult result;

            if (puzzle.IsSolved)
            {
                result = SolveResult.Success;
            }
            else
            {
                result = !puzzle.IsValid ? SolveResult.Invalid : SolveResult.Failure;
            }

            return (result, puzzle);
        }

        public (bool, Puzzle, ISolveStrategy?) Advance(Puzzle puzzle)
        {
            foreach (var strat in _strategies)
            {
                var (success, newPuzzle) = strat.Apply(puzzle);
                puzzle = newPuzzle;

                if (success)
                {
                    return (true, puzzle, strat);
                }
            }

            return (false, puzzle, null);
        }

        private (bool, Puzzle) AttemptBruteForce(in Puzzle puzzle, int depth)
        {
            if (depth > _maxBruteForceDepth)
            {
                return (false, puzzle);
            }

            var optionsArray = ArrayPool<int>.Shared.Rent(Puzzle.LineLength);

            foreach (var region in puzzle.Regions)
            {
                var placedDigits = region.GetPlacedDigits();

                for (int i = 1; i < Puzzle.LineLength; i++)
                {
                    var value = SudokuValues.FromHumanValue(i);

                    if (placedDigits.HasAnyOptions(value))
                    {
                        continue;
                    }

                    var (success, newPuzzle) = EvaluateOptions(puzzle, depth, optionsArray, region, value);

                    if (success)
                    {
                        ArrayPool<int>.Shared.Return(optionsArray);

                        return (true, newPuzzle);
                    }
                }
            }

            ArrayPool<int>.Shared.Return(optionsArray);
            return (false, puzzle);
        }

        private (bool, Puzzle) EvaluateOptions(Puzzle puzzle, int depth, int[] optionsArray, Region region, SudokuValues value)
        {
            var options = region.GetPositions(value);
            var count = options.CopyIndices(optionsArray);

            if (count != 2)
            {
                return (false, puzzle);
            }

            var possiblePuzzle = puzzle.UpdateCell(region[optionsArray[0]].SetValue(value));

            var (result, resultPuzzle) = Solve(possiblePuzzle, depth + 1);

            if (result == SolveResult.Success)
            {
                return (true, resultPuzzle);
            }
            else if (result == SolveResult.Invalid)
            {
                // it must be the other option
                var newPuzzle = puzzle.UpdateCell(region[optionsArray[1]].SetValue(value));

                return (true, newPuzzle);
            }

            return (false, puzzle);
        }
    }
}
