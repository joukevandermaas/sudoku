using System;

namespace Sudoku
{
    class Solver
    {
        private Puzzle _puzzle;
        private ISolveStrategy[] _strategies = new[]
        {
            new NakedSingleStrategy(),
        };

        public Solver(Puzzle puzzle)
        {
            _puzzle = puzzle;
        }

        public Puzzle Solve()
        {
            const int maxSteps = 100;
            var actualSteps = 0;

            while (!_puzzle.IsSolved)
            {
                if (actualSteps >= maxSteps)
                {
                    break;
                }

                var anySuccess = false;
                foreach (var strat in _strategies)
                {
                    var (success, newPuzzle) = strat.Apply(_puzzle);

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

                Console.WriteLine(_puzzle);
            }

            if (_puzzle.IsSolved)
            {
                Console.WriteLine("Solved after {0} steps.", actualSteps);
            }
            else
            {
                Console.WriteLine("Could not solve after {0} steps.", actualSteps);
            }

            return _puzzle;
        }
    }
}
