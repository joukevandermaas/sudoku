using System;
using System.Buffers;
using System.Collections.Generic;
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

        static int successes = 0;
        static int failures = 0;

        private (bool, Puzzle) AttemptBruteForce(in Puzzle puzzle, int depth)
        {
            if (depth > _maxBruteForceDepth)
            {
                return (false, puzzle);
            }

            var options = ArrayPool<SudokuValues>.Shared.Rent(Puzzle.LineLength);

            bool success = false;
            var endResult = puzzle;

            const int size = Puzzle.LineLength * Puzzle.LineLength;
            for (int i = 0; i < size; i++)
            {
                var cell = puzzle[i];

                var count = cell.Value.CopyValues(options);

                if (count != 2)
                {
                    continue;
                }

                var newCell = cell.SetValue(options[0]);
                var newPuzzle = puzzle.UpdateCell(newCell);

                var (result, resultPuzzle) = Solve(newPuzzle, depth + 1);

                if (result == SolveResult.Success)
                {
                    successes++;
                    success = true;
                    endResult = resultPuzzle;
                    break;
                }
                else if (result == SolveResult.Invalid)
                {
                    failures++;
                    // it must be the other option
                    success = true;
                    var correctCell = cell.SetValue(options[1]);
                    endResult = puzzle.UpdateCell(correctCell);

                    break;
                }
            }

            // didn't find any cell to brute force, or there
            // was no outcome
            ArrayPool<SudokuValues>.Shared.Return(options);

            return (success, endResult);
        }
    }
}
