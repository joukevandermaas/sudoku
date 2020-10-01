using System;
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
        private Puzzle _puzzle;
        private readonly ISolveStrategy[] _strategies;
        private readonly int _maxSteps;
        private readonly bool _logging;

        public Solver(Puzzle puzzle)
            : this(puzzle, 100, true)
        {
        }

        public Solver(Puzzle puzzle, int maxSteps, bool logging)
            : this(puzzle, maxSteps, logging, new ISolveStrategy[]
            { 
                new NakedSingleStrategy(),
                new SinglePositionStrategy(),
                new BruteForceStrategy()
            })
        {
        }

        public Solver(Puzzle puzzle, int maxSteps, bool logging, IEnumerable<ISolveStrategy> strategies)
        {
            _puzzle = puzzle;
            _strategies = strategies.ToArray();
            _maxSteps = maxSteps;
            _logging = logging;
        }

        public (SolveResult, Puzzle) Solve()
        {
            var actualSteps = 0;

            while (!_puzzle.IsSolved && !_puzzle.IsInvalid)
            {
                if (actualSteps >= _maxSteps)
                {
                    break;
                }

                var anySuccess = false;
                foreach (var strat in _strategies)
                {
                    var (success, newPuzzle) = strat.Apply(_puzzle);

                    if (_logging)
                    {
                        Console.WriteLine("Ran {0}. Result: {1}", strat.GetType(), success);
                    }

                    if (success)
                    {
                        anySuccess = true;
                        _puzzle = newPuzzle;
                        break;
                    }
                }

                actualSteps += 1;

                if (!anySuccess)
                {
                    break;
                }

                if (_logging)
                {
                    Console.WriteLine(_puzzle);
                }
            }

            SolveResult result;

            if (_puzzle.IsSolved)
            {
                result = SolveResult.Success;
                if (_logging)
                {
                    Console.WriteLine("Solved after {0} steps.", actualSteps);
                }
            }
            else
            {
                if (_logging)
                {
                    Console.WriteLine("Could not solve after {0} steps.", actualSteps);
                }

                result = _puzzle.IsInvalid ? SolveResult.Invalid : SolveResult.Failure;
            }

            return (result, _puzzle);
        }
    }
}
