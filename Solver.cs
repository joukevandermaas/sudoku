using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Solver()
            : this(maxSteps: 100, maxBruteForceDepth: 0)
        {
        }

        public Solver(int maxSteps, int maxBruteForceDepth)
            : this(maxSteps, maxBruteForceDepth, new ISolveStrategy[]
            {
                new NakedSingleStrategy(),
                new SinglePositionStrategy()
            })
        {
        }

        public Solver(int maxSteps, int maxBruteForceDepth, IEnumerable<ISolveStrategy> strategies)
        {
            _strategies = strategies.ToArray();
            _maxSteps = maxSteps;
            _maxBruteForceDepth = maxBruteForceDepth;
        }

        public (SolveResult, Puzzle) Solve(Puzzle puzzle)
        {
            return Solve(puzzle, 1);
        }

        private (SolveResult, Puzzle) Solve(Puzzle puzzle, int bruteForceDepth)
        {
            var actualSteps = 0;

            var watch = Stopwatch.StartNew();

            while (!puzzle.IsSolved && puzzle.IsValid)
            {
                if (actualSteps >= _maxSteps)
                {
                    break;
                }

                var anySuccess = false;
                foreach (var strat in _strategies)
                {
                    var (success, newPuzzle) = strat.Apply(puzzle);
                    puzzle = newPuzzle;

                    if (success)
                    {
                        anySuccess = true;
                        break;
                    }
                }

                actualSteps += 1;

                if (!anySuccess)
                {
                    var (success, newPuzzle) = AttemptBruteForce(puzzle, bruteForceDepth);
                    puzzle = newPuzzle;

                    if (!success)
                    {
                        break;
                    }
                }
            }

            watch.Stop();

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

        private (bool, Puzzle) AttemptBruteForce(Puzzle puzzle, int depth)
        {
            if (depth > _maxBruteForceDepth)
            {
                return (false, puzzle);
            }

            var cells = puzzle.Cells;

            foreach (var cell in cells)
            {
                var options = cell.Value.GetOptions().ToList();

                if (options.Count != 2)
                {
                    continue;
                }

                var newCell = cell.SetValue(options[0]);
                var newPuzzle = puzzle.UpdateCell(newCell);

                var (result, resultPuzzle) = Solve(newPuzzle, depth + 1);

                if (result == SolveResult.Success)
                {
                    return (true, resultPuzzle);
                }
                else if (result == SolveResult.Invalid)
                {
                    // it must be the other option
                    var correctCell = cell.SetValue(options[1]);
                    return (true, puzzle.UpdateCell(correctCell));
                }
            }

            // didn't find any cell to brute force, or there
            // was no outcome
            return (false, puzzle);
        }
    }
}
