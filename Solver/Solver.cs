using System.Collections.Generic;

namespace Sudoku
{
    public enum SolveResult
    {
        None,
        Success,
        Failure,
        Invalid
    }

    public class Solver
    {
        private readonly int _maxSteps;
        private readonly int _maxBruteForceDepth;
        private readonly int _currentBruteForceDepth;

        public Puzzle CurrentPuzzle { get; private set; }

        private readonly List<RegionQueue> _regionQueues;
        private readonly List<(ISolveStrategy strat, string name)> _strategies = new List<(ISolveStrategy, string)>
        {
            (new NakedSingleStrategy(), "digits"),
            (new HiddenSingleStrategy(), "hidden single"),
            (new BoxLayoutStrategy(), "box layout"),
            (new HiddenTupleStrategy(2), "hidden pair"),
            (new TupleStrategy(2), "pair"),
            (new FishStrategy(2), "xwing"),
            (new HiddenTupleStrategy(3), "hidden tripple"),
            (new TupleStrategy(3), "tripple"),
            (new HiddenTupleStrategy(4), "hidden quadruple"),
            (new TupleStrategy(4), "quadruple"),
            (new FishStrategy(3), "swordfish"),
        };

        public Solver(Puzzle puzzle, int maxSteps, int maxBruteForceDepth)
        {
            CurrentPuzzle = puzzle;
            _maxSteps = maxSteps;
            _maxBruteForceDepth = maxBruteForceDepth;
            _currentBruteForceDepth = 1;

            MaybeAddBruteForceStrategy();

            _regionQueues = new List<RegionQueue>(_strategies.Count);

            for (int i = 0; i < _strategies.Count; i++)
            {
                _regionQueues.Add(RegionQueue.GetFull());
            }
        }

        internal Solver(ChangeSet changeSet, Solver other)
        {
            CurrentPuzzle = changeSet.ApplyTo(other.CurrentPuzzle);
            _maxSteps = other._maxSteps;
            _maxBruteForceDepth = other._maxBruteForceDepth;
            _currentBruteForceDepth = other._currentBruteForceDepth + 1;

            MaybeAddBruteForceStrategy();

            _regionQueues = new List<RegionQueue>(_strategies.Count);

            for (int i = 0; i < _strategies.Count; i++)
            {
                var queue = other._regionQueues[i].Clone();
                changeSet.ApplyTo(queue);
                _regionQueues.Add(queue);
            }
        }

        private void MaybeAddBruteForceStrategy()
        {
            if (_maxBruteForceDepth > 0 && _currentBruteForceDepth < _maxBruteForceDepth)
            {
                _strategies.Add((new BruteForceStrategy(this), "brute force"));
            }
        }


        internal (SolveResult, Puzzle) Solve()
        {
            var actualSteps = 0;

            while (!CurrentPuzzle.IsSolved && CurrentPuzzle.IsValid)
            {
                if (actualSteps >= _maxSteps)
                {
                    break;
                }

                var (success, _) = Advance();

                if (!success)
                {
                    break;
                }
                actualSteps += 1;
            }

            SolveResult result;

            if (CurrentPuzzle.IsSolved)
            {
                result = SolveResult.Success;
            }
            else
            {
                result = !CurrentPuzzle.IsValid ? SolveResult.Invalid : SolveResult.Failure;
            }

            return (result, CurrentPuzzle);
        }

        public (bool success, string? method) Advance()
        {
            for (int i = 0; i < _strategies.Count; i++)
            {
                var (strat, name) = _strategies[i];
                var workQueue = _regionQueues[i];
#if DEBUG
                    Program.AddDebugText($"{name} is considering regions: {workQueue}");
#endif

                var changeSet = strat.Apply(CurrentPuzzle, workQueue);

                if (changeSet != ChangeSet.Empty)
                {
#if DEBUG
                    Program.AddDebugText($"<br>{name} changed regions: {changeSet}");
#endif

                    var newPuzzle = changeSet.ApplyTo(CurrentPuzzle);
                    CurrentPuzzle = newPuzzle;

                    for (int j = 0; j < _regionQueues.Count; j++)
                    {
                        changeSet.ApplyTo(_regionQueues[j]);
                    }

                    return (true, name);
                }
            }

            return (false, null);
        }
    }
}
